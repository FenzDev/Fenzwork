using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt.AssetsSystem.Loaders
{
    public class TextureLoader : AssetLoader<Texture2D>
    {
        public override string CategoryName { get; protected set; } = "Textures";

        private const string _DefaultBinary = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAl0lEQVQ4jaWSwRHDMAgENx41QmO0YtMKjamU5JGRLGvAjib3FbfigNfB8eYPFQBFEWTJWKk4/gWMZsNujTs7o6eMj4b1gkxzzbZibh2MXZascI6SwTtA0cf8twBBUBQAx9MfDet1F0CDZGrdzSvfMkOk6F7CIUbzyI4tBEQzyNa8FCHSUoQUUKmXfII8XmWlngDHf2551gfW6CxxPQoxKAAAAABJRU5ErkJggg==";

        public GraphicsDevice GraphicsDevice { get; }

        public TextureLoader(GraphicsDevice gdevice)
        {
            GraphicsDevice = gdevice;
            using (var stream = new MemoryStream(Convert.FromBase64String(_DefaultBinary)))
            {
                DefaultObject = Texture2D.FromStream(gdevice, stream);
            }
        }

        public override ImmutableArray<string> FileExtensions { get; } = ["png"];

        public override Texture2D DefaultObject { get; protected set; }

        public override Texture2D Load(string name, Stream stream)
        {
            var tex = Texture2D.FromStream(GraphicsDevice, stream);
            tex.Name = name;

            return tex;
        }

        public override Texture2D Reload(Texture2D old, string name, Stream stream)
        {
            old.Reload(stream); return old;
        }
    }
}
