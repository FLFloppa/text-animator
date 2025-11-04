using System.Diagnostics.CodeAnalysis;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Describes the playback lifecycle states of the Animation Studio preview.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal enum AnimationPlaybackState
    {
        /// <summary>
        /// No preview session is currently active.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// A preview session is actively playing.
        /// </summary>
        Playing = 1,

        /// <summary>
        /// Playback is paused but can be resumed without rebuilding the animation.
        /// </summary>
        Paused = 2
    }
}
