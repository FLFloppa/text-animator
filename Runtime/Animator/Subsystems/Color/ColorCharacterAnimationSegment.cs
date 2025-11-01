using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback;
using FLFloppa.TextAnimator.Tags;

namespace FLFloppa.TextAnimator.Animator.Subsystems.Color
{
    /// <summary>
    /// Applies color-related modifiers and maintains alpha floor state.
    /// </summary>
    internal sealed class ColorCharacterAnimationSegment : ICharacterAnimationPipelineSegment
    {
        private readonly ColorStateCache _stateCache = new ColorStateCache();
        private readonly List<ITextOutputApplicatorBinding<ColorCharacterState>> _applicatorBindings = new List<ITextOutputApplicatorBinding<ColorCharacterState>>();

        /// <inheritdoc />
        public TextAnimatorSubsystemKey Key => TextAnimatorSubsystemKeys.Color;

        /// <inheritdoc />
        public void EnsureCapacity(int characterCount)
        {
            _stateCache.EnsureCapacity(characterCount);
        }

        /// <inheritdoc />
        public void Configure(ITextOutput output, IEnumerable<ITextOutputApplicator> applicators)
        {
            _applicatorBindings.Clear();

            if (output == null || applicators == null)
            {
                return;
            }

            foreach (var applicator in applicators)
            {
                if (applicator == null)
                {
                    continue;
                }

                if (!applicator.TargetSubsystem.Equals(Key))
                {
                    continue;
                }

                if (applicator is ITextOutputApplicatorBindingProvider<ColorCharacterState> provider &&
                    provider.TryCreateBinding(output, out var binding))
                {
                    _applicatorBindings.Add(binding);
                }
            }
        }

        /// <inheritdoc />
        public void ResetState(int characterIndex)
        {
            _stateCache.Reset(characterIndex);
        }

        /// <inheritdoc />
        public void Apply(
            int characterIndex,
            CharacterDescriptor descriptor,
            float sessionElapsedTime,
            float characterElapsedTime,
            IReadOnlyDictionary<TagNode, ISubsystemModifier> modifiers,
            ITextOutput output)
        {
            _ = modifiers;

            var modifierBindings = descriptor.Modifiers;
            if (modifierBindings.Count == 0)
            {
                SetBaselineAndFlush(characterIndex, output);
                return;
            }

            var state = _stateCache.Acquire(characterIndex);

            var color = state.Color;
            color.r = 1f;
            color.g = 1f;
            color.b = 1f;
            color.a = 1f;

            var baselineAlpha = _stateCache.GetAlphaFloor(characterIndex);
            if (baselineAlpha > color.a)
            {
                color.a = baselineAlpha;
            }

            state.Color = color;

            for (var i = 0; i < modifierBindings.Count; i++)
            {
                var modifierBinding = modifierBindings[i];
                var modifier = modifierBinding.Modifier;
                if (!modifier.TargetSubsystem.Equals(Key))
                {
                    continue;
                }

                if (modifier is IColorModifier colorModifier)
                {
                    colorModifier.Modify(state, characterIndex, sessionElapsedTime, characterElapsedTime);
                }
            }

            _stateCache.AccumulateAlphaFloor(characterIndex, state.Color.a);

            if (_applicatorBindings.Count == 0)
            {
                return;
            }

            for (var i = 0; i < _applicatorBindings.Count; i++)
            {
                _applicatorBindings[i].Apply(characterIndex, state);
            }
        }

        private void SetBaselineAndFlush(int characterIndex, ITextOutput output)
        {
            var state = _stateCache.Acquire(characterIndex);

            var color = state.Color;
            color.r = 1f;
            color.g = 1f;
            color.b = 1f;
            color.a = Math.Max(1f, _stateCache.GetAlphaFloor(characterIndex));
            state.Color = color;
            _stateCache.AccumulateAlphaFloor(characterIndex, color.a);

            if (_applicatorBindings.Count == 0)
            {
                return;
            }

            for (var i = 0; i < _applicatorBindings.Count; i++)
            {
                _applicatorBindings[i].Apply(characterIndex, state);
            }
        }
    }
}
