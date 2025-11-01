using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="ShakeTagHandler"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "ShakeTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Shake", order = 610)]
    public sealed class ShakeTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset amplitude;
        [SerializeField] private FloatParameterDefinitionAsset frequency;
        [SerializeField] private BoolParameterDefinitionAsset synchronize;

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            var amplitudeParam = RequireParameter<IParameterDefinition<float>>(amplitude);
            var frequencyParam = RequireParameter<IParameterDefinition<float>>(frequency);
            var synchronizeParam = RequireParameter<IParameterDefinition<bool>>(synchronize);
            return new ShakeTagHandler(amplitudeParam, frequencyParam, synchronizeParam);
        }
    }
}
