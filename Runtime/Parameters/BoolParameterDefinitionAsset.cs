using System;
using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Creates boolean parameter definitions with configurable true/false tokens.
    /// </summary>
    [CreateAssetMenu(fileName = "BoolParameter", menuName = "FLFloppa/Text Animator/Parameters/Bool Parameter", order = 610)]
    public sealed class BoolParameterDefinitionAsset : BaseParameterDefinitionAsset
    {
        [SerializeField] private List<string> identifiers = new List<string>();
        [SerializeField] private bool defaultValue;
        [SerializeField] private List<string> trueValues = new List<string> { "true", "1", "yes", "on" };
        [SerializeField] private List<string> falseValues = new List<string> { "false", "0", "no", "off" };
        [SerializeField] private bool ignoreCase = true;

        /// <inheritdoc />
        public override IParameterDefinition BuildRuntimeDefinition()
        {
            if (identifiers == null || identifiers.Count == 0)
            {
                throw new InvalidOperationException("At least one identifier must be provided for a bool parameter definition.");
            }

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var normalizedTrue = Normalize(trueValues);
            var normalizedFalse = Normalize(falseValues);
            var defaultValueSnapshot = defaultValue;

            bool Converter(string rawValue)
            {
                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    return defaultValueSnapshot;
                }

                var trimmed = rawValue.Trim();

                if (MatchesAny(trimmed, normalizedTrue, comparison))
                {
                    return true;
                }

                if (MatchesAny(trimmed, normalizedFalse, comparison))
                {
                    return false;
                }

                if (bool.TryParse(trimmed, out var parsed))
                {
                    return parsed;
                }

                return defaultValueSnapshot;
            }

            return new ParameterDefinition<bool>(identifiers, defaultValueSnapshot, Converter);
        }

        private static string[] Normalize(List<string> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<string>();
            }

            var result = new string[source.Count];
            for (var i = 0; i < source.Count; i++)
            {
                result[i] = source[i]?.Trim() ?? string.Empty;
            }

            return result;
        }

        private static bool MatchesAny(string value, string[] candidates, StringComparison comparison)
        {
            foreach (var candidate in candidates)
            {
                if (string.Equals(value, candidate, comparison))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
