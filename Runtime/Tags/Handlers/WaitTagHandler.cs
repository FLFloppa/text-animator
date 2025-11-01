using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Playback;
using FLFloppa.TextAnimator.Document;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Inserts a wait instruction into the playback timeline.
    /// </summary>
    public sealed class WaitTagHandler : IPlaybackControlHandler
    {
        private readonly IParameterDefinition<float> _durationParameter;

        public WaitTagHandler(IParameterDefinition<float> durationParameter)
        {
            _durationParameter = durationParameter;
        }

        public void Apply(TagNode node, PlaybackTimelineBuilder timelineBuilder)
        {
            var duration = Mathf.Max(0f, _durationParameter.Parse(node.Attributes));
            timelineBuilder.AddInstruction(new WaitInstruction(duration));
            timelineBuilder.ProcessChildrenWithInheritance(node);
        }
    }
}
