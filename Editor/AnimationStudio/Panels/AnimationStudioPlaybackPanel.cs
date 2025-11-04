using System;
using FLFloppa.EditorHelpers;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Panels
{
    /// <summary>
    /// UI Toolkit panel exposing playback controls for the Animation Studio preview.
    /// </summary>
    internal sealed class AnimationStudioPlaybackPanel
    {
        private readonly IAnimationStudioWorkspaceCoordinator _coordinator;
        private readonly Button _playButton;
        private readonly Button _pauseButton;
        private readonly Button _stopButton;
        private readonly Toggle _autoRefreshToggle;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationStudioPlaybackPanel"/> class.
        /// </summary>
        /// <param name="coordinator">Coordinator providing workspace services.</param>
        public AnimationStudioPlaybackPanel(IAnimationStudioWorkspaceCoordinator coordinator)
        {
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));

            Root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                    paddingTop = 8f,
                    paddingBottom = 8f,
                    paddingLeft = 8f,
                    paddingRight = 8f
                }
            };

            var controlsRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center
                }
            };
            Root.Add(controlsRow);

            _playButton = InspectorUi.Controls.CreateActionButton("Play", () => _coordinator.Play());
            ConfigureButton(_playButton);
            _playButton.style.marginRight = InspectorUi.Layout.ButtonSpacing;
            controlsRow.Add(_playButton);

            _pauseButton = InspectorUi.Controls.CreateActionButton("Pause", () => _coordinator.Pause());
            ConfigureButton(_pauseButton);
            _pauseButton.style.marginRight = InspectorUi.Layout.ButtonSpacing;
            controlsRow.Add(_pauseButton);

            _stopButton = InspectorUi.Controls.CreateActionButton("Stop", () => _coordinator.Stop());
            ConfigureButton(_stopButton);
            controlsRow.Add(_stopButton);

            _autoRefreshToggle = new Toggle("Auto Refresh Preview")
            {
                value = coordinator.IsAutoRefreshingPreview,
                style =
                {
                    marginTop = 8f
                }
            };
            _autoRefreshToggle.RegisterValueChangedCallback(evt => _coordinator.IsAutoRefreshingPreview = evt.newValue);
            Root.Add(_autoRefreshToggle);
        }

        /// <summary>
        /// Gets the root visual element of the panel.
        /// </summary>
        public VisualElement Root { get; }

        /// <summary>
        /// Updates the transport controls to reflect the provided playback state.
        /// </summary>
        /// <param name="isPlaying">Indicates whether playback is currently running.</param>
        /// <param name="isPaused">Indicates whether playback is paused.</param>
        public void UpdateTransportState(bool isPlaying, bool isPaused)
        {
            _playButton.text = isPaused ? "Resume" : "Play";
            _pauseButton.SetEnabled(isPlaying);
            _stopButton.SetEnabled(isPlaying || isPaused);
        }

        /// <summary>
        /// Synchronizes the auto-refresh toggle with the active setting.
        /// </summary>
        /// <param name="value">Current auto-refresh value.</param>
        public void SetAutoRefresh(bool value)
        {
            _autoRefreshToggle.SetValueWithoutNotify(value);
        }

        private static void ConfigureButton(Button button)
        {
            button.style.minWidth = 120f;
            button.style.marginBottom = 0f;
        }
    }
}
