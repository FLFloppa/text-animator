using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback.Systems;

namespace FLFloppa.TextAnimator.Animator.Subsystems
{
    /// <summary>
    /// Creates composite character animation pipelines from a predefined set of subsystems.
    /// </summary>
    internal sealed class CompositeCharacterAnimationPipelineFactory : ICharacterAnimationPipelineFactory
    {
        private readonly ITextAnimatorSubsystem[] _subsystems;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeCharacterAnimationPipelineFactory"/> class.
        /// </summary>
        /// <param name="subsystems">The subsystem snapshot contributing pipeline segments.</param>
        public CompositeCharacterAnimationPipelineFactory(ITextAnimatorSubsystem[] subsystems)
        {
            _subsystems = subsystems ?? throw new ArgumentNullException(nameof(subsystems));
            if (_subsystems.Length == 0)
            {
                throw new ArgumentException("Composite pipeline factory requires at least one subsystem.", nameof(subsystems));
            }
        }

        /// <inheritdoc />
        public ICharacterAnimationPipeline Create(ITextOutput textOutput)
        {
            if (textOutput == null)
            {
                throw new ArgumentNullException(nameof(textOutput));
            }

            var segments = new List<ICharacterAnimationPipelineSegment>(_subsystems.Length);
            for (var i = 0; i < _subsystems.Length; i++)
            {
                var subsystem = _subsystems[i];
                if (subsystem == null)
                {
                    continue;
                }

                var segment = subsystem.CreatePipelineSegment();
                if (segment == null)
                {
                    continue;
                }

                var applicators = subsystem.GetApplicators();
                segment.Configure(textOutput, applicators ?? Array.Empty<ITextOutputApplicator>());
                segments.Add(segment);
            }

            if (segments.Count == 0)
            {
                throw new InvalidOperationException("No valid pipeline segments created by composite factory.");
            }

            if (segments.Count == 1 && segments[0] is ICharacterAnimationPipeline standalonePipeline)
            {
                return standalonePipeline;
            }

            return new CompositeCharacterAnimationPipeline(textOutput, segments);
        }
    }
}
