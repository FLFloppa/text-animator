using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Produces parameter definitions that parse <see cref="IntRandomizable"/> values from markup attributes.
    /// </summary>
    [CreateAssetMenu(fileName = "IntRandomizableParameter", menuName = "FLFloppa/Text Animator/Parameters/Int Randomizable Parameter", order = 610)]
    public sealed class IntRandomizableParameterDefinitionAsset : BaseParameterDefinitionAsset
    {
        [SerializeField] private List<string> identifiers = new List<string>();
        [SerializeField] private int defaultCount = 1;
        [SerializeField] private bool defaultUseRandom;
        [SerializeField] private int defaultRandomMin = 1;
        [SerializeField] private int defaultRandomMax = 1;

        /// <inheritdoc />
        public override IParameterDefinition BuildRuntimeDefinition()
        {
            if (identifiers == null || identifiers.Count == 0)
            {
                throw new InvalidOperationException("At least one identifier must be provided for an IntRandomizable parameter definition.");
            }

            var defaultValue = CreateRandomizable(defaultCount, defaultUseRandom, defaultRandomMin, defaultRandomMax);
            return new ParameterDefinition<IntRandomizable>(identifiers, defaultValue, ParseValue);
        }

        private IntRandomizable ParseValue(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return CreateRandomizable(defaultCount, defaultUseRandom, defaultRandomMin, defaultRandomMax);
            }

            var trimmed = rawValue.Trim();
            var rangeSeparatorIndex = trimmed.IndexOf('-');
            if (rangeSeparatorIndex >= 0)
            {
                var minPart = trimmed[..rangeSeparatorIndex];
                var maxPart = trimmed[(rangeSeparatorIndex + 1)..];
                if (!TryParse(minPart, out var min) || !TryParse(maxPart, out var max))
                    return CreateRandomizable(defaultCount, defaultUseRandom, defaultRandomMin, defaultRandomMax);
                
                if (min > max)
                {
                    (min, max) = (max, min);
                }

                return CreateRandomizable(defaultCount, true, min, max);
            }
            else if (TryParse(trimmed, out var value))
            {
                return CreateRandomizable(value, false, value, value);
            }

            return CreateRandomizable(defaultCount, defaultUseRandom, defaultRandomMin, defaultRandomMax);
        }

        private static bool TryParse(string input, out int value)
        {
            return int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static IntRandomizable CreateRandomizable(int @default, bool useRandom, int min, int max)
        {
            var effectiveDefault = Mathf.Max(1, @default);
            var effectiveMin = Mathf.Max(1, Math.Min(min, max));
            var effectiveMax = Mathf.Max(effectiveMin, Math.Max(min, max));

            if (!useRandom)
            {
                effectiveMin = effectiveDefault;
                effectiveMax = effectiveDefault;
            }

            return new IntRandomizable(effectiveDefault, useRandom, effectiveMin, effectiveMax);
        }
    }
}
