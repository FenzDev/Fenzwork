using Fenzwork.Graphics;
using FontStashSharp.Interfaces;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fenzwork.Systems.GUI
{
    public sealed class GuiRenderer
    {
        private GraphicsDevice GD;

        public GuiRenderer(GraphicsDevice gD)
        {
            GD = gD;
            _GpuIndexBuffer = new DynamicIndexBuffer(gD, IndexElementSize.SixteenBits, _IndexArray.Length, BufferUsage.WriteOnly);
            _GpuVertexBuffer = new DynamicVertexBuffer(gD, VertexPositionColorTexture.VertexDeclaration, _VertexArray.Length, BufferUsage.WriteOnly);
        }

        private VertexPositionColorTexture[] _VertexArray = new VertexPositionColorTexture[65536];
        private DynamicVertexBuffer _GpuVertexBuffer;

        private ushort[] _IndexArray = new ushort[98304];
        private DynamicIndexBuffer _GpuIndexBuffer;

        private GuiBatch[] Batches = new GuiBatch[4096];

        private int _UsedVertexCount = 0;
        private int _UsedIndexCount = 0;
        private int _UsedBatchesCount = 0;

        public void Render()
        {
            for (int b = 0; b < Batches.Length; b++)
            {
                if (!Batches[b].IsDrawable)
                    continue;

                if (Batches[b].ClipMask.HasValue)
                {
                    GD.RasterizerState.ScissorTestEnable = true;
                    GD.ScissorRectangle = Batches[b].ClipMask.Value;
                }
                else
                {
                    GD.RasterizerState.ScissorTestEnable = false;
                    GD.ScissorRectangle = GD.Viewport.Bounds;
                }

                GD.Textures[0] = Batches[b].Texture;
                GD.SetVertexBuffer(_GpuVertexBuffer);
                GD.Indices = _GpuIndexBuffer;
                GD.DrawIndexedPrimitives(PrimitiveType.TriangleList, Batches[b].VerticesOffest, Batches[b].IndicesOffest, Batches[b].VerticesCount);
            }

        }



    }
}
