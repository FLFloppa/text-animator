using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Generic ScriptableObject base class producing a strongly typed parameter definition.
    /// </summary>
    /// <typeparam name="TValue">Type produced by the definition.</typeparam>
    public abstract class ParameterDefinitionAsset<TValue> : BaseParameterDefinitionAsset
    {
        [SerializeField] private List<string> identifiers = new List<string>();
        [SerializeField] private TValue defaultValue;

        protected abstract TValue Convert(string rawValue);

        public override IParameterDefinition BuildRuntimeDefinition()
        {
            return new ParameterDefinition<TValue>(identifiers, defaultValue, Convert);
        }
    }
}