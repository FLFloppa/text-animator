using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Catalog
{
    /// <summary>
    /// ScriptableObject storing the catalog items displayed within the Animation Studio catalog window.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AnimationStudioCatalogRegistry",
        menuName = "FLFloppa/Text Animator/Animation Studio/Catalog Registry",
        order = 1001)]
    internal sealed class AnimationStudioCatalogRegistryAsset : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Ordered list of catalog groups rendered in the Animation Studio catalog.")]
        private List<AnimationStudioCatalogGroupAsset> groups = new List<AnimationStudioCatalogGroupAsset>();

        /// <summary>
        /// Gets the ordered collection of catalog groups.
        /// </summary>
        internal IReadOnlyList<AnimationStudioCatalogGroupAsset> Groups => groups;
    }
}
