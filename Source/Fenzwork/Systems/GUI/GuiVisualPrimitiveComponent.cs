using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.GUI
{
    public abstract class GuiVisualPrimitiveComponent : GuiComponent
    {
        protected internal abstract void GenerateGeometry(out VertexPositionColorTexture[] vertices, out ushort[] indices, out GuiBatch metadata);

    }
}
