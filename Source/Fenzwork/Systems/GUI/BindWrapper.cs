using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.GUI
{
    public delegate void BindableEventHandler(GuiComponent? componentSender);

    public sealed class Bindable
    {
        public string? VariableName { get; init; }
        public object Content { get; internal set; }
        public event BindableEventHandler OnChange;
        public void ChangeValue(GuiComponent? componentSender, object newValue) 
        {
            if (newValue.Equals(Content))
                return;

            Content = newValue;
            OnChange(componentSender);
        }
    }

    public class BindWrapper<T>
    {
        private readonly Bindable _Bindable;

        public BindWrapper(Bindable bindable)
        {
            _Bindable = bindable;
            bindable.OnChange += OnChange;
        }

        public Bindable Bindable => _Bindable;

        public string VariableName => _Bindable.VariableName;

        public T Content { get => (T)Bindable.Content; internal set => Bindable.Content = value; }

        public event BindableEventHandler OnChange;

        public void ChangeValue(GuiComponent? componentSender, T newValue) => _Bindable.ChangeValue(componentSender, newValue);

        public static implicit operator BindWrapper<T>(Bindable instance) => new BindWrapper<T>(instance);
        public static implicit operator BindWrapper<T>(T instance) => new Bindable() { Content = instance };
        public static implicit operator T(BindWrapper<T> instance) => instance.Content;

    }
}
