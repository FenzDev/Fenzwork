using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

namespace Fenzwork.Systems.GUI
{
    public struct GuiRenderingBatch
    {
        public GuiGPUStates States;
        public readonly bool IsActive => States.IsActive;
        public readonly Asset<Texture2D> Texture => States.Texture;
        public readonly Asset<Effect> Effect => States.Effect;
        public readonly Rectangle? ClipMask => States.ClipMask;
        public 
        public readonly int IndeciesOffset;
        public readonly int VerticesOffset;
        public readonly int VerticesCount;
    }
}
