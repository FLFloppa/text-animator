using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// ScriptableObject registry that composes runtime tag handlers and their aliases.
    /// </summary>
    [CreateAssetMenu(fileName = "TagHandlerRegistry", menuName = "FLFloppa/Text Animator/Handlers/Registry", order = 610)]
    public sealed class TagHandlerRegistryAsset : ScriptableObject
    {
        [SerializeField] private List<TagHandlerAsset> handlers = new List<TagHandlerAsset>();

        /// <summary>
        /// Builds a runtime tag handler factory from the configured assets.
        /// </summary>
        public Tags.ITagHandlerFactory BuildFactory()
        {
            var registrations = new List<TagHandlerRegistration>();
            foreach (var asset in handlers)
            {
                if (asset == null)
                {
                    continue;
                }

                var handlerInstance = asset.BuildHandler();
                foreach (var identifier in asset.TagIdentifiers)
                {
                    if (string.IsNullOrWhiteSpace(identifier))
                    {
                        continue;
                    }

                    registrations.Add(new TagHandlerRegistration(identifier, handlerInstance));
                }
            }

            return new Tags.RuntimeTagHandlerFactory(registrations);
        }
    }
}
