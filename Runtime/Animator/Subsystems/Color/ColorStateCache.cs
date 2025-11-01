using System;
using FLFloppa.TextAnimator.Characters.States;

namespace FLFloppa.TextAnimator.Animator.Subsystems.Color
{
    /// <summary>
    /// Provides pooled color states and alpha floors for color animation segments.
    /// </summary>
    internal sealed class ColorStateCache
    {
        private ColorCharacterState[] _states = Array.Empty<ColorCharacterState>();
        private float[] _alphaFloors = Array.Empty<float>();

        public ColorCharacterState Acquire(int characterIndex)
        {
            if (characterIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(characterIndex));
            }

            EnsureCapacity(characterIndex + 1);
            return _states[characterIndex];
        }

        public float GetAlphaFloor(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= _alphaFloors.Length)
            {
                return 0f;
            }

            return _alphaFloors[characterIndex];
        }

        public void AccumulateAlphaFloor(int characterIndex, float alpha)
        {
            if (characterIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(characterIndex));
            }

            EnsureCapacity(characterIndex + 1);
            if (alpha > _alphaFloors[characterIndex])
            {
                _alphaFloors[characterIndex] = alpha;
            }
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
            Array.Resize(ref _alphaFloors, newLength);

            for (var i = previousLength; i < _states.Length; i++)
            {
                _states[i] = new ColorCharacterState();
                _alphaFloors[i] = 0f;
            }
        }

        private static int CalculateNewLength(int currentLength, int requiredLength)
        {
            var newLength = Math.Max(4, currentLength == 0 ? requiredLength : currentLength);
            while (newLength < requiredLength)
            {
                newLength = newLength * 2;
            }

            return newLength;
        }
    }
}
