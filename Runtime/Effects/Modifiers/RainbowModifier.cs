using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using UnityEngine;

namespace FLFloppa.TextAnimator.Effects.Modifiers
{
    /// <summary>
    /// Applies a rainbow gradient animation to character colors with optional looping.
    /// </summary>
    public sealed class RainbowModifier : IColorModifier
    {
        private readonly float _duration;
        private readonly float _colorShift;
        private readonly bool _loop;
        private readonly Gradient _gradient;

        /// <inheritdoc />
        public TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Color;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainbowModifier"/> class.
        /// </summary>
        /// <param name="duration">Duration of one complete animation cycle.</param>
        /// <param name="colorShift">Amount of color offset per character index (0-1 range for full gradient).</param>
        /// <param name="gradient">Gradient defining the rainbow colors.</param>
        /// <param name="loop">If true, animation repeats continuously; otherwise plays once.</param>
        public RainbowModifier(float duration, float colorShift, Gradient gradient, bool loop)
        {
            if (gradient == null)
            {
                throw new ArgumentNullException(nameof(gradient));
            }

            _duration = Mathf.Max(0.001f, duration);
            _colorShift = colorShift;
            _loop = loop;
            _gradient = new Gradient
            {
                colorKeys = gradient.colorKeys,
                alphaKeys = gradient.alphaKeys,
                mode = gradient.mode
            };
        }

        /// <inheritdoc />
        public void Modify(ColorCharacterState state, int characterIndex, float sessionElapsedTime, float characterElapsedTime)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var normalizedTime = characterElapsedTime / _duration;
            if (_loop)
            {
                normalizedTime = normalizedTime - Mathf.Floor(normalizedTime);
            }
            else
            {
                normalizedTime = Mathf.Clamp01(normalizedTime);
            }

            var gradientPosition = normalizedTime + (characterIndex * _colorShift);
            gradientPosition = gradientPosition - Mathf.Floor(gradientPosition);

            var rainbowColor = _gradient.Evaluate(gradientPosition);
            state.Color = rainbowColor;
            state.OverrideRGB = true;
        }
    }
}
