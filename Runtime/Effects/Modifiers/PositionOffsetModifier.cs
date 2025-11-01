using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using UnityEngine;

namespace FLFloppa.TextAnimator.Effects.Modifiers
{
    /// <summary>
    /// Applies a position offset animation to transform character states with optional looping.
    /// </summary>
    public sealed class PositionOffsetModifier : ITransformModifier
    {
        private readonly float _duration;
        private readonly bool _loop;
        private readonly bool _override;
        private readonly AnimationCurve _positionXCurve;
        private readonly AnimationCurve _positionYCurve;
        private readonly AnimationCurve _positionZCurve;

        /// <inheritdoc />
        public TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionOffsetModifier"/> class.
        /// </summary>
        /// <param name="duration">Duration of one complete animation cycle.</param>
        /// <param name="positionXCurve">Animation curve where values directly represent X-axis position offset.</param>
        /// <param name="positionYCurve">Animation curve where values directly represent Y-axis position offset.</param>
        /// <param name="positionZCurve">Animation curve where values directly represent Z-axis position offset.</param>
        /// <param name="loop">If true, animation repeats continuously; otherwise plays once.</param>
        /// <param name="override">If true, replaces existing position offset; otherwise adds to it.</param>
        public PositionOffsetModifier(
            float duration,
            AnimationCurve positionXCurve,
            AnimationCurve positionYCurve,
            AnimationCurve positionZCurve,
            bool loop,
            bool @override)
        {
            if (positionXCurve == null)
            {
                throw new ArgumentNullException(nameof(positionXCurve));
            }

            if (positionYCurve == null)
            {
                throw new ArgumentNullException(nameof(positionYCurve));
            }

            if (positionZCurve == null)
            {
                throw new ArgumentNullException(nameof(positionZCurve));
            }

            _duration = Mathf.Max(0.001f, duration);
            _loop = loop;
            _override = @override;
            _positionXCurve = new AnimationCurve(positionXCurve.keys);
            _positionYCurve = new AnimationCurve(positionYCurve.keys);
            _positionZCurve = new AnimationCurve(positionZCurve.keys);
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

            var offsetX = _positionXCurve.Evaluate(normalizedTime);
            var offsetY = _positionYCurve.Evaluate(normalizedTime);
            var offsetZ = _positionZCurve.Evaluate(normalizedTime);
            var offset = new Vector3(offsetX, offsetY, offsetZ);

            if (_override)
            {
                state.PositionOffset = offset;
            }
            else
            {
                state.PositionOffset += offset;
            }
        }
    }
}
