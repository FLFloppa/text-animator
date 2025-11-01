using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Assets.Actions;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="WordByWordTagHandler"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "WordByWordTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Word By Word", order = 610)]
    public sealed class WordByWordTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset perWordDuration;
        [SerializeField] private IntRandomizableParameterDefinitionAsset wordsPerBatch;
        [SerializeField] private ActionAsset onWordAction;

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            var durationParam = RequireParameter<IParameterDefinition<float>>(perWordDuration);
            var countParam = RequireParameter<IParameterDefinition<IntRandomizable>>(wordsPerBatch);
            var action = onWordAction != null ? onWordAction.BuildAction() : null;
            return new WordByWordTagHandler(durationParam, countParam, action);
        }
    }
}
