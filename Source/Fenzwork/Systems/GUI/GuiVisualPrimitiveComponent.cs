using Microsoft.Xna.Framework.Graphics;
using System;

namespace Fenzwork.Systems.GUI
{
    public abstract class GuiVisualPrimitiveComponent : GuiComponent
    {
        internal int _IndeciesRelativeOffset;
        internal int _GPUBufferIndeciesCount;
        internal int _GPUBufferVerticesOffset;
        internal int _GPUBufferVerticesCount;
        internal int _ReneringBufferIndex;
        protected internal abstract void GenerateGeometry(out VertexPositionColorTexture[] vertices, out Func<ushort, ushort[]> indicesFactory, out int indicesCount, out GuiGPUStates batch);

        protected internal void MarkDirty() => View.Renderer.MarkDirty(this);
    }
}
