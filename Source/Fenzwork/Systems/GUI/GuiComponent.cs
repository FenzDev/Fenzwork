using Microsoft.Xna.Framework.Graphics;
using System.Xml;

namespace Fenzwork.Systems.GUI
{
    public abstract class GuiComponent
    {
        public bool IsVisible;

        protected internal virtual void Read(XmlReader reader) { }
    }
}
