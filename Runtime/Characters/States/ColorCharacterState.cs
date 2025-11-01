using UnityEngine;

namespace FLFloppa.TextAnimator.Characters.States
{
    /// <summary>
    /// Represents color state for a single character.
    /// </summary>
    public sealed class ColorCharacterState
    {
        public Color Color { get; set; } = new Color(1f, 1f, 1f, 0f);
        public bool OverrideRGB { get; set; } = false;

        /// <summary>
        /// Resets the state to its default value.
        /// </summary>
        public void Reset()
        {
            Color = new Color(1f, 1f, 1f, 0f);
            OverrideRGB = false;
        }
    }
}
