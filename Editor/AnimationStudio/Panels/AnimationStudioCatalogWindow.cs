using System;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Panels;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Dockable editor window hosting the Animation Studio catalog panel.
    /// </summary>
    internal sealed class AnimationStudioCatalogWindow : EditorWindow
    {
        private IAnimationStudioWorkspaceCoordinator? _coordinator;
        private AnimationStudioCatalogPanel? _panel;

        /// <summary>
        /// Occurs when a handler tile is activated via click inside the catalog.
        /// </summary>
        public event Action<TagHandlerAsset>? HandlerInvoked;

        /// <summary>
        /// Opens the catalog window and assigns the specified coordinator.
        /// </summary>
        /// <param name="coordinator">Coordinator that exposes shared workspace services.</param>
        public static AnimationStudioCatalogWindow Open(IAnimationStudioWorkspaceCoordinator coordinator)
        {
            if (coordinator == null)
            {
                throw new ArgumentNullException(nameof(coordinator));
            }

            var window = GetWindow<AnimationStudioCatalogWindow>();
            window.Initialize(coordinator);
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Effects Catalog");
            minSize = new Vector2(260f, 260f);

            if (_coordinator != null && _panel == null)
            {
                BuildUi();
                RefreshCatalog();
            }
        }

        /// <summary>
        /// Associates the window with a workspace coordinator and rebuilds the UI.
        /// </summary>
        /// <param name="coordinator">Coordinator providing workspace services.</param>
        public void Initialize(IAnimationStudioWorkspaceCoordinator coordinator)
        {
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            BuildUi();
            RefreshCatalog();
        }

        public void RefreshCatalog()
        {
            if (_panel == null)
            {
                return;
            }

            _panel.Refresh();
        }

        private void BuildUi()
        {
            rootVisualElement.Clear();

            if (_panel != null)
            {
                _panel.HandlerInvoked -= OnHandlerInvoked;
            }

            if (_coordinator == null)
            {
                rootVisualElement.Add(new HelpBox("Animation Studio coordinator unavailable. Open the Animation Studio window to reinitialize.", HelpBoxMessageType.Warning));
                _panel = null;
                return;
            }

            _panel = new AnimationStudioCatalogPanel(_coordinator);
            _panel.HandlerInvoked += OnHandlerInvoked;
            rootVisualElement.Add(_panel.Root);
        }

        private void OnHandlerInvoked(TagHandlerAsset handler)
        {
            HandlerInvoked?.Invoke(handler);
        }
    }
}
