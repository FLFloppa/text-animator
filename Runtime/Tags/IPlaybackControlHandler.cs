using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Playback;

namespace FLFloppa.TextAnimator.Tags
{
    /// <summary>
    /// Handles timeline shaping tags.
    /// </summary>
    public interface IPlaybackControlHandler : ITagHandler
    {
        void Apply(TagNode node, PlaybackTimelineBuilder timelineBuilder);
    }
}