using Fenzwork.Graphics;
using Fenzwork.Systems.Assets;
using FontStashSharp.Interfaces;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Fenzwork.Systems.GUI
{
    public sealed class GuiRenderer
    {
        public readonly GraphicsDevice GD;
        public static Asset<Effect> SpriteEffect { get; private set; }
        public static Asset<Texture2D> PixelTexture { get; private set; }

        public GuiRenderer(GraphicsDevice gD)
        {
            GD = gD;
            _GpuIndexBuffer = new DynamicIndexBuffer(gD, IndexElementSize.SixteenBits, _IndexArray.Length, BufferUsage.WriteOnly);
            _GpuVertexBuffer = new DynamicVertexBuffer(gD, VertexPositionColorTexture.VertexDeclaration, _VertexArray.Length, BufferUsage.WriteOnly);

            SpriteEffect = new Asset<Effect>(".guiRenderer:SpriteEffect", new SpriteEffect(gD));
            PixelTexture = new Asset<Texture2D>(".guiRenderer:PixelTexture", new Texture2D(gD, 1, 1));

        }

        private VertexPositionColorTexture[] _VertexArray = new VertexPositionColorTexture[65536];
        private DynamicVertexBuffer _GpuVertexBuffer;

        private ushort[] _IndexArray = new ushort[98304];
        private DynamicIndexBuffer _GpuIndexBuffer;

        private GuiRenderingBatch[] Batches = new GuiRenderingBatch[4096];

        private int _VerticesEndCursor = 0;
        private int _IndicesEndCursor = 0;
        private int _BatchesEndCursor = 0;

        public void Render()
        {
            for (int b = 0; b < _BatchesEndCursor; b++)
            {
                if (!Batches[b].IsActive)
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
                GD.DrawIndexedPrimitives(PrimitiveType.TriangleList, Batches[b].VerticesOffset, Batches[b].IndicesOffest, Batches[b].VerticesCount);
            }

        }

        public void MarkDirty(GuiVisualPrimitiveComponent component)
        {
            component.GenerateGeometry(out VertexPositionColorTexture[] vertices, out var indicesFactory, out var indicesCount, out var gpuStateBatch);
            // if this component doesn't belong to any batch 
            if (component._ReneringBufferIndex == -1)
            {
                for (int b = 0; b < _BatchesEndCursor; b++)
                {
                    if (!Batches[b].IsActive)
                        continue;

                    // continue if selected batch is not similar as the one the component has
                    if (!gpuStateBatch.Equals(Batches[b].States))
                        continue;

                    // add it to the batch
                    component._ReneringBufferIndex = b;
                    break;
                }
            }

        }

    }
}
