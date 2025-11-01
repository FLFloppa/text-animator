using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Document;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Contains the playback timeline, plain text buffer, and character descriptors produced during compilation.
    /// </summary>
    public sealed class TimelineBuildResult
    {
        public TimelineBuildResult(PlaybackTimeline timeline, string plainText, IReadOnlyList<CharacterDescriptor> characters, IReadOnlyDictionary<TagNode, ISubsystemModifier> modifiers)
        {
            Timeline = timeline;
            PlainText = plainText;
            Characters = characters;
            Modifiers = modifiers;
        }

        public PlaybackTimeline Timeline { get; }
        public string PlainText { get; }
        public IReadOnlyList<CharacterDescriptor> Characters { get; }
        public IReadOnlyDictionary<TagNode, ISubsystemModifier> Modifiers { get; }
    }
}