using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Abstract ScriptableObject base for serializing parameter definitions with aliases.
    /// </summary>
    public abstract class BaseParameterDefinitionAsset : ScriptableObject
    {
        public abstract IParameterDefinition BuildRuntimeDefinition();
    }
}