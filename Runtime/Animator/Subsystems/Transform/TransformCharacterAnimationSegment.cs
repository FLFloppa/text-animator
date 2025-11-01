using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback;
using FLFloppa.TextAnimator.Playback.Systems;
using FLFloppa.TextAnimator.Tags;

namespace FLFloppa.TextAnimator.Animator.Subsystems.Transform
{
    /// <summary>
    /// Applies transform-related character modifiers.
    /// </summary>
    internal sealed class TransformCharacterAnimationSegment : ICharacterAnimationPipelineSegment
    {
        private readonly TransformStateCache _stateCache = new TransformStateCache();
        private readonly List<ITextOutputApplicatorBinding<TransformCharacterState>> _applicatorBindings = new List<ITextOutputApplicatorBinding<TransformCharacterState>>();

        /// <inheritdoc />
        public TextAnimatorSubsystemKey Key => TextAnimatorSubsystemKeys.Transform;

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

                if (applicator is ITextOutputApplicatorBindingProvider<TransformCharacterState> provider &&
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
                return;
            }

            var state = _stateCache.Acquire(characterIndex);
            var modified = false;

            for (var i = 0; i < modifierBindings.Count; i++)
            {
                var modifier = modifierBindings[i].Modifier;
                if (!modifier.TargetSubsystem.Equals(Key))
                {
                    continue;
                }

                if (modifier is ITransformModifier transformModifier)
                {
                    transformModifier.Modify(state, characterIndex, sessionElapsedTime, characterElapsedTime);
                    modified = true;
                }
            }

            if (!modified || _applicatorBindings.Count == 0)
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
