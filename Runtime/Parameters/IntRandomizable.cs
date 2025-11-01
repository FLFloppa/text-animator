using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Randomizable integer value using Unity's random number generation utilities.
    /// </summary>
    public sealed class IntRandomizable : RandomizableValue<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntRandomizable"/> class.
        /// </summary>
        /// <param name="defaultValue">The default value used when randomization is disabled.</param>
        /// <param name="useRandom">Determines whether randomization should be used.</param>
        /// <param name="minValue">The inclusive minimum random value.</param>
        /// <param name="maxValue">The inclusive maximum random value.</param>
        public IntRandomizable(int defaultValue, bool useRandom, int minValue, int maxValue)
            : base(defaultValue, useRandom, minValue, maxValue)
        {
        }

        /// <inheritdoc />
        protected override int GetRandomValue()
        {
            return Random.Range(MinValue, MaxValue + 1);
        }
    }
}
