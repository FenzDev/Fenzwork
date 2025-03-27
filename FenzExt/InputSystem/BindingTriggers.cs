using Microsoft.Xna.Framework.Input;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt.InputSystem
{
    internal class BindingTriggers
    {
        public BindingTriggers(KeyCombination key0, KeyCombination key1 = default, Buttons gamepad = Buttons.None, MouseTriggers mouse = MouseTriggers.None)
        {

        }

        public KeyCombination Key0;
        public KeyCombination Key1;
        public Buttons Gamepad;
        public MouseTriggers Mouse;
    }
}
