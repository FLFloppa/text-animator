using System;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback.Systems;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Coordinates timeline execution against a text output target.
    /// </summary>
    public sealed class PlaybackEngine
    {
        private readonly ITextOutput _textOutput;
        private readonly ICharacterAnimationPipelineFactory _pipelineFactory;

        public PlaybackEngine(ITextOutput textOutput, ICharacterAnimationPipelineFactory pipelineFactory)
        {
            _textOutput = textOutput ?? throw new ArgumentNullException(nameof(textOutput));
            _pipelineFactory = pipelineFactory ?? throw new ArgumentNullException(nameof(pipelineFactory));
        }

        /// <summary>
        /// Starts a new playback session using the specified build result.
        /// </summary>
        public PlaybackSession Start(TimelineBuildResult buildResult)
        {
            if (buildResult == null)
            {
                throw new ArgumentNullException(nameof(buildResult));
            }

            _textOutput.SetText(buildResult.PlainText);
            _textOutput.SetVisibleCharacterCount(0);

            var animationPipeline = _pipelineFactory.Create(_textOutput);
            return new PlaybackSession(
                _textOutput,
                animationPipeline,
                buildResult.Modifiers,
                buildResult.Timeline.Instructions,
                buildResult.Characters);
        }
    }
}
