using FLFloppa.TextAnimator.Tags.Assets.Actions;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="ActionTagHandler"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Action", order = 610)]
    public sealed class ActionTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private ActionAsset action;

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            if (action == null)
            {
                throw new System.InvalidOperationException("ActionTagHandlerAsset requires an ActionAsset to be assigned.");
            }

            var actionDelegate = action.BuildAction();
            return new ActionTagHandler(actionDelegate);
        }
    }
}
