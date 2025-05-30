using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets.Loaders
{
    //public class EffectLoader : AssetLoader
    //{
    //    public override string CategoryName { get; protected set; } = "Shaders";

    //    public override ImmutableArray<string> FileExtensions { get; } = ["mgfx"];

    //    public GraphicsDevice GraphicsDevice { get; }

    //    public override Effect DefaultObject { get; protected set; }

    //    public EffectLoader(GraphicsDevice gdevice)
    //    {
    //        GraphicsDevice = gdevice;
    //        DefaultObject = new SpriteEffect(gdevice);
    //    }

    //    byte[] ReadStreamBytes(Stream stream)
    //    {
    //        // Read the entire stream into a byte array
    //        byte[] fileBytes = new byte[stream.Length];
    //        stream.Read(fileBytes, 0, (int)stream.Length);
    //        return fileBytes;
    //    }

    //    public override Effect Load(string name, Stream stream)
    //    {
    //        return new Effect(GraphicsDevice, ReadStreamBytes(stream));
    //    }

    //    public override Effect Reload(Effect old, string name, Stream stream)
    //    {
    //        old.Dispose();
    //        return Load(name, stream);
    //    }
    //}
}
