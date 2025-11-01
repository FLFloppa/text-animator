using System;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Reveals a specific character in the output.
    /// </summary>
    public sealed class RevealCharacterInstruction : IPlaybackInstruction
    {
        private readonly Action<string> _onCharacterRevealed;

        public int CharacterIndex { get; }
        public float Duration { get; }

        public RevealCharacterInstruction(int characterIndex, float duration, Action<string> onCharacterRevealed = null)
        {
            CharacterIndex = characterIndex;
            Duration = duration;
            _onCharacterRevealed = onCharacterRevealed;
        }

        public void Execute(IPlaybackSessionControl sessionControl)
        {
            if (sessionControl == null)
            {
                throw new ArgumentNullException(nameof(sessionControl));
            }

            sessionControl.RevealCharacter(CharacterIndex, _onCharacterRevealed);
        }
    }
}