using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// String parameter definition asset.
    /// </summary>
    [CreateAssetMenu(fileName = "StringParameter", menuName = "FLFloppa/Text Animator/Parameters/String Parameter", order = 610)]
    public sealed class StringParameterDefinitionAsset : ParameterDefinitionAsset<string>
    {
        protected override string Convert(string rawValue) => rawValue;
    }
}