﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

namespace Fenzwork.Systems.GUI
{
    public abstract class GuiVisualComponent : GuiComponent
    {
        protected GuiVisualComponent()
        {
            IsVisible.OnChange += IsVisible_Changed;
             MarkDirty();
        }

        private void IsVisible_Changed(GuiComponent componentSender) => MarkDirty();

        internal int _IndexInBatch;
        internal GuiRenderingBatch _RenderingBatch;
        internal int _IRelOffset = -1;
        internal int _ICapacity;
        internal int _VRelOffset = -1;
        internal int _VCapacity;

        protected internal abstract void GenerateGeometry(out VertexPositionColorTexture[] vertices, out Func<int, ushort[]> indicesFactory, out GuiGPUStates batch);

        protected internal void MarkDirty() => View?.Renderer.CleanDirtyComponent(this);
    }
}
