using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Document;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Associates a modifier with its originating tag for a specific character descriptor.
    /// </summary>
    public readonly struct CharacterModifierBinding
    {
        public CharacterModifierBinding(TagNode tag, ISubsystemModifier modifier)
        {
            Tag = tag;
            Modifier = modifier;
        }

        /// <summary>
        /// Gets the tag that produced the modifier.
        /// </summary>
        public TagNode Tag { get; }

        /// <summary>
        /// Gets the modifier instance applied to the character.
        /// </summary>
        public ISubsystemModifier Modifier { get; }
    }
}
