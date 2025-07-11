using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Xml;

namespace Fenzwork.Systems.GUI.Components
{
    public class RectangleComponent : GuiVisualPrimitiveComponent
    {
        public BindWrapper<int> X = 100;
        public BindWrapper<int> Y = 100;
        public BindWrapper<int> Width = 100;
        public BindWrapper<int> Height = 100;
        public BindWrapper<Color> Fill = Color.White;

        public int _FinalX;
        public int _FinalY;
        public int _FinalWidth;
        public int _FinalHeight;

        protected internal override void GenerateGeometry(out VertexPositionColorTexture[] vertices, out Func<ushort, ushort[]> indicesFactory, out int indicesCount, out GuiGPUStates states)
        {
            VertexPositionColorTexture TL = new(new(X, Y, 0f), Fill, new(0f, 0f));
            VertexPositionColorTexture TR = new(new(X + Width, Y, 0f), Fill, new(1f, 0f));
            VertexPositionColorTexture BL = new(new(X, Y + Height, 0f), Fill, new(0f, 1f));
            VertexPositionColorTexture BR = new(new(X + Width, Y + Height, 0f), Fill, new(1f, 1f));

            vertices = [TL, TR, BL, BR];
            indicesCount = 6;
            indicesFactory = (offset) => [
                offset,
                (ushort)(offset + 2),
                (ushort)(offset + 1),
                (ushort)(offset + 1),
                (ushort)(offset + 2),
                (ushort)(offset + 3) 
            ];

            states = new GuiGPUStates() { Texture = GuiRenderer.PixelTexture, Effect = GuiRenderer.SpriteEffect };
        }
        protected internal override void Read(XmlReader reader)
        {
            ParseIntBind(ref X, reader.GetAttribute("x"));
            ParseIntBind(ref Y, reader.GetAttribute("y"));
            ParseIntBind(ref Width, reader.GetAttribute("width"));
            ParseIntBind(ref Height, reader.GetAttribute("height"));
            ParseColorBind(ref Fill, reader.GetAttribute("fill"));

            X.OnChange += OnPropertyChanged;
            Y.OnChange += OnPropertyChanged;
            Width.OnChange += OnPropertyChanged;
            Height.OnChange += OnPropertyChanged;
            Fill.OnChange += OnPropertyChanged;
        }

        private void OnPropertyChanged(GuiComponent? componentSender) => MarkDirty();

        private void ParseIntBind(ref BindWrapper<int> field, string content)
        {
            if (TryGetBind(ref field, content))
                return;

            field = new BindWrapper<int>(new () { Content = int.Parse(content)});
        }
        private void ParseColorBind(ref BindWrapper<Color> field, string content)
        {
            if (TryGetBind(ref field, content))
                return;

            field = new BindWrapper<Color>(new () { Content = ParseHexColor(content)});
            
        }

        public static Color ParseHexColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Hex color string is null or empty.", nameof(hex));

            if (hex[0] == '#')
                hex = hex[1..]; // remove the '#'

            if (hex.Length != 6 && hex.Length != 8)
                throw new FormatException("Hex color must be in format #RRGGBB or #RRGGBBAA.");

            byte r = Convert.ToByte(hex[0..2], 16);
            byte g = Convert.ToByte(hex[2..4], 16);
            byte b = Convert.ToByte(hex[4..6], 16);
            byte a = (hex.Length == 8) ? Convert.ToByte(hex[6..8], 16) : (byte)255;

            return new Color(r, g, b, a);
        }

    }
}
