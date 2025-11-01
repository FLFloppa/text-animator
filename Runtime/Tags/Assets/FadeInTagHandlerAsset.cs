using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    [CreateAssetMenu(fileName = "FadeInTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Fade In", order = 610)]
    public sealed class FadeInTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset duration;
        [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public override ITagHandler BuildHandler()
        {
            var durationParam = RequireParameter<IParameterDefinition<float>>(duration);
            return new FadeInTagHandler(durationParam, alphaCurve);
        }
    }
}
