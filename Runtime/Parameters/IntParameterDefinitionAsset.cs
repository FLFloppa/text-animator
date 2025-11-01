using UnityEngine;

namespace FLFloppa.TextAnimator.Parameters
{
    /// <summary>
    /// Integer parameter definition asset.
    /// </summary>
    [CreateAssetMenu(fileName = "IntParameter", menuName = "FLFloppa/Text Animator/Parameters/Int Parameter", order = 610)]
    public sealed class IntParameterDefinitionAsset : ParameterDefinitionAsset<int>
    {
        protected override int Convert(string rawValue) => int.Parse(rawValue, System.Globalization.CultureInfo.InvariantCulture);
    }
}