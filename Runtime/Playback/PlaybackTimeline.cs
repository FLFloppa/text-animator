using System.Collections.Generic;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Represents a linearized playback sequence of instructions.
    /// </summary>
    public sealed class PlaybackTimeline
    {
        private readonly List<IPlaybackInstruction> _instructions = new List<IPlaybackInstruction>();

        public IReadOnlyList<IPlaybackInstruction> Instructions => _instructions;
        public float TotalDuration { get; private set; }

        public void AddInstruction(IPlaybackInstruction instruction)
        {
            _instructions.Add(instruction);
            TotalDuration += instruction.Duration;
        }
    }
}
