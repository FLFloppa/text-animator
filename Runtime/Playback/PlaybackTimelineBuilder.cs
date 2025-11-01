using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Playback.Strategies;
using FLFloppa.TextAnimator.Tags;
using UnityEngine;

namespace FLFloppa.TextAnimator.Playback
{
    /// <summary>
    /// Builds a playback timeline, plain text buffer, and character descriptors from a parsed tag tree.
    /// </summary>
    public sealed class PlaybackTimelineBuilder
    {
        private readonly ITagHandlerFactory _handlerFactory;
        private readonly List<char> _plainText = new List<char>();
        private readonly List<CharacterDescriptor> _characters = new List<CharacterDescriptor>();
        private readonly List<TagSpan> _tagSpans = new List<TagSpan>();
        private readonly List<IPlaybackInstruction> _instructions = new List<IPlaybackInstruction>();

        private readonly List<ModifierSpanEntry> _modifierSpans = new List<ModifierSpanEntry>();

        private readonly Stack<Action<string>> _revealActionStack = new Stack<Action<string>>();
        private readonly Stack<IRevealBatchingStrategy> _revealStrategyStack = new Stack<IRevealBatchingStrategy>();
        private readonly Stack<float> _characterDurationStack = new Stack<float>();

        private int _currentCharacterIndex;

        public PlaybackTimelineBuilder(ITagHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        /// <summary>
        /// Resets the builder state and compiles a new timeline from the specified root node.
        /// </summary>
        public TimelineBuildResult Build(TagNode root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            Reset();
            foreach (var child in root.Children)
            {
                ProcessNode(child, 0f, null);
            }

            AssignTagSequences();
            var characters = _characters.ToArray();
            var plainText = new string(_plainText.ToArray());
            var timeline = new PlaybackTimeline();
            foreach (var instruction in _instructions)
            {
                timeline.AddInstruction(instruction);
            }

            var modifiers = new Dictionary<TagNode, ISubsystemModifier>();
            foreach (var modifierSpan in _modifierSpans)
            {
                if (!modifiers.ContainsKey(modifierSpan.Tag))
                {
                    modifiers[modifierSpan.Tag] = modifierSpan.Modifier;
                }
            }
            
            return new TimelineBuildResult(timeline, plainText, characters, modifiers);
        }

        internal int InstructionCount => _instructions.Count;

        internal int CharacterCount => _characters.Count;

        internal void AddInstruction(IPlaybackInstruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            _instructions.Add(instruction);
        }

        internal void InsertInstruction(int index, IPlaybackInstruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            index = Mathf.Clamp(index, 0, _instructions.Count);
            _instructions.Insert(index, instruction);
        }

        internal void ProcessChildren(TagNode parent, float characterDuration,
            Action<string> onCharacterRevealed = null)
        {
            ProcessChildrenInternal(parent, characterDuration, onCharacterRevealed, null);
        }

        internal void ProcessChildren(TagNode parent, float characterDuration, int revealCount,
            Action<string> onCharacterRevealed = null)
        {
            var strategy = new CharacterCountRevealStrategy(revealCount);
            ProcessChildrenInternal(parent, characterDuration, onCharacterRevealed, strategy);
        }

        internal void ProcessChildrenByWords(TagNode parent, float characterDuration, int revealCount,
            Action<string> onCharacterRevealed = null)
        {
            var strategy = new WordRevealStrategy(revealCount);
            ProcessChildrenInternal(parent, characterDuration, onCharacterRevealed, strategy);
        }

        /// <summary>
        /// Processes children while inheriting all context from parent (strategy, duration, action).
        /// Used by tags that should not modify playback behavior (e.g., wait, markers).
        /// </summary>
        internal void ProcessChildrenWithInheritance(TagNode parent)
        {
            var inheritedDuration = _characterDurationStack.Count > 0 ? _characterDurationStack.Peek() : 0f;
            var inheritedAction = _revealActionStack.Count > 0 ? _revealActionStack.Peek() : null;
            
            foreach (var child in parent.Children)
            {
                ProcessNode(child, inheritedDuration, inheritedAction);
            }
        }

        private void ProcessChildrenInternal(TagNode parent, float characterDuration,
            Action<string> onCharacterRevealed, IRevealBatchingStrategy strategyOverride)
        {
            var effectiveAction =
                onCharacterRevealed ?? (_revealActionStack.Count > 0 ? _revealActionStack.Peek() : null);
            var parentStrategy = _revealStrategyStack.Count > 0 ? _revealStrategyStack.Peek() : null;
            var strategy = strategyOverride ??
                           parentStrategy?.CreateChild() ?? CharacterCountRevealStrategy.CreateDefault();

            _revealActionStack.Push(effectiveAction);
            _revealStrategyStack.Push(strategy);
            _characterDurationStack.Push(characterDuration);
            try
            {
                foreach (var child in parent.Children)
                {
                    ProcessNode(child, characterDuration, effectiveAction);
                }
            }
            finally
            {
                _characterDurationStack.Pop();
                _revealStrategyStack.Pop();
                _revealActionStack.Pop();
            }
        }

        private void Reset()
        {
            _plainText.Clear();
            _characters.Clear();
            _tagSpans.Clear();
            _instructions.Clear();
            _modifierSpans.Clear();
            _currentCharacterIndex = 0;
            _revealActionStack.Clear();
            _revealStrategyStack.Clear();
            _characterDurationStack.Clear();
        }

        private void ProcessNode(IDocumentNode node, float characterDuration, Action<string> onCharacterRevealed)
        {
            switch (node)
            {
                case TextNode text:
                    AppendText(text, characterDuration, onCharacterRevealed);
                    break;
                case RichTextNode richText:
                    AppendRichText(richText);
                    break;
                case TagNode tagNode:
                    ProcessTag(tagNode, characterDuration, onCharacterRevealed);
                    break;
            }
        }

        private void AppendText(TextNode textNode, float characterDuration, Action<string> onCharacterRevealed)
        {
            if (string.IsNullOrEmpty(textNode.Text))
            {
                return;
            }

            var strategy = _revealStrategyStack.Count > 0
                ? _revealStrategyStack.Peek()
                : CharacterCountRevealStrategy.CreateDefault();
            var previousWasWordCharacter = false;
            foreach (var character in textNode.Text)
            {
                var isWordCharacter = WordCharacterUtility.IsWordCharacter(character);
                var isWordStart = isWordCharacter && !previousWasWordCharacter;
                var descriptor = new CharacterDescriptor(_currentCharacterIndex, character);

                _characters.Add(descriptor);
                _plainText.Add(character);

                var instructionDuration =
                    strategy.ShouldApplyDuration(character, descriptor, isWordCharacter, isWordStart)
                        ? characterDuration
                        : 0f;
                _instructions.Add(new RevealCharacterInstruction(_currentCharacterIndex, instructionDuration,
                    onCharacterRevealed));
                _currentCharacterIndex++;

                strategy.Advance(character, descriptor, isWordCharacter, isWordStart);
                previousWasWordCharacter = isWordCharacter;
            }
        }

        private void AppendRichText(RichTextNode richTextNode)
        {
            if (string.IsNullOrEmpty(richTextNode.Markup))
            {
                return;
            }

            foreach (var character in richTextNode.Markup)
            {
                _plainText.Add(character);
            }
        }

        private void ProcessTag(TagNode tagNode, float characterDuration, Action<string> onCharacterRevealed)
        {
            var spanStart = _currentCharacterIndex;

            var handler = _handlerFactory.CreateHandler(tagNode.TagName);

            if (handler is IPlaybackControlHandler controlHandler)
            {
                controlHandler.Apply(tagNode, this);
            }
            else
            {
                // Modifier tags (fadeIn, wave, shake) inherit parent strategy
                ProcessChildrenInternal(tagNode, characterDuration, onCharacterRevealed, null);
            }

            var spanEnd = _currentCharacterIndex;

            if (handler is ICompositeModifierProvider compositeProvider)
            {
                var modifiers = compositeProvider.CreateModifiers(tagNode);
                if (modifiers != null)
                {
                    foreach (var modifier in modifiers)
                    {
                        if (modifier != null)
                        {
                            _modifierSpans.Add(new ModifierSpanEntry(tagNode, modifier, spanStart, spanEnd));
                        }
                    }
                }
            }
            else if (handler is ICharacterModifierProvider modifierProvider)
            {
                var modifier = modifierProvider.CreateModifier(tagNode);
                if (modifier != null)
                {
                    _modifierSpans.Add(new ModifierSpanEntry(tagNode, modifier, spanStart, spanEnd));
                }
            }

            _tagSpans.Add(new TagSpan(tagNode, spanStart, spanEnd));
        }

        private void AssignTagSequences()
        {
            if (_characters.Count == 0)
            {
                return;
            }

            if (_tagSpans.Count == 0)
            {
                return;
            }

            var spans = _tagSpans.ToArray();
            var modifierSpans = _modifierSpans.ToArray();

            for (var i = 0; i < _characters.Count; i++)
            {
                var descriptor = _characters[i];
                var tags = new List<TagNode>();
                var modifierBindings = new List<CharacterModifierBinding>();
                
                foreach (var span in spans)
                {
                    if (span.Contains(descriptor.Index))
                    {
                        tags.Add(span.Node);
                    }
                }

                foreach (var modifierSpan in modifierSpans)
                {
                    if (modifierSpan.Contains(descriptor.Index))
                    {
                        modifierBindings.Add(new CharacterModifierBinding(modifierSpan.Tag, modifierSpan.Modifier));
                    }
                }

                descriptor.AssignMetadata(tags.ToArray(), modifierBindings.ToArray());
                _characters[i] = descriptor;
            }
        }

        private readonly struct TagSpan
        {
            public TagSpan(TagNode node, int startIndex, int endIndex)
            {
                Node = node;
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public TagNode Node { get; }
            public int StartIndex { get; }
            public int EndIndex { get; }

            public bool Contains(int index) => index >= StartIndex && index < EndIndex;
        }

        private readonly struct ModifierSpanEntry
        {
            public ModifierSpanEntry(TagNode tag, ISubsystemModifier modifier, int startIndex, int endIndex)
            {
                Tag = tag;
                Modifier = modifier;
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public TagNode Tag { get; }
            public ISubsystemModifier Modifier { get; }
            public int StartIndex { get; }
            public int EndIndex { get; }

            public bool Contains(int index) => index >= StartIndex && index < EndIndex;
        }
    }
}
