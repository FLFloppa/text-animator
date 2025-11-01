using System;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Playback;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Applies a per-word reveal duration to the contents of the tag.
    /// </summary>
    public sealed class WordByWordTagHandler : IPlaybackControlHandler
    {
        private readonly IParameterDefinition<float> _durationParameter;
        private readonly IParameterDefinition<IntRandomizable> _countParameter;
        private readonly Action<string> _onWordRevealed;

        /// <summary>
        /// Initializes a new instance of the <see cref="WordByWordTagHandler"/> class.
        /// </summary>
        /// <param name="durationParameter">Duration parameter definition.</param>
        /// <param name="countParameter">Reveal-count parameter definition.</param>
        /// <param name="onWordRevealed">Optional callback invoked when words are revealed.</param>
        public WordByWordTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<IntRandomizable> countParameter,
            Action<string> onWordRevealed)
        {
            _durationParameter = durationParameter ?? throw new ArgumentNullException(nameof(durationParameter));
            _countParameter = countParameter ?? throw new ArgumentNullException(nameof(countParameter));
            _onWordRevealed = onWordRevealed ?? (_ => { });
        }

        /// <inheritdoc />
        public void Apply(TagNode node, PlaybackTimelineBuilder timelineBuilder)
        {
            var perWordDuration = Mathf.Max(0f, _durationParameter.Parse(node.Attributes));
            var countParameter = _countParameter.Parse(node.Attributes);
            var revealCount = Mathf.Max(1, countParameter.Value);

            if (perWordDuration <= 0f)
            {
                timelineBuilder.ProcessChildrenByWords(node, 0f, revealCount, _onWordRevealed);
                return;
            }

            timelineBuilder.ProcessChildrenByWords(node, perWordDuration, revealCount, _onWordRevealed);
        }
    }
}
