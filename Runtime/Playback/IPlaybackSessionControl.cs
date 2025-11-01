using System;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Provides timeline control operations that playback instructions can invoke.
    /// </summary>
    public interface IPlaybackSessionControl
    {
        /// <summary>
        /// Reveals the specified character within the playback session.
        /// </summary>
        /// <param name="characterIndex">The zero-based character index.</param>
        /// <param name="onCharacterRevealed">Optional callback executed when the character becomes visible.</param>
        void RevealCharacter(int characterIndex, Action<string> onCharacterRevealed = null);
    }
}
