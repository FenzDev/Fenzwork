using Fenzwork.Graphics;
using Fenzwork.Systems.Assets;
using FontStashSharp.Interfaces;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

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

            SpriteEffect = new Asset<Effect>(".guiRenderer:SpriteEffect", new SpriteEffect(gD) {  });
            var pix = new Texture2D(gD, 1, 1); pix.SetData([Color.White]);
            PixelTexture = new Asset<Texture2D>(".guiRenderer:PixelTexture", pix);
        }

        private VertexPositionColorTexture[] _VertexArray = new VertexPositionColorTexture[65536];
        private DynamicVertexBuffer _GpuVertexBuffer;

        private ushort[] _IndexArray = new ushort[98304];
        private DynamicIndexBuffer _GpuIndexBuffer;

        private List<GuiRenderingBatch> _Batches = new (1024);

        private int _VerticesEndCursor = 0;
        private int _IndicesEndCursor = 0;

        public void Render()
        {
            GD.SetVertexBuffer(_GpuVertexBuffer);
            GD.Indices = _GpuIndexBuffer;
            
            GD.DepthStencilState = DepthStencilState.None;
            GD.SamplerStates[0] = SamplerState.PointClamp;
            GD.BlendState = BlendState.AlphaBlend;
            
            for (int b = 0; b < _Batches.Count; b++)
            {
                if (!_Batches[b].IsActive)
                    continue;

                if (_Batches[b].ClipMask.HasValue)
                {
                    GD.ScissorRectangle = _Batches[b].ClipMask.Value;
                }
                else
                {
                    GD.ScissorRectangle = GD.Viewport.Bounds;
                }
                GD.RasterizerState = new RasterizerState() { ScissorTestEnable = false, CullMode = CullMode.CullClockwiseFace };

                for (int p = 0; p < _Batches[b].Effect.Content.CurrentTechnique.Passes.Count; p++)
                {
                    _Batches[b].Effect.Content.CurrentTechnique.Passes[p].Apply();
                    GD.Textures[0] = _Batches[b].Texture;
                    GD.DrawIndexedPrimitives(PrimitiveType.TriangleList, _Batches[b].VerticesOffset, _Batches[b].IndicesOffset, _Batches[b].IndicesCount / 3);
                }
            }

        }

        public void TryExpandBatchVI(bool isVertexNotIndex, int batchIndex, ushort expensionAmount)
        {
            if (expensionAmount <= 0)
                return;

            var batch = _Batches[batchIndex];

            int deltaCapacity;
            if (isVertexNotIndex)
            {
                deltaCapacity = batch.VerticesCapacity << expensionAmount - 1;
                batch.VerticesCapacity <<= expensionAmount;
            }
            else
            {
                deltaCapacity = batch.IndicesCapacity << expensionAmount - 1;
                batch.IndicesCapacity <<= expensionAmount;
            }

            if (batchIndex < _Batches.Count - 1)
                ShiftBatchesAfter(isVertexNotIndex, batchIndex, batch, deltaCapacity);

            if (isVertexNotIndex)
                _VerticesEndCursor += deltaCapacity;
            else
                _IndicesEndCursor += deltaCapacity;
        }
        public void TryShrinkBatchVI(bool isVertexNotIndex, int batchIndex, ushort shrinkingAmount)
        {
            if (shrinkingAmount <= 0)
                return;

            var batch = _Batches[batchIndex];

            int deltaCapacity;
            if (isVertexNotIndex)
            {
                deltaCapacity = batch.VerticesCapacity >> shrinkingAmount - 1;
                batch.VerticesCapacity >>= shrinkingAmount;
            }
            else
            {
                deltaCapacity = batch.IndicesCapacity >> shrinkingAmount - 1;
                batch.IndicesCapacity >>= shrinkingAmount;
            }

            if (batchIndex < _Batches.Count - 1)
                ShiftBatchesAfter(isVertexNotIndex, batchIndex, batch, deltaCapacity);

            if (isVertexNotIndex)
                _VerticesEndCursor += deltaCapacity;
            else
                _IndicesEndCursor += deltaCapacity;
        }

        private void ShiftBatchesAfter(bool isVertexNotIndex, int batchIndex, GuiRenderingBatch batch, int amount)
        {
            int srcArrayStart, srcArrayEnd;
            if (isVertexNotIndex)
            {
                srcArrayStart = batch.VerticesOffset + amount;
                srcArrayEnd = _VerticesEndCursor;

                var vertexSpan = _VertexArray.AsSpan();
                vertexSpan[srcArrayStart..srcArrayEnd]
                    .CopyTo(vertexSpan[(srcArrayStart + amount)..]);
            }
            else
            {
                srcArrayStart = batch.IndicesOffset + amount;
                srcArrayEnd = _IndicesEndCursor;

                var indexSpan = _IndexArray.AsSpan();
                indexSpan[srcArrayStart..srcArrayEnd]
                    .CopyTo(indexSpan[(srcArrayStart + amount)..]);
            }

            for (int i = batchIndex + 1; i < _Batches.Count; i++)
            {
                if (isVertexNotIndex)
                    _Batches[i].VerticesOffset += amount;
                else
                    _Batches[i].IndicesOffset += amount;
            }
        }

        private void TryResizeBatchVI(bool isVertexNotIndex, int batchIndex, GuiRenderingBatch batch, int deltaCapacity)
        {
            TryExpandBatchVI(
                isVertexNotIndex,
                batchIndex,
                (ushort)(isVertexNotIndex
                    ? (batch.VerticesCount + deltaCapacity - 1) / batch.VerticesCapacity
                    : (batch.VerticesCount + deltaCapacity - 1) / batch.IndicesCapacity
                )
            );
            TryShrinkBatchVI(
                isVertexNotIndex,
                batchIndex,
                (ushort)(isVertexNotIndex
                    ? (batch.VerticesCount + deltaCapacity - 1) / batch.VerticesCapacity
                    : (batch.VerticesCount + deltaCapacity - 1) / batch.IndicesCapacity
                )
            );
        }

        private void TryResizeComponentVI(bool isVertexNotIndex, int batchIndex, GuiRenderingBatch batch, int newCapacity, GuiVisualComponent component)
        {
            // if new capacity is out of the interval [(1/4)-1] of old capacity then don't resize (return)
            if ((isVertexNotIndex && (component._VCapacity > newCapacity - 1 || (component._VCapacity >> 2) < newCapacity - 1))
                || (!isVertexNotIndex && (component._ICapacity > newCapacity - 1 || (component._ICapacity >> 2) < newCapacity - 1)))
                return;

            int deltaCapacity;
            if (isVertexNotIndex)
            {
                deltaCapacity = newCapacity - component._VCapacity;
                component._VCapacity = newCapacity;
            }
            else
            {
                deltaCapacity = newCapacity - component._ICapacity;
                component._ICapacity = newCapacity;
            }

            // this will either expand or shrink the batch capacity as well as shift the batches after if possible
            TryResizeBatchVI(isVertexNotIndex, batchIndex, batch, deltaCapacity);

            for (int c = component._IndexInBatch + 1; c < batch.Components.Count; c++)
            {
                if (isVertexNotIndex)
                    batch.Components[c]._VRelOffset += deltaCapacity;
                else
                    batch.Components[c]._IRelOffset += deltaCapacity;
            }
        }


        public void CleanDirtyComponent(GuiVisualComponent component)
        {
            component.GenerateGeometry(out VertexPositionColorTexture[] vertices, out Func<int, ushort[]> indicesFactory, out int indicesCount, out GuiGPUStates gpuStateBatch);

            int batchIndex = -1;

            // if this component doesn't belong to any batch 
            if (component._RenderingBatch == null)
            {
                bool foundBatch = false;
                for (int b = 0; b < _Batches.Count; b++)
                {
                    if (!_Batches[b].IsActive)
                        continue;

                    // continue if selected batch's gpu changes/states is not similar as the one the component has
                    if (!gpuStateBatch.Equals(_Batches[b].States))
                        continue;

                    // add it to the batch
                    component._RenderingBatch = _Batches[b];
                    batchIndex = b;
                    foundBatch = true;
                    break;
                }

                // if not found batch create one !
                if (!foundBatch)
                {
                    component._RenderingBatch = new()
                    {
                        States = gpuStateBatch,
                        VerticesOffset = _VerticesEndCursor,
                        IndicesOffset = _VerticesEndCursor,
                    };
                    batchIndex = _Batches.Count;
                    _Batches.Add(component._RenderingBatch);
                }

                component._IndexInBatch = _Batches[batchIndex].Components.Count;
                component._RenderingBatch.Components.Add(component);
            }

            if (component._IRelOffset == -1 /*|| component._VRelOffset == -1*/)
            {
                component._IRelOffset = component._RenderingBatch.IndicesCount;
                component._VRelOffset = component._RenderingBatch.VerticesCount;
            }


            var indices = indicesFactory(component._VRelOffset);
            _Batches[batchIndex].VerticesCount += vertices.Length - component._VCapacity;
            _Batches[batchIndex].IndicesCount += indices.Length - component._ICapacity;

            // Expand vertices capacity if overflown
            TryResizeComponentVI(isVertexNotIndex: true, batchIndex, component._RenderingBatch, vertices.Length, component);
            TryResizeComponentVI(isVertexNotIndex: false, batchIndex, component._RenderingBatch, indicesCount, component);

            // Copy the new vertices and indicesArray
            vertices.AsSpan()
                .CopyTo(_VertexArray.AsSpan(_Batches[batchIndex].VerticesOffset + component._VRelOffset));
            indices.AsSpan()
                .CopyTo(_IndexArray.AsSpan(_Batches[batchIndex].IndicesOffset + component._IRelOffset));

            //TODO: Take only the changed
            _GpuVertexBuffer.SetData(_VertexArray, 0, _VerticesEndCursor);
            _GpuIndexBuffer.SetData(_IndexArray, 0, _IndicesEndCursor);
        }
    }
}
