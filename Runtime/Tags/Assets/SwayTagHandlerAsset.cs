using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="SwayTagHandler"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "SwayTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Sway", order = 610)]
    public sealed class SwayTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset frequency;
        [SerializeField] private FloatParameterDefinitionAsset amplitude;
        [SerializeField] private BoolParameterDefinitionAsset grouped;
        [SerializeField] private FloatParameterDefinitionAsset phaseOffset;

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            var frequencyParam = RequireParameter<IParameterDefinition<float>>(frequency);
            var amplitudeParam = RequireParameter<IParameterDefinition<float>>(amplitude);
            var groupedParam = RequireParameter<IParameterDefinition<bool>>(grouped);
            var phaseOffsetParam = RequireParameter<IParameterDefinition<float>>(phaseOffset);
            return new SwayTagHandler(frequencyParam, amplitudeParam, groupedParam, phaseOffsetParam);
        }
    }
}
