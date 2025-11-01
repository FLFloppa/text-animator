using System;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Represents a value that can either use a default value or be randomized.
    /// </summary>
    /// <typeparam name="T">The type of value managed by this randomizable instance.</typeparam>
    public abstract class RandomizableValue<T>
        where T : struct, IComparable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RandomizableValue{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">The default value used when random mode is disabled.</param>
        /// <param name="useRandom">Determines whether a randomized value should be produced.</param>
        /// <param name="minValue">The inclusive minimum value for the randomized range.</param>
        /// <param name="maxValue">The inclusive maximum value for the randomized range.</param>
        protected RandomizableValue(T defaultValue, bool useRandom, T minValue, T maxValue)
        {
            DefaultValue = defaultValue;
            UseRandom = useRandom;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <summary>
        /// Gets the default value used when randomization is disabled.
        /// </summary>
        public T DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether randomization should be used.
        /// </summary>
        public bool UseRandom { get; }

        /// <summary>
        /// Gets the inclusive minimum value of the randomization range.
        /// </summary>
        public T MinValue { get; }

        /// <summary>
        /// Gets the inclusive maximum value of the randomization range.
        /// </summary>
        public T MaxValue { get; }

        /// <summary>
        /// Gets the resulting value, using either the default or randomized value depending on <see cref="UseRandom"/>.
        /// </summary>
        public T Value => UseRandom ? GetRandomValue() : DefaultValue;

        /// <summary>
        /// Generates a random value within the specified range.
        /// </summary>
        /// <returns>The randomized value.</returns>
        protected abstract T GetRandomValue();
    }
}
