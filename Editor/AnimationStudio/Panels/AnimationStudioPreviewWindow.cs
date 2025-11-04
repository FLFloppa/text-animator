using System;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Panels;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Dockable editor window hosting the preview panel for the Animation Studio workspace.
    /// </summary>
    internal sealed class AnimationStudioPreviewWindow : EditorWindow
    {
        private IAnimationStudioWorkspaceCoordinator? _coordinator;
        private AnimationStudioPreviewPanel? _panel;

        /// <summary>
        /// Opens the preview window and assigns the specified coordinator.
        /// </summary>
        /// <param name="coordinator">Coordinator that exposes shared workspace services.</param>
        public static AnimationStudioPreviewWindow Open(IAnimationStudioWorkspaceCoordinator coordinator)
        {
            if (coordinator == null)
            {
                throw new ArgumentNullException(nameof(coordinator));
            }

            var window = GetWindow<AnimationStudioPreviewWindow>();
            window.Initialize(coordinator);
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Preview");
            minSize = new Vector2(320f, 240f);

            if (_coordinator != null && _panel == null)
            {
                BuildUi();
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
        }

        private void BuildUi()
        {
            rootVisualElement.Clear();

            if (_coordinator == null)
            {
                rootVisualElement.Add(new HelpBox("Animation Studio coordinator unavailable. Open the Animation Studio window to reinitialize.", HelpBoxMessageType.Warning));
                _panel = null;
                return;
            }

            _panel = new AnimationStudioPreviewPanel(() => _coordinator.RequestPreviewRebuild());
            rootVisualElement.Add(_panel.Root);
        }

        /// <summary>
        /// Updates the preview IMGUI callback to render the provided GUI handler.
        /// </summary>
        /// <param name="guiHandler">Handler invoked by the IMGUI container.</param>
        public void SetPreviewGuiHandler(Action guiHandler)
        {
            if (guiHandler == null)
            {
                throw new ArgumentNullException(nameof(guiHandler));
            }

            if (_panel == null)
            {
                return;
            }

            _panel.Host.onGUIHandler = guiHandler;
        }
    }
}
