using System;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Playback;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Applies a per-character reveal duration to the contents of the tag.
    /// </summary>
    public sealed class CharByCharTagHandler : IPlaybackControlHandler
    {
        private readonly IParameterDefinition<float> _durationParameter;
        private readonly IParameterDefinition<IntRandomizable> _countParameter;
        private readonly Action<string> _onCharacterRevealed;

        public CharByCharTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<IntRandomizable> countParameter,
            Action<string> onCharacterRevealed)
        {
            _durationParameter = durationParameter ?? throw new ArgumentNullException(nameof(durationParameter));
            _countParameter = countParameter ?? throw new ArgumentNullException(nameof(countParameter));
            _onCharacterRevealed = onCharacterRevealed ?? (_ => { });
        }

        public void Apply(TagNode node, PlaybackTimelineBuilder timelineBuilder)
        {
            var perCharacterDuration = Mathf.Max(0f, _durationParameter.Parse(node.Attributes));
            var countParameter = _countParameter.Parse(node.Attributes);
            var revealCount = Mathf.Max(1, countParameter.Value);

            if (perCharacterDuration <= 0f)
            {
                timelineBuilder.ProcessChildren(node, 0f, revealCount, _onCharacterRevealed);
                return;
            }

            timelineBuilder.ProcessChildren(node, perCharacterDuration, revealCount, _onCharacterRevealed);
        }
    }
}
