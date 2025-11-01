using System;
using FLFloppa.TextAnimator.Characters.States;

namespace FLFloppa.TextAnimator.Animator.Subsystems.Material
{
    /// <summary>
    /// Provides pooled material states for material animation segments.
    /// </summary>
    internal sealed class MaterialStateCache
    {
        private MaterialCharacterState[] _states = Array.Empty<MaterialCharacterState>();

        public MaterialCharacterState Acquire(int characterIndex)
        {
            if (characterIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(characterIndex));
            }

            EnsureCapacity(characterIndex + 1);
            return _states[characterIndex];
        }

        public void Reset(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= _states.Length)
            {
                return;
            }

            _states[characterIndex]?.Reset();
        }

        public void EnsureCapacity(int characterCount)
        {
            if (characterCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(characterCount));
            }

            if (_states.Length >= characterCount)
            {
                return;
            }

            var previousLength = _states.Length;
            var newLength = CalculateNewLength(previousLength, characterCount);

            Array.Resize(ref _states, newLength);
            for (var i = previousLength; i < _states.Length; i++)
            {
                _states[i] = new MaterialCharacterState();
            }
        }

        private static int CalculateNewLength(int currentLength, int requiredLength)
        {
            var newLength = Math.Max(4, currentLength == 0 ? requiredLength : currentLength);
            while (newLength < requiredLength)
            {
                newLength *= 2;
            }

            return newLength;
        }
    }
}
