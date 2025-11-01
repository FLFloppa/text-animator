using System.Collections.Generic;

namespace FLFloppa.TextAnimator.Characters.States
{
    /// <summary>
    /// Represents material override state for a single character.
    /// </summary>
    public sealed class MaterialCharacterState
    {
        private readonly Dictionary<int, float> _floatOverrides = new Dictionary<int, float>();

        /// <summary>
        /// Gets the float shader property overrides keyed by property ID.
        /// </summary>
        public IReadOnlyDictionary<int, float> FloatOverrides => _floatOverrides;

        /// <summary>
        /// Sets a float override.
        /// </summary>
        /// <param name="propertyId">The shader property ID.</param>
        /// <param name="value">The value to assign.</param>
        public void SetFloat(int propertyId, float value)
        {
            _floatOverrides[propertyId] = value;
        }

        /// <summary>
        /// Clears all overrides.
        /// </summary>
        public void Reset()
        {
            _floatOverrides.Clear();
        }
    }
}
