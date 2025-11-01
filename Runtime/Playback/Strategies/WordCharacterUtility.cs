namespace FLFloppa.TextAnimator.Playback.Strategies
{
    /// <summary>
    /// Utility helpers for analysing word-related character properties.
    /// </summary>
    internal static class WordCharacterUtility
    {
        /// <summary>
        /// Determines whether the specified character is part of a word token.
        /// </summary>
        /// <param name="character">Character to evaluate.</param>
        /// <returns><c>true</c> when the character belongs to a word token.</returns>
        public static bool IsWordCharacter(char character)
        {
            if (char.IsWhiteSpace(character))
            {
                return false;
            }

            if (char.IsLetterOrDigit(character))
            {
                return true;
            }

            return character == '\'' || character == '-';
        }
    }
}
