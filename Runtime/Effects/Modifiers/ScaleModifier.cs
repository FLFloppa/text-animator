using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using UnityEngine;

namespace FLFloppa.TextAnimator.Effects.Modifiers
{
    /// <summary>
    /// Applies a scale animation to transform character states with optional looping and per-axis control.
    /// </summary>
    public sealed class ScaleModifier : ITransformModifier
    {
        private readonly float _duration;
        private readonly bool _loop;
        private readonly AnimationCurve _scaleXCurve;
        private readonly AnimationCurve _scaleYCurve;
        private readonly AnimationCurve _scaleZCurve;

        /// <inheritdoc />
        public TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleModifier"/> class.
        /// </summary>
        /// <param name="duration">Duration of one complete animation cycle.</param>
        /// <param name="scaleXCurve">Animation curve defining X-axis scale values over time.</param>
        /// <param name="scaleYCurve">Animation curve defining Y-axis scale values over time.</param>
        /// <param name="scaleZCurve">Animation curve defining Z-axis scale values over time.</param>
        /// <param name="loop">If true, animation repeats continuously; otherwise plays once.</param>
        public ScaleModifier(float duration, AnimationCurve scaleXCurve, AnimationCurve scaleYCurve, AnimationCurve scaleZCurve, bool loop)
        {
            if (scaleXCurve == null)
            {
                throw new ArgumentNullException(nameof(scaleXCurve));
            }

            if (scaleYCurve == null)
            {
                throw new ArgumentNullException(nameof(scaleYCurve));
            }

            if (scaleZCurve == null)
            {
                throw new ArgumentNullException(nameof(scaleZCurve));
            }

            _duration = Mathf.Max(0.001f, duration);
            _loop = loop;
            _scaleXCurve = new AnimationCurve(scaleXCurve.keys);
            _scaleYCurve = new AnimationCurve(scaleYCurve.keys);
            _scaleZCurve = new AnimationCurve(scaleZCurve.keys);
        }

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

            var scaleX = _scaleXCurve.Evaluate(normalizedTime);
            var scaleY = _scaleYCurve.Evaluate(normalizedTime);
            var scaleZ = _scaleZCurve.Evaluate(normalizedTime);
            state.Scale = new Vector3(scaleX, scaleY, scaleZ);
        }
    }
}
