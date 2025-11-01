using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback;
using FLFloppa.TextAnimator.Tags;

namespace FLFloppa.TextAnimator.Animator.Subsystems
{
    /// <summary>
    /// Defines a composable segment that contributes modifier evaluation for a text animator pipeline.
    /// </summary>
    public interface ICharacterAnimationPipelineSegment
    {
        /// <summary>
        /// Gets the key identifying the subsystem associated with this segment.
        /// </summary>
        TextAnimatorSubsystemKey Key { get; }

        /// <summary>
        /// Ensures the segment has sufficient capacity for the specified character count.
        /// </summary>
        /// <param name="characterCount">The required character count.</param>
        void EnsureCapacity(int characterCount);

        /// <summary>
        /// Configures the segment with output applicators compatible with the active text output.
        /// </summary>
        /// <param name="output">The active text output instance.</param>
        /// <param name="applicators">The applicators available to the subsystem.</param>
        void Configure(ITextOutput output, IEnumerable<ITextOutputApplicator> applicators);

        /// <summary>
        /// Resets the subsystem state for the specified character.
        /// </summary>
        /// <param name="characterIndex">The character index to reset.</param>
        void ResetState(int characterIndex);

        /// <summary>
        /// Applies subsystem-specific modifier logic for a single character.
        /// </summary>
        /// <param name="characterIndex">The zero-based index of the character.</param>
        /// <param name="descriptor">The descriptor describing the character.</param>
        /// <param name="sessionElapsedTime">The elapsed playback time for the session.</param>
        /// <param name="characterElapsedTime">The elapsed time since the character was revealed.</param>
        /// <param name="modifiers">The modifier lookup keyed by tag node.</param>
        /// <param name="output">The text output to apply state changes to.</param>
        void Apply(
            int characterIndex,
            CharacterDescriptor descriptor,
            float sessionElapsedTime,
            float characterElapsedTime,
            IReadOnlyDictionary<TagNode, ISubsystemModifier> modifiers,
            ITextOutput output);
    }
}
