using Fenzwork.GenLib.Models;
using RectpackSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.GenLib
{
    public record struct SpriteFileInfo(string Name, int Width, int Height, DateTime Timestamp, bool DontInclude);
    public enum AtlasUpdateType
    {
        // 00
        None = 0,
        // 01
        Soft = 1,
        // 11  (was set to 3 because when doing `PendingUpdate |= AtlasUpdateType.Soft;` this wont effect it
        Hard = 3
    }

    public class AtlasPacker
    {
        FileSpriteNode[] SpriteNodes;
        Dictionary<string, SpriteFileInfo> SpriteCache = [];
        List<SpriteFileInfo> PendingSpritesForSoftUpdate = [];
        FolderSpriteNode Sprites = new();
        List<Atlas> Atlases = [];
        public string WorkingDir;
        public string SpritesCacheFilePath;
        public AtlasConfig Config;
        public AtlasUpdateType PendingUpdate;

        public void Begin()
        {
            // We read sprites cache if there is
            if (File.Exists(SpritesCacheFilePath))
                ReadSpritesCache(File.OpenRead(SpritesCacheFilePath));

        }

        public void AddSprite(string fileName, string fullPath)
        {
            Size pngSize = FetchSpriteInfo(fileName, fullPath);

            var fileNameSplitted = fileName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var currentNode = Sprites;
            var newSpriteNode = new FileSpriteNode()
            {
                Name = fileName,
                FullName = fullPath,
                Size = new(pngSize.Width, pngSize.Height)
            };
            for (var i = 0; i < fileNameSplitted.Length; i++)
            {
                currentNode.TotalFilesNumber++;
                currentNode.TotalArea += newSpriteNode.TotalArea;
                currentNode.TotalAreaWithPadding += (long)(newSpriteNode.TotalArea + Config.SpritePadding * (Config.SpritePadding + pngSize.Width + pngSize.Height));

                if (i == fileNameSplitted.Length - 1)
                {
                    currentNode.Files.Add(newSpriteNode);
                    return;
                }

                var folderNode = currentNode.SubFolders.Find(node => node.Name == fileNameSplitted[i]);
                if (folderNode != null)
                    currentNode = folderNode;
                else
                {
                    var newFolderNode = new FolderSpriteNode() { Name = fileNameSplitted[i] };
                    currentNode.SubFolders.Add(newFolderNode);
                    currentNode = newFolderNode;
                }
            }

        }
        
        public void Generate()
        {
            if (Sprites.TotalFilesNumber != SpriteCache.Count)
                AskForHardUpdate();
            else
                FetchAndTrySoftUpdate();

            if (PendingUpdate == AtlasUpdateType.Hard)
                HardUpdate();
        }

        void HardUpdate()
        {
            var atlasFolder = string.IsNullOrEmpty(Config.PackInto) ? WorkingDir : Path.Combine(WorkingDir, Config.PackInto);

            foreach (var file in Directory.EnumerateFiles(atlasFolder, $"{Config.AtlasNamePrefix}*.png"))
                File.Delete(file);

            // First we allocate an array of rectangles
            Span<PackingRectangle> spritesRectangles = stackalloc PackingRectangle[Sprites.TotalFilesNumber];
            // We also set array which allows to build the atlas metadata
            SpriteNodes = new FileSpriteNode[Sprites.TotalFilesNumber];

            // - S.F.P.R.G - //
            // We sort the sprites tree in an ascending order based on TotalArea
            Sort(Sprites);
            // Then we fill both the rects span and metadata array ('SpriteNodes') from 'Sprites'
            Fill(Sprites, spritesRectangles);
            // We Attempt to pack the atlases which reorganize the rectangles 
            Pack(Sprites, spritesRectangles, 0);
            // We try to reduce the num of atlases by searching for small ones and repack them into one
            Reduce(spritesRectangles);
            // We generate all the atlases with metadata file
            Generate(spritesRectangles);
        }

        void FetchAndTrySoftUpdate()
        {
            try
            {
                var dir = string.IsNullOrEmpty(Config.PackInto)? WorkingDir: Path.Combine(WorkingDir, Config.PackInto);
                var metadataFilePath = Path.Combine(dir, Config.MetadataName);
            
                if (!File.Exists(metadataFilePath))
                {
                    AskForHardUpdate();
                    return;
                }

                using var metadata = new BinaryReader(File.OpenRead(metadataFilePath));

                var atlasesCount = metadata.ReadInt32();
                var atlases = new string[atlasesCount];
                for (int a = 0; a < atlasesCount; a++)
                {
                    var atlasName = metadata.ReadString();

                    if (!File.Exists(Path.Combine(dir, atlasName)))
                    {
                        AskForHardUpdate();
                        return;
                    }
                }

                if (PendingUpdate == AtlasUpdateType.Soft)
                    SoftUpdate(metadata);
            }
            catch
            {
                AskForHardUpdate();
            }
        }

        void SoftUpdate(BinaryReader metadata)
        {
            var atlasFileNamePrefix = string.IsNullOrEmpty(Config.PackInto) 
                ? Path.Combine(WorkingDir, Config.AtlasNamePrefix)
                : Path.Combine(WorkingDir, Config.PackInto, Config.AtlasNamePrefix);
            var spritesCount = metadata.ReadInt32();
            for (int i = 0; i < spritesCount; i++)
            {
                var name = metadata.ReadString();
                
                var pendingSpriteIndex = PendingSpritesForSoftUpdate.FindIndex(spriteInfo => spriteInfo.Name == name);
                if (pendingSpriteIndex == -1)
                {
                    metadata.BaseStream.Seek(20 /* 5*sizeof(int/uint) */, SeekOrigin.Current);
                    continue;
                }
                var atlasIndex = metadata.ReadInt32();
                var x = metadata.ReadUInt32();
                var y = metadata.ReadUInt32();
                metadata.BaseStream.Seek(8, SeekOrigin.Current); // skip width and height

                DrawOver(Path.Combine(WorkingDir, name), $"{atlasFileNamePrefix}{atlasIndex}.png", x, y);

                PendingSpritesForSoftUpdate.RemoveAt(pendingSpriteIndex);
                if (PendingSpritesForSoftUpdate.Count == 0)
                    return;
            }
        }

        static void DrawOver(string spritePath, string atlasPath, uint x, uint y)
        {
            // Load atlas png
            using var atlasStream = File.OpenRead(atlasPath);
            using var atlasBitmap = SKBitmap.Decode(atlasStream);

            // Load sprite png
            using var spriteStream = File.OpenRead(spritePath);
            using var spriteBitmap = SKBitmap.Decode(spriteStream);

            // Create surface from atlas image
            using var surface = SKSurface.Create(new SKImageInfo(atlasBitmap.Width, atlasBitmap.Height));
            var canvas = surface.Canvas;

            // Draw atlas image
            canvas.DrawBitmap(atlasBitmap, SKPoint.Empty);

            // Draw sprite image at a specific position
            canvas.DrawBitmap(spriteBitmap, x, y);

            // Save result
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var outputStream = File.OpenWrite(atlasPath);
            data.SaveTo(outputStream);
        }

        void Reduce(Span<PackingRectangle> spritesRectangles)
        {
            //Debugger.Launch();
            for (int a = 0; a < Atlases.Count; a++)
            {
                for (int aOther = a + 1; aOther < Atlases.Count; aOther++)
                {
                    if (TryMergedPack(spritesRectangles, a, aOther))
                        aOther--;
                }
            }
        }
        
        bool TryMergedPack(Span<PackingRectangle> mainSpritesRectangles, int thisAtlas, int otherAtlas)
        {
            // if the sum of the areas ocuppied by the two atlases are over the max bounds logically that aint gonna pack it more
            if (Atlases[thisAtlas].TotalAreaWithPadding + Atlases[otherAtlas].TotalAreaWithPadding > Config.MaxTextureSize * Config.MaxTextureSize)
                return false;

            Span<PackingRectangle> tempAtlasRectangles = stackalloc PackingRectangle[Atlases[thisAtlas].RectanglesCount + Atlases[otherAtlas].RectanglesCount];

            int tempOffest, offest, length;

            // Copy from this atlas and append it to temp
            tempOffest = 0;
            for (int r = 0; r < Atlases[thisAtlas].Ranges.Count; r++)
            {
                var range = Atlases[thisAtlas].Ranges[r];
                offest = range.Start;
                length = range.End - range.Start + 1;
                CopyRectangles(mainSpritesRectangles, tempAtlasRectangles, offest, tempOffest, length);

                tempOffest += length;
            }
            // Copy from other atlas and append it to temp
            offest = Atlases[otherAtlas].Ranges[0].Start;
            length = Atlases[otherAtlas].Ranges[0].End - offest + 1;
            CopyRectangles(mainSpritesRectangles, tempAtlasRectangles, offest, tempOffest, length);

            // Try pack if it did not work return
            if (!PackAtlas(tempAtlasRectangles, 0, -1, out var packedBounds, false))
                return false;

            // Copy from packed temp and put into this atlas' ranges
            tempOffest = 0;
            for (int r = 0; r < Atlases[thisAtlas].Ranges.Count; r++)
            {
                var range = Atlases[thisAtlas].Ranges[r];
                offest = range.Start;
                length = range.End - range.Start + 1;
                CopyRectangles(tempAtlasRectangles, mainSpritesRectangles, tempOffest, offest, length);

                tempOffest += length;
            }

            // Copy from packed temp and put into other atlas range
            offest = Atlases[otherAtlas].Ranges[0].Start;
            length = Atlases[otherAtlas].Ranges[0].End - offest + 1;
            CopyRectangles(tempAtlasRectangles, mainSpritesRectangles, tempOffest, offest, length);

            // Merge the other atlas into this atlas
            Atlases[thisAtlas].Ranges.Add((offest, offest + length - 1));
            Atlases[thisAtlas] = new Atlas( Atlases[thisAtlas].Ranges,
                                            tempAtlasRectangles.Length,
                                            Atlases[thisAtlas].TotalAreaWithPadding + Atlases[otherAtlas].TotalAreaWithPadding,
                                            (int)packedBounds.Width,
                                            (int)packedBounds.Height );
            // Remove the other atlas from the atlas list (we have reduced one atlas)
            Atlases.RemoveAt(otherAtlas);

            // return true so that we tell it we have removed an atlas
            return true;
        }

        static void CopyRectangles(Span<PackingRectangle> from, Span<PackingRectangle> into, int fromOffest, int intoOffest, int length)
        {
            from.Slice(fromOffest, length)
                .CopyTo(into.Slice(intoOffest, length));
        }

        void Generate(Span<PackingRectangle> spritesRectangles)
        {
            var outputFolder = string.IsNullOrEmpty(Config.PackInto) ? WorkingDir: Path.Combine(WorkingDir, Config.PackInto);
            Directory.CreateDirectory(outputFolder);
            using var metadataWriter = new BinaryWriter( File.OpenWrite(Path.Combine(outputFolder, Config.MetadataName)) );

            metadataWriter.Write(Atlases.Count);
            for (int a = 0; a < Atlases.Count; a++)
                metadataWriter.Write($"{Config.AtlasNamePrefix}{a}.png");
            metadataWriter.Write(spritesRectangles.Length);
            for (int a = 0; a < Atlases.Count; a++)
            {
                var atlas = new SKBitmap(Atlases[a].BoundsWidth, Atlases[a].BoundsHeight);
                using var canvas = new SKCanvas(atlas);
                canvas.Clear(SKColors.Transparent);

                var atlasFileName = $"{Config.AtlasNamePrefix}{a}.png";

                for (int r = 0; r < Atlases[a].Ranges.Count; r++)
                {
                    for (int i = Atlases[a].Ranges[r].Start; i <= Atlases[a].Ranges[r].End; i++)
                    {
                        var sprite = SpriteNodes[spritesRectangles[i].Id];
                        metadataWriter.Write(sprite.Name);
                        metadataWriter.Write(a);
                        metadataWriter.Write(spritesRectangles[i].X);
                        metadataWriter.Write(spritesRectangles[i].Y);
                        metadataWriter.Write((uint)(spritesRectangles[i].Width - Config.SpritePadding));
                        metadataWriter.Write((uint)(spritesRectangles[i].Height - Config.SpritePadding));

                        var spriteBitmap = SKBitmap.Decode(sprite.FullName);

                        canvas.DrawBitmap(spriteBitmap, spritesRectangles[i].X, spritesRectangles[i].Y);
                    }
                }

                using var atlasPng = File.OpenWrite(Path.Combine(outputFolder, atlasFileName));
                atlas.Encode(SKEncodedImageFormat.Png, 0).SaveTo(atlasPng);

            }
            metadataWriter.BaseStream.SetLength(metadataWriter.BaseStream.Position);
        }

        void Pack(FolderSpriteNode folderNode, Span<PackingRectangle> packingRectangles, int offest, long totalAreaWithPadding = 0, int subFoldersUsedCount = -1)
        {
            // This Pack method aims to auto categorize atlasses based on folder to reduce draw calls 
            // supposing that game systems used will try to use only those certain folders
            // but at same time gather best amount of folder if possible within bounds.
            // How does the method work:
            // First we TRY:
            //   comparing the total area (plus padding) with max size squared
            //     if we found that its greater then we throw an error (that will be caught later)
            //   then we attempt to generate the atlas.
            //   if no error simply save that atlas into a file and ofc sign the metadata
            // if exception was CAUGHT There are three cases to do:
            //   1- There is only one file which is bigger then defined max, simply we throw an error
            //   2- There are no folders but only files more than one,
            //      slice the span into two, both sides should have approx equal area
            //   3- There is atleast one folder,
            //       we do two recursions:
            //        - with slice of span from a folder from the right (which is the folder with most area)
            //        - with slice of span from the rest

            if (subFoldersUsedCount == -1)
                subFoldersUsedCount = folderNode.SubFolders.Count;
            if (totalAreaWithPadding == 0)
                totalAreaWithPadding = folderNode.TotalAreaWithPadding;

            // if there is no solution given the bounds
            if (totalAreaWithPadding > Config.MaxTextureSize * Config.MaxTextureSize
                || !PackAtlas(packingRectangles, offest, totalAreaWithPadding))
            {
                // if exception was CAUGHT There are three cases to do:
                if (subFoldersUsedCount == 0)
                {
                    //   1- There is only one file which is bigger then defined max, simply we throw an error
                    //   2- There are no folders but only files more than one,
                    //      slice the span into two, both sides should have approx equal area
                    SplitAndPackSprites(packingRectangles, offest, folderNode.TotalAreaWithPadding);
                }
                // 3- There is atleast one folder
                else
                {
                    // we do two recursions:
                    //  - with slice of span from a folder from the right (which is the folder with most area)
                    //  - with slice of span from the rest

                    var lastSubFolder = folderNode.SubFolders[subFoldersUsedCount - 1];

                    if (packingRectangles.Length - lastSubFolder.TotalFilesNumber != 0)
                        Pack(folderNode, packingRectangles[..^lastSubFolder.TotalFilesNumber], offest, totalAreaWithPadding - lastSubFolder.TotalAreaWithPadding, subFoldersUsedCount - 1);
                    Pack(lastSubFolder, packingRectangles[^lastSubFolder.TotalFilesNumber..], offest + packingRectangles.Length - lastSubFolder.TotalFilesNumber, lastSubFolder.TotalAreaWithPadding, lastSubFolder.SubFolders.Count);
                }
            }
        }

        void SplitAndPackSprites(Span<PackingRectangle> packingRectangles, int offest, long totalAreaWithPadding)
        {
            if (packingRectangles.Length == 0)
                return;

            // 1- There is only one file which is bigger then defined max, simply we throw an error
            // 2- There are no folders but only files more than one,
            // slice the span into two, both sides should have approx equal area
            long areaCounter = 0;
            int i = packingRectangles.Length - 1;
            while (areaCounter < (totalAreaWithPadding / 2) && i >= 0)
            {
                areaCounter += packingRectangles[i].Area;
                i--;
            }

            if (i <= 0)
                PackAtlas(packingRectangles, offest, totalAreaWithPadding);
            else
            {
                TryPackSprites(packingRectangles[..^i], offest, totalAreaWithPadding - areaCounter);
                TryPackSprites(packingRectangles[^i..], offest + packingRectangles.Length - i, areaCounter);
            }
        }

        bool PackAtlas(Span<PackingRectangle> packingRectangles, int offest, long totalArea, out PackingRectangle bounds, bool addToList = true)
        {
            if (!RectanglePacker.Pack(packingRectangles, out bounds, maxBoundsWidth: Config.MaxTextureSize, maxBoundsHeight: Config.MaxTextureSize))
                return false;
            if (addToList)
                Atlases.Add(new Atlas([(offest, offest + packingRectangles.Length - 1)], packingRectangles.Length, totalArea, (int)bounds.Width, (int)bounds.Height));
            return true;
        }

        bool PackAtlas(Span<PackingRectangle> packingRectangles, int offest, long totalArea, bool addToList = true)
            => PackAtlas(packingRectangles, offest, totalArea, out var _, addToList);

        void TryPackSprites(Span<PackingRectangle> packingRectangles, int offest, long totalAreaWithPadding)
        {
            if (!PackAtlas(packingRectangles, offest, totalAreaWithPadding))
                SplitAndPackSprites(packingRectangles, offest, totalAreaWithPadding);
        }

        static Comparison<ISpriteNode> AreaComparer => (a, b) => a.TotalArea.CompareTo(b.TotalArea);
        static void Sort(FolderSpriteNode folderNode)
        {
            folderNode.Files.Sort(AreaComparer);
            folderNode.SubFolders.Sort(AreaComparer);
            for (int i = 0; i < folderNode.SubFolders.Count; i++)
            {
                Sort(folderNode.SubFolders[i]);
            }
        }

        int _IDCounter;
        void Fill(FolderSpriteNode folderNode, Span<PackingRectangle> packingRectangles)
        {
            for (int i = 0; i < folderNode.Files.Count; i++)
            {
                var size = folderNode.Files[i].Size;
                SpriteNodes[_IDCounter] = folderNode.Files[i];
                packingRectangles[i] = new PackingRectangle(new(0, 0, size.Width + Config.SpritePadding, size.Height + Config.SpritePadding), _IDCounter++);
            }
            var filesCounter = 0;
            for (int i = 0; i < folderNode.SubFolders.Count; i++)
            {
                Fill(folderNode.SubFolders[i], packingRectangles[(folderNode.Files.Count + filesCounter)..]);
                filesCounter += folderNode.SubFolders[i].TotalFilesNumber;
            }
        }

        public void End()
        {
            // We write the sprites cache
            WriteSpritesCache(File.OpenWrite(SpritesCacheFilePath));

            // clear some fields
            SpriteCache.Clear();
            Sprites = new();
        }

        private Size FetchSpriteInfo(string name, string fullPath)
        {
            Size pngSize;
            var currentTimestamp = File.GetLastWriteTime(fullPath);
            // We use the cache to reduce the amount of files reading overhead
            if (SpriteCache.TryGetValue(fullPath, out var storedInfo))
            {
                if (storedInfo.Timestamp != currentTimestamp)
                {
                    pngSize = ReadPngSize(fullPath, currentTimestamp);
                    if (pngSize.Width == storedInfo.Width && pngSize.Height == storedInfo.Height)
                        AskForSoftUpdate(storedInfo);
                    else
                        AskForHardUpdate();
                    SpriteCache[fullPath] = new(name, pngSize.Width, pngSize.Height, currentTimestamp, false);
                }
                else
                    pngSize = new(storedInfo.Width, storedInfo.Height);

            }
            else
            {
                AskForHardUpdate();

                pngSize = ReadPngSize(fullPath, currentTimestamp);
                var spriteInfo = new SpriteFileInfo(name, pngSize.Width, pngSize.Height, currentTimestamp, false);
                SpriteCache.Add(fullPath, spriteInfo);
            }

            return pngSize;
        }

        void AskForSoftUpdate(SpriteFileInfo sprite)
        {
            PendingSpritesForSoftUpdate.Add(sprite);
            PendingUpdate |= AtlasUpdateType.Soft;
        }
        void AskForHardUpdate()
        {
            PendingUpdate = AtlasUpdateType.Hard;
        }

        Size ReadPngSize(string fullPath, DateTime timeStamp)
        {
            using var file = File.OpenRead(fullPath);
            var newPngSize = ReadPngSizeFromStream(file);

            return new Size(newPngSize.Width, newPngSize.Height);
        }

        Size ReadPngSizeFromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);

            stream.Seek(16, SeekOrigin.Begin); // Skip 8-byte signature + 8-byte IHDR header (length + type)

            int width = ReadInt32BigEndian(reader);
            int height = ReadInt32BigEndian(reader);

            return (width, height);
        }

        int ReadInt32BigEndian(BinaryReader reader)
        {
            // stackalloc fast boi
            Span<byte> buffer = stackalloc byte[4];
            reader.BaseStream.ReadExactly(buffer);
            return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
        }

        void ReadSpritesCache(Stream stream)
        {
            using var reader = new BinaryReader(stream);

            var num = reader.ReadInt32();
            for (var i = 0; i < num; i++)
            {
                var path = reader.ReadString();
                var name = reader.ReadString();
                var lastWriteTime = new DateTime(reader.ReadInt64());
                var width = reader.ReadInt32();
                var height = reader.ReadInt32();
                SpriteCache.Add(path, new (name, width, height, lastWriteTime, true));
            }
        }
        void WriteSpritesCache(Stream stream)
        {
            using var writer = new BinaryWriter(stream);

            writer.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);
            var removedCounter = 0;
            foreach (var pngStamp in SpriteCache)
            {
                if (pngStamp.Value.DontInclude)
                {
                    removedCounter++;
                    continue;
                }
                writer.Write(pngStamp.Key);
                writer.Write(pngStamp.Value.Name);
                writer.Write(pngStamp.Value.Timestamp.Ticks);
                writer.Write(pngStamp.Value.Width);
                writer.Write(pngStamp.Value.Height);
            }
            // write the actual sprites count
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            writer.Write(SpriteCache.Count- removedCounter);
        }
    }

    public record struct Atlas(List<(int Start, int End)> Ranges, int RectanglesCount, long TotalAreaWithPadding, int BoundsWidth, int BoundsHeight);
    public record struct Size(int Width, int Height)
    {
        public static implicit operator (int Width, int Height)(Size value)
        {
            return (value.Width, value.Height);
        }

        public static implicit operator Size((int Width, int Height) value)
        {
            return new Size(value.Width, value.Height);
        }
    }
}
