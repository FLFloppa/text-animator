using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using UnityEngine;

namespace FLFloppa.TextAnimator.Effects.Modifiers
{
    /// <summary>
    /// Gradually fades in characters over a specified duration.
    /// </summary>
    public sealed class FadeInModifier : IColorModifier
    {
        private readonly float _duration;
        private readonly AnimationCurve _alphaCurve;

        /// <inheritdoc />
        public TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Color;

        /// <summary>
        /// Initializes a new instance of the <see cref="FadeInModifier"/> class.
        /// </summary>
        /// <param name="duration">Duration over which the fade should complete.</param>
        /// <param name="alphaCurve">Curve describing the alpha progression from 0 to 1.</param>
        public FadeInModifier(float duration, AnimationCurve alphaCurve)
        {
            if (alphaCurve == null)
            {
                throw new ArgumentNullException(nameof(alphaCurve));
            }

            _duration = Mathf.Max(0.001f, duration);
            _alphaCurve = new AnimationCurve(alphaCurve.keys);
        }

        /// <summary>
        /// Applies the fade-in effect to the provided character property bag.
        /// </summary>
        /// <param name="propertyBag">The property bag to mutate.</param>
        /// <param name="characterIndex">The index of the character being modified.</param>
        /// <param name="elapsedTime">Total session elapsed time.</param>
        /// <param name="characterElapsedTime">Elapsed time since the character was revealed.</param>
        public void Modify(ColorCharacterState state, int characterIndex, float sessionElapsedTime, float characterElapsedTime)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var normalizedTime = Mathf.Clamp01(characterElapsedTime / _duration);
            var evaluatedAlpha = Mathf.Clamp01(_alphaCurve.Evaluate(normalizedTime));
            var color = state.Color;
            color.a = evaluatedAlpha;
            state.Color = color;
        }
    }
}
