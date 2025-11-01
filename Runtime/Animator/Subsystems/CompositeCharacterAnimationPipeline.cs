using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback;
using FLFloppa.TextAnimator.Playback.Systems;
using FLFloppa.TextAnimator.Tags;

namespace FLFloppa.TextAnimator.Animator.Subsystems
{
    /// <summary>
    /// Composite pipeline that delegates modifier application to a set of subsystem-defined segments.
    /// </summary>
    internal sealed class CompositeCharacterAnimationPipeline : ICharacterAnimationPipeline
    {
        private readonly ICharacterAnimationPipelineSegment[] _segments;
        private readonly int[] _segmentCapacities;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeCharacterAnimationPipeline"/> class.
        /// </summary>
        /// <param name="textOutput">The text output that will receive property updates.</param>
        /// <param name="segments">The ordered collection of pipeline segments.</param>
        public CompositeCharacterAnimationPipeline(ITextOutput textOutput, IEnumerable<ICharacterAnimationPipelineSegment> segments)
        {
            if (textOutput == null)
            {
                throw new ArgumentNullException(nameof(textOutput));
            }

            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            var segmentList = new List<ICharacterAnimationPipelineSegment>();
            foreach (var segment in segments)
            {
                if (segment != null)
                {
                    segmentList.Add(segment);
                }
            }

            if (segmentList.Count == 0)
            {
                throw new ArgumentException("Composite pipeline requires at least one segment.", nameof(segments));
            }

            _segments = segmentList.ToArray();
            _segmentCapacities = new int[_segments.Length];
        }

        /// <inheritdoc />
        public void Apply(
            int characterIndex,
            CharacterDescriptor descriptor,
            float sessionElapsedTime,
            CharacterRevealClock revealClock,
            IReadOnlyDictionary<TagNode, ISubsystemModifier> modifiers,
            ITextOutput output)
        {
            if (modifiers == null)
            {
                throw new ArgumentNullException(nameof(modifiers));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (revealClock == null)
            {
                throw new ArgumentNullException(nameof(revealClock));
            }

            var characterElapsed = revealClock.GetRevealElapsed(characterIndex, sessionElapsedTime);
            var cachedElapsed = revealClock.GetCachedElapsed(characterIndex);
            if (characterElapsed < cachedElapsed)
            {
                characterElapsed = cachedElapsed;
            }
            else
            {
                revealClock.UpdateCachedElapsed(characterIndex, characterElapsed);
            }

            var requiredCapacity = revealClock.CharacterCount;

            for (var i = 0; i < _segments.Length; i++)
            {
                var segment = _segments[i];
                if (_segmentCapacities[i] < requiredCapacity)
                {
                    segment.EnsureCapacity(requiredCapacity);
                    _segmentCapacities[i] = requiredCapacity;
                }
                segment.ResetState(characterIndex);
                segment.Apply(
                    characterIndex,
                    descriptor,
                    sessionElapsedTime,
                    characterElapsed,
                    modifiers,
                    output);
            }
        }
    }
}
