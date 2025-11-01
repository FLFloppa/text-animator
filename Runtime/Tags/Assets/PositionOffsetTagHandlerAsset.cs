using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="PositionOffsetTagHandler"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "PositionOffsetTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Position Offset", order = 610)]
    public sealed class PositionOffsetTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset duration;
        [SerializeField] private BoolParameterDefinitionAsset loop;
        [SerializeField] private BoolParameterDefinitionAsset @override;
        [SerializeField] private AnimationCurve positionXCurve = AnimationCurve.Constant(0f, 1f, 0f);
        [SerializeField] private AnimationCurve positionYCurve = AnimationCurve.Constant(0f, 1f, 0f);
        [SerializeField] private AnimationCurve positionZCurve = AnimationCurve.Constant(0f, 1f, 0f);

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            var durationParameter = RequireParameter<IParameterDefinition<float>>(duration);
            var loopParameter = RequireParameter<IParameterDefinition<bool>>(loop);
            var overrideParameter = RequireParameter<IParameterDefinition<bool>>(@override);
            return new PositionOffsetTagHandler(durationParameter, loopParameter, overrideParameter, positionXCurve, positionYCurve, positionZCurve);
        }
    }
}
