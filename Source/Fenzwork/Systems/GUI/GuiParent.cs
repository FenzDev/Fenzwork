using System.Collections.Generic;

namespace Fenzwork.Systems.GUI
{
    public class GuiParent : GuiComponent
    {
        public List<GuiComponent> Children = [];

        protected void ResetGeometry()
        {
            
        }
    }
}
