using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using UnityEngine;

namespace FLFloppa.TextAnimator.Effects.Modifiers
{
    /// <summary>
    /// Applies a sine wave offset to character vertices.
    /// </summary>
    public sealed class WaveModifier : ITransformModifier
    {
        private readonly float _amplitude;
        private readonly float _frequency;
        private readonly float _phaseOffset;

        /// <inheritdoc />
        public TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Transform;

        public WaveModifier(float amplitude, float frequency, float phaseOffset)
        {
            _amplitude = amplitude;
            _frequency = frequency;
            _phaseOffset = phaseOffset;
        }

        public void Modify(TransformCharacterState state, int characterIndex, float sessionElapsedTime, float characterElapsedTime)
        {
            if (state == null)
            {
                return;
            }

            var wave = Mathf.Sin((_frequency * sessionElapsedTime) + characterIndex * _phaseOffset);
            state.PositionOffset += new Vector3(0f, _amplitude * wave, 0f);
        }
    }
}
