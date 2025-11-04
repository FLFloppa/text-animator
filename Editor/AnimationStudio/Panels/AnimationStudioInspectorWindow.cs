using System;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Panels;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Dockable editor window hosting the tag inspector panel for the Animation Studio workspace.
    /// </summary>
    internal sealed class AnimationStudioInspectorWindow : EditorWindow
    {
        private AnimationStudioInspectorPanel? _panel;
        private Action<VisualElement>? _contentBuilder;

        /// <summary>
        /// Opens the inspector window and assigns the content builder callback.
        /// </summary>
        /// <param name="contentBuilder">Callback used to populate the inspector UI.</param>
        public static AnimationStudioInspectorWindow Open(Action<VisualElement> contentBuilder)
        {
            if (contentBuilder == null)
            {
                throw new ArgumentNullException(nameof(contentBuilder));
            }

            var window = GetWindow<AnimationStudioInspectorWindow>();
            window.Initialize(contentBuilder);
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Tag Inspector");
            minSize = new Vector2(280f, 260f);

            if (_panel == null)
            {
                BuildUi();
                Refresh();
            }
        }

        /// <summary>
        /// Assigns the inspector content builder and rebuilds the UI.
        /// </summary>
        /// <param name="contentBuilder">Callback used to populate inspector content.</param>
        public void Initialize(Action<VisualElement> contentBuilder)
        {
            _contentBuilder = contentBuilder ?? throw new ArgumentNullException(nameof(contentBuilder));
            BuildUi();
            Refresh();
        }

        /// <summary>
        /// Forces the inspector to rebuild its content.
        /// </summary>
        public void Refresh()
        {
            if (_panel == null)
            {
                return;
            }

            _panel.SetContent(_contentBuilder ?? (_ => { }));
        }

        private void BuildUi()
        {
            rootVisualElement.Clear();

            _panel = new AnimationStudioInspectorPanel();
            rootVisualElement.Add(_panel.Root);
        }
    }
}
