using System.Collections.Generic;
using FLFloppa.TextAnimator.Tags.Handlers;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject wrapper producing <see cref="CompositeTagHandler"/> instances that combine multiple tag handlers.
    /// </summary>
    [CreateAssetMenu(fileName = "CompositeTagHandler", menuName = "FLFloppa/Text Animator/Handlers/Composite", order = 610)]
    public sealed class CompositeTagHandlerAsset : TagHandlerAsset
    {
        [SerializeField] private TagHandlerAsset[] childHandlers = System.Array.Empty<TagHandlerAsset>();

        /// <inheritdoc />
        public override ITagHandler BuildHandler()
        {
            if (childHandlers == null || childHandlers.Length == 0)
            {
                Debug.LogWarning($"CompositeTagHandlerAsset '{name}' has no child handlers assigned.", this);
                return null;
            }

            var childModifierProviders = new List<ICharacterModifierProvider>(childHandlers.Length);

            for (var i = 0; i < childHandlers.Length; i++)
            {
                var childAsset = childHandlers[i];
                if (childAsset == null)
                {
                    Debug.LogWarning($"CompositeTagHandlerAsset '{name}' has a null child handler at index {i}.", this);
                    continue;
                }

                var handler = childAsset.BuildHandler();
                if (handler is ICharacterModifierProvider modifierProvider)
                {
                    childModifierProviders.Add(modifierProvider);
                }
                else if (handler != null)
                {
                    Debug.LogWarning($"Child handler at index {i} in CompositeTagHandlerAsset '{name}' is not an ICharacterModifierProvider. Only character modifier providers can be combined.", this);
                }
            }

            if (childModifierProviders.Count == 0)
            {
                Debug.LogWarning($"CompositeTagHandlerAsset '{name}' has no valid child modifier providers.", this);
                return null;
            }

            return new CompositeTagHandler(childModifierProviders);
        }
    }
}
