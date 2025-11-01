namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Base contract for timeline instructions.
    /// </summary>
    public interface IPlaybackInstruction
    {
        /// <summary>
        /// Gets the duration of the instruction in seconds.
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// Executes the instruction using the specified playback session control surface.
        /// </summary>
        /// <param name="sessionControl">The playback session control interface.</param>
        void Execute(IPlaybackSessionControl sessionControl);
    }
}