namespace FLFloppa.TextAnimator.Playback.Strategies
{
    /// <summary>
    /// Defines batching behavior for reveal instructions.
    /// </summary>
    internal interface IRevealBatchingStrategy
    {
        /// <summary>
        /// Determines whether the current character should use the configured duration.
        /// </summary>
        /// <param name="character">The character being processed.</param>
        /// <param name="descriptor">Descriptor associated with the character.</param>
        /// <param name="isWordCharacter">Indicates whether the character is considered part of a word.</param>
        /// <param name="isWordStart">Indicates whether the character starts a new word token.</param>
        /// <returns>True when the reveal should consume the full duration.</returns>
        bool ShouldApplyDuration(char character, CharacterDescriptor descriptor, bool isWordCharacter, bool isWordStart);

        /// <summary>
        /// Advances the batching state after processing a character.
        /// </summary>
        /// <param name="character">The character being processed.</param>
        /// <param name="descriptor">Descriptor associated with the character.</param>
        /// <param name="isWordCharacter">Indicates whether the character is considered part of a word.</param>
        /// <param name="isWordStart">Indicates whether the character starts a new word token.</param>
        void Advance(char character, CharacterDescriptor descriptor, bool isWordCharacter, bool isWordStart);

        /// <summary>
        /// Creates a strategy suitable for nested content.
        /// </summary>
        /// <returns>A new strategy instance sharing batching state as appropriate.</returns>
        IRevealBatchingStrategy CreateChild();
    }
}
