using FLFloppa.TextAnimator.Tags.Assets;
using UnityEngine;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Catalog
{
    /// <summary>
    /// ScriptableObject describing how an effect should appear within the Animation Studio catalog.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AnimationStudioCatalogItem",
        menuName = "FLFloppa/Text Animator/Animation Studio/Catalog Item",
        order = 1000)]
    internal sealed class AnimationStudioCatalogItemAsset : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Human friendly name shown in the catalog tile and detail panel.")]
        private string displayName = string.Empty;

        [SerializeField]
        [Tooltip("Rich description displayed in the catalog detail panel.")]
        [TextArea]
        private string description = string.Empty;

        [SerializeField]
        [Tooltip("Icon used for the catalog tile when no animated preview is supplied.")]
        private Texture2D? icon;

        [SerializeField]
        [Tooltip("Optional animated image or texture that replaces the icon when assigned.")]
        private Texture2D? previewTexture;

        [SerializeField]
        [Tooltip("Tag handler invoked when this catalog item is dragged or inserted into markup.")]
        private TagHandlerAsset? handler;

        /// <summary>
        /// Gets the display name shown in the catalog.
        /// </summary>
        internal string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        /// <summary>
        /// Gets the description rendered in the catalog detail area.
        /// </summary>
        internal string Description => description ?? string.Empty;

        /// <summary>
        /// Gets the icon texture used when no preview texture is assigned.
        /// </summary>
        internal Texture2D? Icon => icon;

        /// <summary>
        /// Gets the preview texture that overrides the icon when non-null.
        /// </summary>
        internal Texture2D? PreviewTexture => previewTexture;

        /// <summary>
        /// Gets the tag handler associated with this catalog item.
        /// </summary>
        internal TagHandlerAsset? Handler => handler;
    }
}
