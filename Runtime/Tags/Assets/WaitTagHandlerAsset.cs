using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    [CreateAssetMenu(fileName = "WaitTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Wait", order = 610)]
    public sealed class WaitTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset duration;

        public override ITagHandler BuildHandler()
        {
            var durationParam = RequireParameter<IParameterDefinition<float>>(duration);
            return new WaitTagHandler(durationParam);
        }
    }
}
