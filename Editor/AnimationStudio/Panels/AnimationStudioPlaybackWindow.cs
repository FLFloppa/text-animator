using System;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Panels;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Dockable editor window hosting the playback controls panel for the Animation Studio workspace.
    /// </summary>
    internal sealed class AnimationStudioPlaybackWindow : EditorWindow
    {
        private IAnimationStudioWorkspaceCoordinator? _coordinator;
        private AnimationStudioPlaybackPanel? _panel;

        /// <summary>
        /// Opens the playback window and assigns the specified coordinator.
        /// </summary>
        /// <param name="coordinator">Coordinator that exposes shared workspace services.</param>
        public static AnimationStudioPlaybackWindow Open(IAnimationStudioWorkspaceCoordinator coordinator)
        {
            if (coordinator == null)
            {
                throw new ArgumentNullException(nameof(coordinator));
            }

            var window = GetWindow<AnimationStudioPlaybackWindow>();
            window.Initialize(coordinator);
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Playback");
            minSize = new Vector2(260f, 140f);

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
            SyncState();
        }

        /// <summary>
        /// Updates the playback controls based on the current playback state.
        /// </summary>
        /// <param name="isPlaying">Indicates whether playback is running.</param>
        /// <param name="isPaused">Indicates whether playback is paused.</param>
        public void UpdateTransportState(bool isPlaying, bool isPaused)
        {
            _panel?.UpdateTransportState(isPlaying, isPaused);
        }

        /// <summary>
        /// Synchronizes the auto-refresh toggle with the coordinator.
        /// </summary>
        public void SyncAutoRefresh()
        {
            if (_coordinator == null)
            {
                return;
            }

            _panel?.SetAutoRefresh(_coordinator.IsAutoRefreshingPreview);
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

            _panel = new AnimationStudioPlaybackPanel(_coordinator);
            rootVisualElement.Add(_panel.Root);
        }

        private void SyncState()
        {
            if (_coordinator == null)
            {
                return;
            }

            _panel?.SetAutoRefresh(_coordinator.IsAutoRefreshingPreview);
        }
    }
}
