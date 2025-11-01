using System.Collections.Generic;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Strongly typed parameter definition contract.
    /// </summary>
    /// <typeparam name="TValue">Type of the parameter value.</typeparam>
    public interface IParameterDefinition<TValue> : IParameterDefinition
    {
        TValue Parse(IReadOnlyDictionary<string, string> attributes);
    }

    /// <summary>
    /// Non-generic parameter definition contract for storage inside ScriptableObjects.
    /// </summary>
    public interface IParameterDefinition
    {
        IReadOnlyList<string> Identifiers { get; }
    }
}