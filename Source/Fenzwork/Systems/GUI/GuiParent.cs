using System.Collections.Generic;

namespace Fenzwork.Systems.GUI
{
    public class GuiParent : GuiComponent
    {
        public List<GuiVisualPrimitiveComponent> Children = [];

        protected void ResetGeometry()
        {
            
            throw new System.NotImplementedException();
        }
    }
}
