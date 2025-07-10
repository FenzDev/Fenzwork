using Microsoft.Xna.Framework.Input;
using System;

namespace Fenzwork.Systems.Input
{
    /// <summary>
    /// Represents a combination of a key and its associated modifiers.
    /// </summary>
    public struct KeyCombination
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyCombination"/> struct with no key assigned.
        /// </summary>
        public KeyCombination()
        {
            Key = Keys.None;
            Modifiers = KeyModifiers.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyCombination"/> struct with a specified key and optional modifiers.
        /// </summary>
        /// <param name="key">The main key of the combination.</param>
        /// <param name="modifiers">The key modifiers (e.g., Ctrl, Shift, Alt).</param>
        public KeyCombination(Keys key, KeyModifiers modifiers = KeyModifiers.None)
        {
            Key = key;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Gets the main key of the combination.
        /// </summary>
        public Keys Key { get; }

        /// <summary>
        /// Gets the modifiers associated with the key.
        /// </summary>
        public KeyModifiers Modifiers { get; }

        public static implicit operator KeyCombination(Keys key) => new (key);
        public static implicit operator KeyCombination((Keys,KeyModifiers) key) => new (key.Item1, key.Item2);
    }

    /// <summary>
    /// Represents modifier keys that can be used in combination with other keys.
    /// </summary>
    [Flags]
    public enum KeyModifiers
    {
        /// <summary>
        /// No modifier.
        /// </summary>
        None,

        /// <summary>
        /// The Control (Ctrl) key.
        /// </summary>
        Ctrl,

        /// <summary>
        /// The Shift key.
        /// </summary>
        Shift,

        /// <summary>
        /// A combination of the Control (Ctrl) and Shift keys.
        /// </summary>
        CtrlShift,

        /// <summary>
        /// The Alt key.
        /// </summary>
        Alt,

        /// <summary>
        /// A combination of the Alt and Control (Ctrl) keys.
        /// </summary>
        AltCtrl,

        /// <summary>
        /// A combination of the Alt and Shift keys.
        /// </summary>
        AltShift,

        /// <summary>
        /// A combination of the Control (Ctrl), Shift, and Alt keys.
        /// </summary>
        CtrlShiftAlt,
    }
}
