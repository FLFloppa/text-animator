using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Assets.Actions;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    [CreateAssetMenu(fileName = "CharByCharTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Char By Char", order = 610)]
    public sealed class CharByCharTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private FloatParameterDefinitionAsset perCharacterDuration;
        [SerializeField] private IntRandomizableParameterDefinitionAsset revealCount;
        [SerializeField] private ActionAsset onCharacterAction;

        public override ITagHandler BuildHandler()
        {
            var durationParam = RequireParameter<IParameterDefinition<float>>(perCharacterDuration);
            var countParam = RequireParameter<IParameterDefinition<IntRandomizable>>(revealCount);
            var action = onCharacterAction != null ? onCharacterAction.BuildAction() : null;
            return new CharByCharTagHandler(durationParam, countParam, action);
        }
    }
}
