using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback.Systems;

namespace FLFloppa.TextAnimator.Playback.Systems
{
    /// <summary>
    /// Applies character modifiers and dispatches property sets to the output target.
    /// </summary>
    public interface ICharacterAnimationPipeline
    {
        /// <summary>
        /// Applies all modifier and property calculations for the specified character index.
        /// </summary>
        /// <param name="characterIndex">The zero-based character index.</param>
        /// <param name="descriptor">The descriptor describing the character.</param>
        /// <param name="sessionElapsedTime">The session elapsed time.</param>
        /// <param name="modifiers">The modifier lookup keyed by tag node.</param>
        /// <param name="output">The text output that receives subsystem state.</param>
        void Apply(
            int characterIndex,
            CharacterDescriptor descriptor,
            float sessionElapsedTime,
            CharacterRevealClock revealClock,
            IReadOnlyDictionary<TagNode, ISubsystemModifier> modifiers,
            ITextOutput output);
    }
}
