using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Fenzwork.Systems.Assets;

namespace Fenzwork.Graphics
{
    public class AtlasMetadata : IDisposable
    {
        public AssetRoot[] Atlases { get; internal set; }
        public Dictionary<AssetRoot, SpriteMetadata> Sprites { get; internal set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                Atlases = null;
                Sprites = null;
                GC.SuppressFinalize(this);
            }
        }

        ~AtlasMetadata() => Dispose(false);
    }
    public record struct SpriteMetadata(int AtlasId, int X, int Y, int Width, int Height);
}
