namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Waits for the specified duration without performing additional actions.
    /// </summary>
    public sealed class WaitInstruction : IPlaybackInstruction
    {
        public float Duration { get; }

        public WaitInstruction(float duration)
        {
            Duration = duration;
        }

        public void Execute(IPlaybackSessionControl sessionControl)
        {
            // No-op. Waiting is handled in the scheduler.
        }
    }
}