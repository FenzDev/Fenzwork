using Fenzwork.Systems.Assets;
using Fenzwork.Systems.GUI.Components;
using System.Collections.Generic;
using System.Dynamic;

namespace Fenzwork.Systems.GUI
{
    public class GuiView : StackComponent, IDataEmbededAsset
    {
        public GuiRenderer Renderer { get; internal set; }
        public Dictionary<string, Bindable> VariablesContext { get; private set; } = [];

        public object PresistantData { get => VariablesContext; set => VariablesContext = (Dictionary<string,IBindable>)value; }
    
        public Bindable GetBind(string name)
        {
            if (VariablesContext.TryGetValue(name, out var bind))
                return bind;
            else
            {
                var newBind = new Bindable() { VariableName = name, Content = null };
                VariablesContext.Add(name, newBind);
                return newBind;
            }
        }
    }
}
