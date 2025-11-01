using System;
using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Runtime parameter definition implementation with alias support.
    /// </summary>
    /// <typeparam name="TValue">Type of the parameter value.</typeparam>
    public sealed class ParameterDefinition<TValue> : IParameterDefinition<TValue>
    {
        public IReadOnlyList<string> Identifiers { get; }
        public TValue DefaultValue { get; }
        private readonly Func<string, TValue> _converter;

        public ParameterDefinition(IReadOnlyList<string> identifiers, TValue defaultValue, Func<string, TValue> converter)
        {
            if (identifiers == null || identifiers.Count == 0)
            {
                throw new ArgumentException("At least one identifier must be provided.", nameof(identifiers));
            }

            Identifiers = identifiers;
            DefaultValue = defaultValue;
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public TValue Parse(IReadOnlyDictionary<string, string> attributes)
        {
            if (attributes == null)
            {
                return DefaultValue;
            }

            foreach (var identifier in Identifiers)
            {
                if (attributes.TryGetValue(identifier, out var valueRaw) && !string.IsNullOrEmpty(valueRaw))
                {
                    try
                    {
                        return _converter(valueRaw);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"Failed to parse parameter '{identifier}' with value '{valueRaw}'. Using default. Error: {exception.Message}");
                        return DefaultValue;
                    }
                }
            }

            return DefaultValue;
        }
    }
}