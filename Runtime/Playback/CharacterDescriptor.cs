using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Document;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Describes an individual character in the compiled document.
    /// </summary>
    public struct CharacterDescriptor
    {
        private TagNode[] _tags;
        private CharacterModifierBinding[] _modifiers;

        internal CharacterDescriptor(int index, char value)
        {
            Index = index;
            Character = value;
            _tags = Array.Empty<TagNode>();
            _modifiers = Array.Empty<CharacterModifierBinding>();
        }

        public int Index { get; }
        public char Character { get; }
        public IReadOnlyList<TagNode> Tags => _tags;
        public IReadOnlyList<CharacterModifierBinding> Modifiers => _modifiers;

        internal void AssignMetadata(TagNode[] tags, CharacterModifierBinding[] modifiers)
        {
            _tags = tags;
            _modifiers = modifiers;
        }
    }
}