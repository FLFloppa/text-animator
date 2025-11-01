using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using UnityEngine;

namespace FLFloppa.TextAnimator.Effects.Modifiers
{
    /// <summary>
    /// Applies a rotation animation to transform character states with optional looping.
    /// </summary>
    public sealed class RotateModifier : ITransformModifier
    {
        private readonly float _duration;
        private readonly bool _loop;
        private readonly AnimationCurve _rotationCurve;

        /// <inheritdoc />
        public TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="RotateModifier"/> class.
        /// </summary>
        /// <param name="duration">Duration of one complete animation cycle.</param>
        /// <param name="rotationCurve">Animation curve where values directly represent rotation angle in degrees.</param>
        /// <param name="loop">If true, animation repeats continuously; otherwise plays once.</param>
        public RotateModifier(float duration, AnimationCurve rotationCurve, bool loop)
        {
            if (rotationCurve == null)
            {
                throw new ArgumentNullException(nameof(rotationCurve));
            }

            _duration = Mathf.Max(0.001f, duration);
            _loop = loop;
            _rotationCurve = new AnimationCurve(rotationCurve.keys);
        }

        /// <inheritdoc />
        public void Modify(TransformCharacterState state, int characterIndex, float sessionElapsedTime, float characterElapsedTime)
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

            var angle = _rotationCurve.Evaluate(normalizedTime);
            var rotation = Quaternion.Euler(0f, 0f, angle);

            state.Rotation = rotation * state.Rotation;
        }
    }
}
