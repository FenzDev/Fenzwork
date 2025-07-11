using Microsoft.Xna.Framework.Graphics;
using System.Xml;

namespace Fenzwork.Systems.GUI
{
    public abstract class GuiComponent
    {
        public GuiParent Parent { get; internal set; }

        protected GuiView View;

        public int Level { get; internal set; }
        public BindWrapper<bool> IsVisible { get; set; }

        protected internal virtual void Read(XmlReader reader) { }

        protected bool TryGetBind<T>(ref BindWrapper<T> field, string content)
        {
            if (content[0] == '{' && content.Length > 2 && content[1] != '{' && content[^2] != '}' && content[^1] == '}')
            {
                field = View.GetBind(content[1..^1].Trim());
                return true;
            }
            return false;
        }
    }
}
