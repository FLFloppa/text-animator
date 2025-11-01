using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    [CreateAssetMenu(fileName = "WaveTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Wave", order = 610)]
    public sealed class WaveTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset amplitude;
        [SerializeField] private FloatParameterDefinitionAsset frequency;
        [SerializeField] private FloatParameterDefinitionAsset phase;

        public override ITagHandler BuildHandler()
        {
            var amplitudeParam = RequireParameter<IParameterDefinition<float>>(amplitude);
            var frequencyParam = RequireParameter<IParameterDefinition<float>>(frequency);
            var phaseParam = RequireParameter<IParameterDefinition<float>>(phase);
            return new WaveTagHandler(amplitudeParam, frequencyParam, phaseParam);
        }
    }
}
