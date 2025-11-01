using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="RotateTagHandler"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "RotateTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Rotate", order = 610)]
    public sealed class RotateTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset duration;
        [SerializeField] private BoolParameterDefinitionAsset loop;
        [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 360f);

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            var durationParameter = RequireParameter<IParameterDefinition<float>>(duration);
            var loopParameter = RequireParameter<IParameterDefinition<bool>>(loop);
            return new RotateTagHandler(durationParameter, loopParameter, rotationCurve);
        }
    }
}
