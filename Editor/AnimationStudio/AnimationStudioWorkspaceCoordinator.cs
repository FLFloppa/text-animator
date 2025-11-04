using System;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Defines the shared operations and state accessors exposed to the Animation Studio auxiliary windows.
    /// </summary>
    internal interface IAnimationStudioWorkspaceCoordinator
    {
        /// <summary>
        /// Gets the persisted workspace state backing the animation studio.
        /// </summary>
        AnimationStudioState State { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the preview rebuilds automatically when markup or configuration changes.
        /// </summary>
        bool IsAutoRefreshingPreview { get; set; }

        /// <summary>
        /// Gets the current markup content.
        /// </summary>
        string CurrentMarkup { get; }

        /// <summary>
        /// Updates the markup content and optionally requests a preview restart.
        /// </summary>
        /// <param name="markup">The new markup string.</param>
        /// <param name="requestPreviewRestart">Whether the preview should be rebuilt after the update.</param>
        void UpdateMarkup(string markup, bool requestPreviewRestart);

        /// <summary>
        /// Requests that the preview be rebuilt.
        /// </summary>
        void RequestPreviewRebuild();

        /// <summary>
        /// Refreshes the catalog display from the active tag handler registry.
        /// </summary>
        void RefreshCatalog();

        /// <summary>
        /// Starts preview playback if possible.
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses preview playback if running.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops preview playback if running.
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// Default implementation of <see cref="IAnimationStudioWorkspaceCoordinator"/> that bridges auxiliary windows to the main animation studio window.
    /// </summary>
    internal sealed class AnimationStudioWorkspaceCoordinator : IAnimationStudioWorkspaceCoordinator
    {
        private readonly AnimationStudioWindow _window;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationStudioWorkspaceCoordinator"/> class.
        /// </summary>
        /// <param name="window">The owning animation studio window.</param>
        public AnimationStudioWorkspaceCoordinator(AnimationStudioWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        /// <inheritdoc />
        public AnimationStudioState State => _window.State;

        /// <inheritdoc />
        public bool IsAutoRefreshingPreview
        {
            get => _window.IsPreviewAutoRefreshing;
            set => _window.SetPreviewAutoRefreshing(value);
        }

        /// <inheritdoc />
        public string CurrentMarkup => _window.CurrentMarkup;

        /// <inheritdoc />
        public void UpdateMarkup(string markup, bool requestPreviewRestart)
        {
            _window.SetMarkup(markup, requestPreviewRestart);
        }

        /// <inheritdoc />
        public void RequestPreviewRebuild()
        {
            _window.RequestPreviewRebuild();
        }

        /// <inheritdoc />
        public void RefreshCatalog()
        {
            _window.RefreshCatalog();
        }

        /// <inheritdoc />
        public void Play()
        {
            _window.PlayPreview();
        }

        /// <inheritdoc />
        public void Pause()
        {
            _window.PausePreview();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _window.StopPreview();
        }
    }
}
