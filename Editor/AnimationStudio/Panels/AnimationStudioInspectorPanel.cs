using System;
using FLFloppa.EditorHelpers;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Panels
{
    /// <summary>
    /// UI Toolkit panel displaying inspector content for the currently selected markup tag.
    /// </summary>
    internal sealed class AnimationStudioInspectorPanel
    {
        private readonly ScrollView _scrollView;
        private readonly VisualElement _contentRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationStudioInspectorPanel"/> class.
        /// </summary>
        public AnimationStudioInspectorPanel()
        {
            Root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    paddingLeft = 6f,
                    paddingRight = 6f,
                    paddingTop = 6f,
                    paddingBottom = 6f
                }
            };

            var header = InspectorUi.Layout.CreateHeader("Tag Inspector", "Select a tag to edit its parameters.");
            Root.Add(header);

            _scrollView = new ScrollView
            {
                style =
                {
                    flexGrow = 1f,
                    marginTop = 6f
                }
            };
            Root.Add(_scrollView);

            _contentRoot = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1f
                }
            };
            _scrollView.Add(_contentRoot);
        }

        /// <summary>
        /// Gets the root visual element hosting the inspector UI.
        /// </summary>
        public VisualElement Root { get; }

        /// <summary>
        /// Gets the container used to populate inspector content.
        /// </summary>
        public ScrollView Content => _scrollView;

        /// <summary>
        /// Replaces inspector content using the provided builder callback.
        /// </summary>
        /// <param name="builder">Callback responsible for creating inspector elements inside the supplied container.</param>
        public void SetContent(Action<VisualElement> builder)
        {
            builder?.Invoke(_contentRoot);
        }
    }
}
