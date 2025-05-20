using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fenzwork.Systems.Input
{
    /// <summary>
    /// Provides centralized input state tracking and binding registration for various input devices.
    /// </summary>
    public static class Input
    {
        // Previouse states
        static MouseState _PreviousMouseState;
        static KeyboardState _PreviousKeyboardState;
        static GamePadState?[] _PreviousGamepadStates = new GamePadState?[4];
        static TouchCollection _PreviousTouches;


        /// <summary>
        /// Gets the current state of the mouse.
        /// </summary>
        public static MouseState MouseState { get; private set; }

        /// <summary>
        /// Gets the current state of the keyboard.
        /// </summary>
        public static KeyboardState KeyboardState { get; private set; }

        /// <summary>
        /// Gets the current states of the gamepads. Array indexes correspond to player indices.
        /// </summary>
        public static GamePadState?[] GamepadStates { get; private set; } = new GamePadState?[4];

        /// <summary>
        /// Gets the current touch panel state.
        /// </summary>
        public static TouchCollection Touches { get; private set; }

        /// <summary>
        /// Gets the list of registered control bindings.
        /// </summary>
        public static List<ControlBinding> Bindings { get; } = new List<ControlBinding>();

        /// <summary>
        /// The threshold for gamepad thumbstick activation.
        /// </summary>
        public static float GamepadThreshold = 0.5f;

        /// <summary>
        /// Gets or sets the variation mode for gamepad thumbstick value calculation.
        /// </summary>
        public static GamepadThumbValueVariation ThumbValueVariation = GamepadThumbValueVariation.Default;

        /// <summary>
        /// Registers a new control binding with the specified parameters.
        /// </summary>
        /// <param name="name">The unique name for the binding.</param>
        /// <param name="key0">The primary key combination.</param>
        /// <param name="key1">The secondary key combination (optional).</param>
        /// <param name="gamepad">The gamepad button trigger (optional).</param>
        /// <param name="mouse">The mouse trigger (optional).</param>
        /// <returns>The registered control binding, or null if a binding with the given name already exists.</returns>
        public static ControlBinding RegisterBinding(string name, KeyCombination key0, KeyCombination key1 = default, Buttons gamepad = Buttons.None, MouseTriggers mouse = MouseTriggers.None)
        {
            if (!Bindings.Exists(bind => bind.Name == name))
            {
                var binding = new ControlBinding() { Name = name, _Trigger = new BindingTriggers(key0, key1, gamepad, mouse) };
                Bindings.Add(binding);

                return binding;
            }
            return null; // Note: returns null if the binding already exists or after registration.
        }

        /// <summary>
        /// Unregisters the control binding with the specified name.
        /// </summary>
        /// <param name="name">The name of the binding to unregister.</param>
        public static void UnregisterBinding(string name)
        {
            int index = Bindings.FindIndex(bind => bind.Name == name);
            if (index != -1)
            {
                Bindings.RemoveAt(index);
            }
        }

        // Internal methods are not exposed publicly, so minimal comments are provided.

        // Initializes the input system.
        internal static void Init()
        {
            // Initialization code can be added here.
        }

        // Updates the input states for mouse, keyboard, gamepad, and touch.
        internal static void Update(GameTime gameTime)
        {
            // assigning previous states
            _PreviousKeyboardState = KeyboardState;
            _PreviousMouseState = MouseState;
            GamepadStates.CopyTo(_PreviousGamepadStates, 0);
            _PreviousTouches = Touches;

            MouseState = Mouse.GetState();
            KeyboardState = Keyboard.GetState();

            // Tick gamepad states for up to 4 players.
            for (int i = 0; i < GamepadStates.Length; i++)
            {
                if (GamePad.GetCapabilities(i).IsConnected)
                    GamepadStates[i] = GamePad.GetState(i);
                else
                    GamepadStates[i] = null;
            }

            Touches = TouchPanel.GetState();

            // Tick all registered control bindings.
            foreach (var bind in Bindings)
            {
                bind._PreviousState = bind._State;
                _UpdateState(bind);
            }
        }

        // Updates the state of an individual control binding.
        static void _UpdateState(ControlBinding binding)
        {
            var t = binding._Trigger;

            // Process gamepad triggers.
            foreach (var gpdNullable in GamepadStates)
            {
                if (!gpdNullable.HasValue)
                    continue;

                var gpd = gpdNullable.Value;

                _ApplyThumbstickState(binding, gpd.ThumbSticks, t.Gamepad);
                if (binding._State)
                    break;

                binding._State = gpd.IsButtonDown(t.Gamepad);
                if (binding._State)
                    break;
            }

            _ApplyKeyboardState(binding, t.Key0);
            if (binding._State)
                return;

            _ApplyKeyboardState(binding, t.Key1);
            if (binding._State)
                return;

            _ApplyMouseState(binding, t.Mouse);
        }

        // Applies thumbstick input to the binding based on the given gamepad axis.
        static void _ApplyThumbstickState(ControlBinding binding, GamePadThumbSticks thumbSticks, Buttons axis)
        {
            switch (axis)
            {
                case Buttons.LeftThumbstickLeft:
                    _ApplyThumbstickAxisState(binding, thumbSticks.Left.X);
                    break;
                case Buttons.LeftThumbstickRight:
                    _ApplyThumbstickAxisState(binding, -thumbSticks.Left.X);
                    break;
                case Buttons.LeftThumbstickDown:
                    _ApplyThumbstickAxisState(binding, thumbSticks.Left.Y);
                    break;
                case Buttons.LeftThumbstickUp:
                    _ApplyThumbstickAxisState(binding, -thumbSticks.Left.Y);
                    break;
                case Buttons.RightThumbstickLeft:
                    _ApplyThumbstickAxisState(binding, thumbSticks.Right.X);
                    break;
                case Buttons.RightThumbstickRight:
                    _ApplyThumbstickAxisState(binding, -thumbSticks.Right.X);
                    break;
                case Buttons.RightThumbstickDown:
                    _ApplyThumbstickAxisState(binding, thumbSticks.Right.Y);
                    break;
                case Buttons.RightThumbstickUp:
                    _ApplyThumbstickAxisState(binding, -thumbSticks.Right.Y);
                    break;
            }
        }

        // Applies thumbstick axis state based on a directional value.
        static void _ApplyThumbstickAxisState(ControlBinding binding, float directPos)
        {
            if (directPos < 0)
                return;

            binding._Value = 0f;

            if (ThumbValueVariation == GamepadThumbValueVariation.Default)
                binding._Value = directPos;

            if (directPos >= GamepadThreshold)
            {
                switch (ThumbValueVariation)
                {
                    case GamepadThumbValueVariation.ZeroOnDeadZone:
                        binding._Value = directPos;
                        break;
                    case GamepadThumbValueVariation.FromThershold:
                        binding._Value = (directPos - GamepadThreshold) / (1f - GamepadThreshold);
                        break;
                    case GamepadThumbValueVariation.Staircase:
                        binding._Value = 1f;
                        break;
                }
                binding._State = true;
            }
        }

        // Applies keyboard input to the binding for the specified key combination.
        static void _ApplyKeyboardState(ControlBinding binding, KeyCombination combination)
        {
            if (combination.Key == Keys.None)
                return;

            // Check if required modifier keys are down.
            var modifiersState = true;

            if ((combination.Modifiers & KeyModifiers.Ctrl) == KeyModifiers.Ctrl)
                modifiersState &= KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);

            if ((combination.Modifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
                modifiersState &= KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);

            if ((combination.Modifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
                modifiersState &= KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);

            binding._State = modifiersState && KeyboardState.IsKeyDown(combination.Key);
        }

        // Applies mouse input to the binding based on the specified trigger.
        static void _ApplyMouseState(ControlBinding binding, MouseTriggers trigger)
        {

            if (MouseState.ScrollWheelValue > 0)
            {

            }
            switch (trigger)
            {
                case MouseTriggers.Left:
                    binding._State = MouseState.LeftButton == ButtonState.Pressed;
                    break;
                case MouseTriggers.Middle:
                    binding._State = MouseState.MiddleButton == ButtonState.Pressed;
                    break;
                case MouseTriggers.Right:
                    binding._State = MouseState.RightButton == ButtonState.Pressed;
                    break;
                case MouseTriggers.ScrollUp:
                    binding._State = MouseState.ScrollWheelValue - _PreviousMouseState.ScrollWheelValue > 0;
                    break;
                case MouseTriggers.ScrollDown:
                    binding._State = MouseState.ScrollWheelValue - _PreviousMouseState.ScrollWheelValue < 0;
                    break;
                case MouseTriggers.HScrollRight:
                    binding._State = MouseState.HorizontalScrollWheelValue - _PreviousMouseState.HorizontalScrollWheelValue > 0;
                    break;
                case MouseTriggers.HScrollLeft:
                    binding._State = MouseState.HorizontalScrollWheelValue - _PreviousMouseState.HorizontalScrollWheelValue < 0;
                    break;
                case MouseTriggers.X1:
                    binding._State = MouseState.XButton1 == ButtonState.Pressed;
                    break;
                case MouseTriggers.X2:
                    binding._State = MouseState.XButton2 == ButtonState.Pressed;
                    break;
            }
        }
    }
}
