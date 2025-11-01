using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Float parameter definition asset.
    /// </summary>
    [CreateAssetMenu(fileName = "FloatParameter", menuName = "FLFloppa/Text Animator/Parameters/Float Parameter", order = 610)]
    public sealed class FloatParameterDefinitionAsset : ParameterDefinitionAsset<float>
    {
        protected override float Convert(string rawValue) => float.Parse(rawValue, System.Globalization.CultureInfo.InvariantCulture);
    }
}