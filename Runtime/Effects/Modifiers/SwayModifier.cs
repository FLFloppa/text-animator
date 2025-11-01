using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using FLFloppa.TextAnimator.Document;
using UnityEngine;

namespace FLFloppa.TextAnimator.Effects.Modifiers
{
    /// <summary>
    /// Applies a continuous swaying rotation effect to character transforms.
    /// </summary>
    public sealed class SwayModifier : ITransformModifier
    {
        private readonly float _frequency;
        private readonly float _amplitude;
        private readonly bool _grouped;
        private readonly float _phaseOffset;
        private readonly TagNode _sourceTag;
        private readonly Dictionary<int, int> _characterSpanIndices = new Dictionary<int, int>();
        private int _spanStart = -1;
        private int _spanEnd = -1;
        private bool _spanInitialized;

        /// <inheritdoc />
        public TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwayModifier"/> class.
        /// </summary>
        /// <param name="frequency">Oscillation frequency in Hz.</param>
        /// <param name="amplitude">Maximum rotation angle in degrees.</param>
        /// <param name="grouped">If true, characters rotate around a shared group pivot; otherwise around individual centers.</param>
        /// <param name="phaseOffset">Phase offset per character for staggered animation.</param>
        /// <param name="sourceTag">Tag node that created this modifier, used to determine grouping span.</param>
        public SwayModifier(float frequency, float amplitude, bool grouped, float phaseOffset, TagNode sourceTag)
        {
            _frequency = Mathf.Max(0f, frequency);
            _amplitude = amplitude;
            _grouped = grouped;
            _phaseOffset = phaseOffset;
            _sourceTag = sourceTag;
        }

        /// <inheritdoc />
        public void Modify(TransformCharacterState state, int characterIndex, float sessionElapsedTime, float characterElapsedTime)
        {
            if (state == null)
            {
                return;
            }

            if (!_spanInitialized)
            {
                _spanInitialized = true;
            }

            if (!_characterSpanIndices.ContainsKey(characterIndex))
            {
                if (_spanStart < 0)
                {
                    _spanStart = characterIndex;
                }
                
                _spanEnd = Mathf.Max(_spanEnd, characterIndex + 1);
                var spanIndex = characterIndex - _spanStart;
                _characterSpanIndices[characterIndex] = spanIndex;
            }
            
            if (!_characterSpanIndices.TryGetValue(characterIndex, out var currentSpanIndex))
            {
                currentSpanIndex = 0;
            }

            var phase = sessionElapsedTime * _frequency * Mathf.PI * 2f;
            
            if (!_grouped && _phaseOffset != 0f)
            {
                phase += _phaseOffset * currentSpanIndex;
            }

            var angle = Mathf.Sin(phase) * _amplitude;
            var rotation = Quaternion.Euler(0f, 0f, angle);

            if (_grouped)
            {
                state.UseGroupPivot = true;
                state.GroupStartIndex = _spanStart;
                state.GroupEndIndex = _spanEnd;
            }

            state.Rotation = rotation * state.Rotation;
        }
    }
}
