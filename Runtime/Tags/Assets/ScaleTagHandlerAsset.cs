using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="ScaleTagHandler"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "ScaleTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Scale", order = 610)]
    public sealed class ScaleTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset duration;
        [SerializeField] private BoolParameterDefinitionAsset loop;
        [SerializeField] private AnimationCurve scaleXCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve scaleYCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve scaleZCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            var durationParameter = RequireParameter<IParameterDefinition<float>>(duration);
            var loopParameter = RequireParameter<IParameterDefinition<bool>>(loop);
            return new ScaleTagHandler(durationParameter, loopParameter, scaleXCurve, scaleYCurve, scaleZCurve);
        }
    }
}
