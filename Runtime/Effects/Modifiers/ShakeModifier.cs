using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using UnityEngine;

namespace FLFloppa.TextAnimator.Effects.Modifiers
{
    /// <summary>
    /// Applies a jittering offset to character meshes to simulate shaking.
    /// </summary>
    public sealed class ShakeModifier : ITransformModifier
    {
        private const float PhaseOffset = 0.37f;

        private readonly float _amplitude;
        private readonly float _frequency;
        private readonly bool _synchronize;

        /// <inheritdoc />
        public TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShakeModifier"/> class.
        /// </summary>
        /// <param name="amplitude">Maximum positional deviation applied to characters.</param>
        /// <param name="frequency">Oscillation frequency controlling shake speed.</param>
        /// <param name="synchronize">When true, characters share the same shake phase.</param>
        public ShakeModifier(float amplitude, float frequency, bool synchronize)
        {
            _amplitude = Mathf.Max(0f, amplitude);
            _frequency = Mathf.Max(0f, frequency);
            _synchronize = synchronize;
        }

        /// <inheritdoc />
        public void Modify(TransformCharacterState state, int characterIndex, float sessionElapsedTime, float characterElapsedTime)
        {
            if (state == null || _amplitude <= 0f || _frequency <= 0f)
            {
                return;
            }

            var phase = sessionElapsedTime * _frequency;
            var noiseSeed = _synchronize ? 0f : characterIndex * PhaseOffset;

            var xNoise = (Mathf.PerlinNoise(phase, noiseSeed) * 2f) - 1f;
            var yNoise = (Mathf.PerlinNoise(noiseSeed, phase) * 2f) - 1f;

            var offset = new Vector3(xNoise, yNoise, 0f) * _amplitude;
            state.PositionOffset += offset;
        }
    }
}
