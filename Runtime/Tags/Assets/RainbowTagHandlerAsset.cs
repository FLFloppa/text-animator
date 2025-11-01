using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="RainbowTagHandler"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "RainbowTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Rainbow", order = 610)]
    public sealed class RainbowTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset duration;
        [SerializeField] private BoolParameterDefinitionAsset loop;
        [SerializeField] private FloatParameterDefinitionAsset colorShift;
        [SerializeField] private Gradient gradient;

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            var durationParameter = RequireParameter<IParameterDefinition<float>>(duration);
            var loopParameter = RequireParameter<IParameterDefinition<bool>>(loop);
            var colorShiftParameter = RequireParameter<IParameterDefinition<float>>(colorShift);
            return new RainbowTagHandler(durationParameter, loopParameter, colorShiftParameter, gradient);
        }
    }
}
