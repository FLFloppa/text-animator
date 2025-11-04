using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Catalog
{
    /// <summary>
    /// ScriptableObject describing a logical grouping of catalog items within the Animation Studio catalog.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AnimationStudioCatalogGroup",
        menuName = "FLFloppa/Text Animator/Animation Studio/Catalog Group",
        order = 1000)]
    internal sealed class AnimationStudioCatalogGroupAsset : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Title displayed for this catalog group inside the Animation Studio catalog.")]
        private string displayName = string.Empty;

        [SerializeField]
        [Tooltip("Ordered list of catalog items contained within this group.")]
        private List<AnimationStudioCatalogItemAsset> items = new List<AnimationStudioCatalogItemAsset>();

        /// <summary>
        /// Gets the title displayed for this catalog group.
        /// </summary>
        internal string DisplayName => displayName ?? string.Empty;

        /// <summary>
        /// Gets the ordered catalog items contained within this group.
        /// </summary>
        internal IReadOnlyList<AnimationStudioCatalogItemAsset> Items => items;
    }
}
