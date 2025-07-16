using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fenzwork.Systems.GUI
{
    public class GuiRenderingBatch
    {
        public GuiGPUStates States;
        public bool IsActive => States.IsActive;
        public Asset<Texture2D> Texture => States.Texture;
        public Asset<Effect> Effect => States.Effect;
        public Rectangle? ClipMask => States.ClipMask;
        public List<GuiVisualComponent> Components = [];
        public int IndicesOffset;
        public int IndicesCount;
        public int IndicesCapacity = 6;
        public int VerticesOffset;
        public int VerticesCount;
        public int VerticesCapacity = 4;
    }

}
