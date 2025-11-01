using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Characters.States;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback;
using FLFloppa.TextAnimator.Tags;

namespace FLFloppa.TextAnimator.Animator.Subsystems.Material
{
    /// <summary>
    /// Applies material-related modifiers, aggregating shader property overrides.
    /// </summary>
    internal sealed class MaterialCharacterAnimationSegment : ICharacterAnimationPipelineSegment
    {
        private readonly MaterialStateCache _stateCache = new MaterialStateCache();
        private readonly List<ITextOutputApplicatorBinding<MaterialCharacterState>> _applicatorBindings = new List<ITextOutputApplicatorBinding<MaterialCharacterState>>();

        /// <inheritdoc />
        public TextAnimatorSubsystemKey Key => TextAnimatorSubsystemKeys.Material;

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

                if (applicator is ITextOutputApplicatorBindingProvider<MaterialCharacterState> provider &&
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
            state.Reset();

            var materialModified = false;
            for (var i = 0; i < modifierBindings.Count; i++)
            {
                var modifier = modifierBindings[i].Modifier;
                if (!modifier.TargetSubsystem.Equals(Key))
                {
                    continue;
                }

                if (modifier is IMaterialModifier materialModifier)
                {
                    materialModifier.Modify(state, characterIndex, sessionElapsedTime, characterElapsedTime);
                    materialModified = true;
                }
            }

            if (materialModified && state.FloatOverrides.Count > 0)
            {
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
}
