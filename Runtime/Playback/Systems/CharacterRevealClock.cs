using System;

namespace FLFloppa.TextAnimator.Playback.Systems
{
    /// <summary>
    /// Tracks per-character reveal timing for playback sessions.
    /// </summary>
    public sealed class CharacterRevealClock
    {
        private readonly float[] _revealTimes;
        private readonly float[] _elapsedCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterRevealClock"/> class.
        /// </summary>
        /// <param name="characterCount">The number of characters in the playback session.</param>
        public CharacterRevealClock(int characterCount)
        {
            if (characterCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(characterCount));
            }

            _revealTimes = new float[characterCount];
            _elapsedCache = new float[characterCount];

            for (var i = 0; i < characterCount; i++)
            {
                _revealTimes[i] = float.NaN;
                _elapsedCache[i] = 0f;
            }
        }

        /// <summary>
        /// Gets the number of characters tracked by this clock.
        /// </summary>
        public int CharacterCount => _revealTimes.Length;

        /// <summary>
        /// Determines whether the specified character has been revealed.
        /// </summary>
        public bool HasRevealTime(int characterIndex)
        {
            ValidateIndex(characterIndex);
            return !float.IsNaN(_revealTimes[characterIndex]);
        }

        /// <summary>
        /// Records the reveal time for the specified character.
        /// </summary>
        public void SetRevealTime(int characterIndex, float revealTime)
        {
            ValidateIndex(characterIndex);
            _revealTimes[characterIndex] = revealTime;
        }

        /// <summary>
        /// Gets the elapsed time since the character was revealed.
        /// </summary>
        public float GetRevealElapsed(int characterIndex, float sessionElapsedTime)
        {
            ValidateIndex(characterIndex);
            var revealTime = _revealTimes[characterIndex];
            if (float.IsNaN(revealTime))
            {
                return 0f;
            }

            var elapsed = sessionElapsedTime - revealTime;
            return elapsed < 0f ? 0f : elapsed;
        }

        /// <summary>
        /// Gets the cached elapsed time for the specified character.
        /// </summary>
        public float GetCachedElapsed(int characterIndex)
        {
            ValidateIndex(characterIndex);
            return _elapsedCache[characterIndex];
        }

        /// <summary>
        /// Updates the cached elapsed time using the greater of the stored and provided values.
        /// </summary>
        public void UpdateCachedElapsed(int characterIndex, float elapsed)
        {
            ValidateIndex(characterIndex);
            if (elapsed > _elapsedCache[characterIndex])
            {
                _elapsedCache[characterIndex] = elapsed;
            }
        }

        private void ValidateIndex(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= _revealTimes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(characterIndex));
            }
        }
    }
}
