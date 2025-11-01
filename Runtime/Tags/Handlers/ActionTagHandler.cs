using System;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Playback;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Invokes an action when the tag is processed during timeline building.
    /// </summary>
    public sealed class ActionTagHandler : IPlaybackControlHandler
    {
        private readonly Action<string> _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionTagHandler"/> class.
        /// </summary>
        /// <param name="action">The action to invoke when the tag is processed.</param>
        public ActionTagHandler(Action<string> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <inheritdoc />
        public void Apply(TagNode node, PlaybackTimelineBuilder timelineBuilder)
        {
            _action?.Invoke(null);
            timelineBuilder.ProcessChildrenWithInheritance(node);
        }
    }
}
