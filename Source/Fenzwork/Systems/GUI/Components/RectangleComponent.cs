using Microsoft.Xna.Framework.Graphics;
using System.Xml;

namespace Fenzwork.Systems.GUI.Components
{
    public class RectangleComponent : GuiVisualPrimitiveComponent
    {
        public int X { get; set; } = 100;
        public int Y { get; set; } = 100;
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;

        protected internal override void GenerateGeometry(out VertexPositionColorTexture[] vertices, out ushort[] indices, out GuiBatch metadata)
        {
            vertices = new VertexPositionColorTexture[4];
            indices = new ushort[6];
            metadata = new GuiBatch();
        }

        protected internal override void Read(XmlReader reader)
        {
            var xStr = reader.GetAttribute("x");
            var yStr = reader.GetAttribute("y");
            var widthStr = reader.GetAttribute("width");
            var heightStr = reader.GetAttribute("height");

            if (xStr != null)
                X = int.Parse(xStr);
            if (yStr != null)
                Y = int.Parse(yStr);
            if (widthStr != null)
                Width = int.Parse(widthStr);
            if (yStr != null)
                Height = int.Parse(heightStr);
        }

    }
}
