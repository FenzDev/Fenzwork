using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;

namespace Fenzwork.Systems.GUI
{
    public abstract class GuiComponent
    {
        public GuiParent Parent { get; internal set; }

        protected internal Rectangle Bounds;

        protected internal GuiView View;

        public int Level { get; internal set; }
        public BindWrapper<bool> IsVisible { get; set; } = true;

        protected internal virtual void Read(XmlReader reader) { }

        protected bool TryGetBindFromContext<T>(ref BindWrapper<T> property, string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            if (content[0] == '{' && content.Length > 2 && content[1] != '{' && content[^2] != '}' && content[^1] == '}')
            {
                if (View.TryGetBind(content[1..^1].Trim(), out var bind))
                {
                    property = bind;
                    return true;
                }
            }

            return false;
        }
    }
}
