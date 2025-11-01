using FLFloppa.TextAnimator.Document;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Represents a single character extracted from the parsed document.
    /// </summary>
    public readonly struct DocumentCharacter
    {
        public DocumentCharacter(int index, char value, TextNode owner)
        {
            Index = index;
            Value = value;
            Owner = owner;
        }

        public int Index { get; }
        public char Value { get; }
        public TextNode Owner { get; }
    }
}