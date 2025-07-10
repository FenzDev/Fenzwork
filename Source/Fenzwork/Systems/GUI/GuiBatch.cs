using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Fenzwork.Systems.GUI
{
    public struct GuiBatch
    {
        public bool IsDrawable;

        public Asset<Texture2D> Texture;
        public Asset<Effect> Effect;
        public Rectangle? ClipMask;
        public int VerticesOffest;
        public ushort VerticesCount;
        public int IndicesOffest;
        public bool IsText;
    }
}
