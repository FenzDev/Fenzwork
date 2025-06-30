using Fenzwork.GenLib.Models;
using RectpackSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.GenLib
{
    public record  SpriteFileInfo(int Width, int Height, DateTime Timestamp);
    
    public class AtlasPacker
    {
        FileSpriteNode[] SpriteNodes;
        Dictionary<string, SpriteFileInfo> SpriteCache = new();
        FolderSpriteNode Sprites = new();
        List<Atlas> Atlases = [];
        public string WorkingDir;
        public string CacheFilePath;
        public AtlasConfig Config;

        public void Begin()
        {
            // We read sprites cache if there is
            if (File.Exists(CacheFilePath))
                ReadSpritesCache(File.OpenRead(CacheFilePath));

            foreach (var file in Directory.EnumerateFiles(Path.Combine(WorkingDir, Config.Folder), $"{Config.AtlasNamePrefix}*.png"))
                File.Delete(file);
        }

        public void AddPack(string fileName, string fullPath)
        {
            Size pngSize = GetPngFileSize(fullPath);

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
                currentNode.TotalAreaWithPadding += (uint)(newSpriteNode.TotalArea + Config.SpritePadding * (Config.SpritePadding + pngSize.Width + pngSize.Height));

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
            // First we set an array of sprites folders
            Span<PackingRectangle> spritesRectangles = new PackingRectangle[Sprites.TotalFilesNumber];
            // We also set array which allows to build the atlas metadata
            SpriteNodes = new FileSpriteNode[Sprites.TotalFilesNumber];
            // Then we sort at in an ascending order based on TotalArea
            Sort(Sprites);
            // Then we fill both the rects span and metadata array ('SpriteNodes') from 'Sprites'
            Fill(Sprites, spritesRectangles);
            // We Attempt to pack the atlases which reorganize the rectangles
            Pack(Sprites, spritesRectangles, 0);
            // We generate all the atlasses with metadata file
            Generate(spritesRectangles);

        }

        void Generate(Span<PackingRectangle> spritesRectangles)
        {
            var outputFolder = Path.Combine(WorkingDir, Config.Folder);
            Directory.CreateDirectory(outputFolder);
            using var metadataWriter = new BinaryWriter( File.OpenWrite(Path.Combine(outputFolder, Config.MetadataName)) );


            metadataWriter.Write(spritesRectangles.Length);
            for (int a = 0; a < Atlases.Count; a++)
            {
                var atlas = new SKBitmap(Atlases[a].BoundsWidth, Atlases[a].BoundsHeight);
                using var canvas = new SKCanvas(atlas);
                canvas.Clear(SKColors.Transparent);

                for (int i = Atlases[a].Start; i <= Atlases[a].End; i++)
                {
                    var sprite = SpriteNodes[spritesRectangles[i].Id];
                    metadataWriter.Write(sprite.Name);
                    metadataWriter.Write(spritesRectangles[i].X);
                    metadataWriter.Write(spritesRectangles[i].Y);
                    metadataWriter.Write(spritesRectangles[i].Width - Config.SpritePadding);
                    metadataWriter.Write(spritesRectangles[i].Height - Config.SpritePadding);

                    var spriteBitmap = SKBitmap.Decode(sprite.FullName);

                    canvas.DrawBitmap(spriteBitmap, spritesRectangles[i].X, spritesRectangles[i].Y);
                }

                using var atlasPng = File.OpenWrite(Path.Combine(outputFolder, $"{Config.AtlasNamePrefix}{a}.png"));
                atlas.Encode(SKEncodedImageFormat.Png, 0).SaveTo(atlasPng);

            }
            metadataWriter.BaseStream.SetLength(metadataWriter.BaseStream.Position);
        }

        void Pack(FolderSpriteNode folderNode, Span<PackingRectangle> packingRectangles, int offest, uint totalAreaWithPadding = 0, int subFoldersUsedCount = -1)
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

            try
            {
                // compare the total area (plus padding) with max size squared
                //  if we found that its greater then we throw an error
                if (totalAreaWithPadding > Config.MaxTextureSize * Config.MaxTextureSize)
                    throw new Exception();

                // then we attempt to generate the atlas.
                PackAtlas(packingRectangles, offest);
            }
            catch
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

                    Pack(lastSubFolder, packingRectangles[^lastSubFolder.TotalFilesNumber..], packingRectangles.Length - lastSubFolder.TotalFilesNumber, lastSubFolder.TotalAreaWithPadding, lastSubFolder.SubFolders.Count);
                    if (packingRectangles.Length - lastSubFolder.TotalFilesNumber != 0)
                    Pack(folderNode, packingRectangles[..^lastSubFolder.TotalFilesNumber], offest, totalAreaWithPadding - lastSubFolder.TotalAreaWithPadding, subFoldersUsedCount - 1);
                }
            }
        }

        void SplitAndPackSprites(Span<PackingRectangle> packingRectangles, int offest, uint totalAreaWithPadding)
        {
            if (packingRectangles.Length == 0)
                return;

            // 1- There is only one file which is bigger then defined max, simply we throw an error
            if (packingRectangles.Length == 1)
                throw new Exception("One sprite is too big for the atlas.");
            // 2- There are no folders but only files more than one,
            else
            {
                // slice the span into two, both sides should have approx equal area
                uint areaCounter = 0;
                int i = packingRectangles.Length - 1;
                while (areaCounter < (totalAreaWithPadding / 2) && i >= 0)
                {
                    areaCounter += packingRectangles[i].Area;
                    i--;
                }

                if (i <= 0)
                    PackAtlas(packingRectangles, offest);
                else
                {
                    TryPackSprites(packingRectangles[..^i], offest, totalAreaWithPadding - areaCounter);
                    TryPackSprites(packingRectangles[^i..], packingRectangles.Length - i, areaCounter);
                }

            }
        }

        void PackAtlas(Span<PackingRectangle> packingRectangles, int offest)
        {
            RectanglePacker.Pack(packingRectangles, out var bounds, maxBoundsWidth: Config.MaxTextureSize, maxBoundsHeight: Config.MaxTextureSize);
            Atlases.Add(new Atlas(offest, offest + packingRectangles.Length - 1, (int)bounds.Width, (int)bounds.Height));
            offest += packingRectangles.Length;
        }

        void TryPackSprites(Span<PackingRectangle> packingRectangles, int offest, uint totalAreaWithPadding)
        {
            try
            {
                PackAtlas(packingRectangles, offest);
            }
            catch
            {
                SplitAndPackSprites(packingRectangles, offest, totalAreaWithPadding);
            }
        }

        static Comparison<ISpriteNode> AreaComparer => (a, b) => a.TotalArea.CompareTo(b.TotalArea);
        void Sort(FolderSpriteNode folderNode)
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
            WriteSpritesCache(File.OpenWrite(CacheFilePath));

            // clear some fields
            SpriteCache.Clear();
            Sprites = new();
        }

        Size GetPngFileSize(string fullPath)
        {
            var currentTimestamp = File.GetLastWriteTime(fullPath);
            
            if (SpriteCache.TryGetValue(fullPath, out var pngWeHave))
            {
                if (currentTimestamp > pngWeHave.Timestamp)
                    return ReadPngFileSize(fullPath, currentTimestamp, false);

                return new Size(pngWeHave.Width, pngWeHave.Height);
            }

            return ReadPngFileSize(fullPath, currentTimestamp, true);
        }
        
        Size ReadPngFileSize(string fullPath, DateTime timeStamp, bool add)
        {
            using var file = File.OpenRead(fullPath);
            var newPngSize = GetPngStreamSize(file);

            SpriteCache[fullPath] = new(newPngSize.Width, newPngSize.Height, timeStamp);

            return new Size(newPngSize.Width, newPngSize.Height);
        }

        Size GetPngStreamSize(Stream stream)
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
                var lastWriteTime = new DateTime(reader.ReadInt64());
                var width = reader.ReadInt32();
                var height = reader.ReadInt32();
                SpriteCache.Add(path, new (width, height, lastWriteTime));
            }
        }
        void WriteSpritesCache(Stream stream)
        {
            using var writer = new BinaryWriter(stream);

            writer.Write(SpriteCache.Count);
            foreach (var pngStamp in SpriteCache)
            {
                writer.Write(pngStamp.Key);
                writer.Write(pngStamp.Value.Timestamp.Ticks);
                writer.Write(pngStamp.Value.Width);
                writer.Write(pngStamp.Value.Height);
            }
        }
    }

    public record struct Atlas(int Start, int End, int BoundsWidth, int BoundsHeight);
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
