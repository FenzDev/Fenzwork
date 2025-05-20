using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Input
{
    public class ControlBinding
    {
        public string Name { get; internal set; }
        internal BindingTriggers _Trigger;

        internal bool _PreviousState;
        internal bool _State;
        internal float _Value;

        public bool IsDown => _State;
        public bool IsUp => !_State;
        public bool IsJustPressed => _State && !_PreviousState;
        public bool IsJustReleased => !_State && _PreviousState;
        public float Value => _Value;

    }
}
