using Fenzwork.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace Fenzwork.Systems.Assets.Loaders
{
    public class AtlasMetadataLoader : AssetRawLoader
    {
        protected override void Load(Stream stream, AssetID assetID, out object resultAsset)
        {
            var result = new AtlasMetadata();
            var localDir = Path.GetDirectoryName(assetID.AssetName);

            var reader = new BinaryReader(stream);

            result.Atlases = new AssetRoot[reader.ReadInt32()];
            for (int a = 0; a < result.Atlases.Length; a++)
            {
                var atlasAssetName = reader.ReadString();
                result.Atlases[a] = AssetsManager.GetRoot($"{localDir}/{atlasAssetName}", typeof(Texture2D));
            }

            var spritesCount = reader.ReadInt32();
            result.Sprites = new Dictionary<AssetRoot, SpriteMetadata>(spritesCount);
            for (int s = 0; s < spritesCount; s++)
            {
                var spriteAssetRoot = AssetsManager.GetRoot(reader.ReadString(), typeof(Sprite));

                var spriteMetadata = new SpriteMetadata();
                spriteMetadata.AtlasId = reader.ReadInt32();
                spriteMetadata.X = reader.ReadInt32();
                spriteMetadata.Y = reader.ReadInt32();
                spriteMetadata.Width = reader.ReadInt32();
                spriteMetadata.Height = reader.ReadInt32();

                result.Sprites.Add(spriteAssetRoot, spriteMetadata);
            }

            resultAsset = result;
        }
    }
}
