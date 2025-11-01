using FLFloppa.TextAnimator.Characters;
using UnityEngine;

namespace FLFloppa.TextAnimator.Playback.Strategies
{
    /// <summary>
    /// Batching strategy that reveals a fixed number of characters per duration step.
    /// </summary>
    internal sealed class CharacterCountRevealStrategy : IRevealBatchingStrategy
    {
        private sealed class BatchState
        {
            public int CharactersInBatch;
        }

        private readonly int _revealCount;
        private readonly BatchState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterCountRevealStrategy"/> class.
        /// </summary>
        /// <param name="revealCount">Number of characters displayed per duration step.</param>
        public CharacterCountRevealStrategy(int revealCount)
            : this(Mathf.Max(1, revealCount), new BatchState())
        {
        }

        private CharacterCountRevealStrategy(int revealCount, BatchState state)
        {
            _revealCount = Mathf.Max(1, revealCount);
            _state = state;
        }

        /// <summary>
        /// Creates a character-count strategy configured for single-character batching.
        /// </summary>
        public static IRevealBatchingStrategy CreateDefault()
        {
            return new CharacterCountRevealStrategy(1);
        }

        /// <inheritdoc />
        public bool ShouldApplyDuration(char character, CharacterDescriptor descriptor, bool isWordCharacter, bool isWordStart)
        {
            return _state.CharactersInBatch == 0;
        }

        /// <inheritdoc />
        public void Advance(char character, CharacterDescriptor descriptor, bool isWordCharacter, bool isWordStart)
        {
            _state.CharactersInBatch++;
            if (_state.CharactersInBatch >= _revealCount)
            {
                _state.CharactersInBatch = 0;
            }
        }

        /// <inheritdoc />
        public IRevealBatchingStrategy CreateChild()
        {
            return new CharacterCountRevealStrategy(_revealCount, _state);
        }
    }
}
