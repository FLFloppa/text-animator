using FLFloppa.TextAnimator.Characters;
using UnityEngine;

namespace FLFloppa.TextAnimator.Playback.Strategies
{
    /// <summary>
    /// Batching strategy that reveals a fixed number of words per duration step.
    /// </summary>
    internal sealed class WordRevealStrategy : IRevealBatchingStrategy
    {
        private sealed class WordBatchState
        {
            public int WordsInBatch;
        }

        private readonly int _revealCount;
        private readonly WordBatchState _state;

        public WordRevealStrategy(int revealCount)
            : this(Mathf.Max(1, revealCount), new WordBatchState())
        {
        }

        private WordRevealStrategy(int revealCount, WordBatchState state)
        {
            _revealCount = revealCount;
            _state = state;
        }

        public bool ShouldApplyDuration(char character, CharacterDescriptor descriptor, bool isWordCharacter, bool isWordStart)
        {
            return isWordStart && _state.WordsInBatch == 0;
        }

        public void Advance(char character, CharacterDescriptor descriptor, bool isWordCharacter, bool isWordStart)
        {
            if (!isWordStart)
            {
                return;
            }

            _state.WordsInBatch++;
            if (_state.WordsInBatch >= _revealCount)
            {
                _state.WordsInBatch = 0;
            }
        }

        public IRevealBatchingStrategy CreateChild()
        {
            return new WordRevealStrategy(_revealCount, _state);
        }
    }
}
