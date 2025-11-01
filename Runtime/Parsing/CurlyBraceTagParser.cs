using System;
using System.Collections.Generic;
using System.Text;
using FLFloppa.TextAnimator.Document;

namespace FLFloppa.TextAnimator.Parsing
{
    /// <summary>
    /// Parses markup enclosed in curly braces and builds a document tree.
    /// </summary>
    public sealed class CurlyBraceTagParser : ITagParser
    {
        private const string RootTagName = "root";

        public TagNode Parse(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var root = new TagNode(RootTagName, new Dictionary<string, string>());
            var stack = new Stack<TagNode>();
            stack.Push(root);

            var textBuffer = new StringBuilder();
            var index = 0;
            while (index < source.Length)
            {
                var current = source[index];
                if (current == '{')
                {
                    if (TryConsumeEscape(source, ref index, textBuffer))
                    {
                        continue;
                    }

                    FlushTextBuffer(textBuffer, stack.Peek());
                    var tagContent = ReadTagContent(source, ref index);
                    if (string.IsNullOrWhiteSpace(tagContent))
                    {
                        throw new MarkupParseException("Encountered empty tag declaration.");
                    }

                    if (tagContent[0] == '/')
                    {
                        ProcessClosingTag(tagContent.Substring(1).Trim(), stack);
                        continue;
                    }

                    var tagName = ParseTagName(tagContent, out var attributesSegment);
                    var attributes = ParseAttributes(attributesSegment);
                    var tagNode = new TagNode(tagName, attributes);
                    stack.Peek().AddChild(tagNode);
                    stack.Push(tagNode);
                }
                else if (current == '<')
                {
                    if (TryConsumeRichTextTag(source, ref index, textBuffer, stack.Peek()))
                    {
                        continue;
                    }

                    textBuffer.Append(current);
                    index++;
                }
                else
                {
                    textBuffer.Append(current);
                    index++;
                }
            }

            FlushTextBuffer(textBuffer, stack.Peek());

            if (stack.Count != 1)
            {
                throw new MarkupParseException("Unmatched opening tags detected in markup.");
            }

            return root;
        }

        private static bool TryConsumeEscape(string source, ref int index, StringBuilder textBuffer)
        {
            if (index + 1 < source.Length && source[index + 1] == '{')
            {
                textBuffer.Append('{');
                index += 2;
                return true;
            }

            return false;
        }

        private static string ReadTagContent(string source, ref int index)
        {
            index++; // Skip '{'
            var closing = source.IndexOf('}', index);
            if (closing == -1)
            {
                throw new MarkupParseException("Missing closing brace for tag declaration.");
            }

            var length = closing - index;
            var result = source.Substring(index, length);
            index = closing + 1;
            return result.Trim();
        }

        private static void FlushTextBuffer(StringBuilder buffer, TagNode owner)
        {
            if (buffer.Length == 0)
            {
                return;
            }

            var textNode = new TextNode(buffer.ToString());
            owner.AddChild(textNode);
            buffer.Clear();
        }

        private static bool TryConsumeRichTextTag(string source, ref int index, StringBuilder textBuffer, TagNode owner)
        {
            if (index + 1 >= source.Length)
            {
                return false;
            }

            var lookahead = source[index + 1];

            if (lookahead == '<')
            {
                textBuffer.Append('<');
                index += 2;
                return true;
            }

            if (!IsRichTextCandidate(lookahead))
            {
                return false;
            }

            var closingIndex = FindRichTextClosingIndex(source, index + 1);
            if (closingIndex == -1)
            {
                return false;
            }

            var markup = source.Substring(index, closingIndex - index + 1);
            FlushTextBuffer(textBuffer, owner);
            owner.AddChild(new RichTextNode(markup));
            index = closingIndex + 1;
            return true;
        }

        private static bool IsRichTextCandidate(char character)
        {
            return char.IsLetter(character) || character == '/' || character == '#' || character == '!' || character == '%';
        }

        private static int FindRichTextClosingIndex(string source, int startIndex)
        {
            var index = startIndex;
            while (index < source.Length)
            {
                if (source[index] == '>')
                {
                    return index;
                }

                if (source[index] == '\n' || source[index] == '\r')
                {
                    break;
                }

                index++;
            }

            return -1;
        }

        private static void ProcessClosingTag(string tagName, Stack<TagNode> stack)
        {
            if (stack.Count <= 1)
            {
                throw new MarkupParseException($"Encountered closing tag '{tagName}' without matching opening tag.");
            }

            var current = stack.Pop();
            if (!string.Equals(current.TagName, tagName, StringComparison.Ordinal))
            {
                throw new MarkupParseException($"Closing tag '{tagName}' does not match currently open tag '{current.TagName}'.");
            }
        }

        private static string ParseTagName(string content, out string attributesSegment)
        {
            var whitespaceIndex = IndexOfWhitespace(content);
            if (whitespaceIndex == -1)
            {
                attributesSegment = string.Empty;
                return content;
            }

            attributesSegment = content.Substring(whitespaceIndex + 1);
            return content.Substring(0, whitespaceIndex);
        }

        private static int IndexOfWhitespace(string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                if (char.IsWhiteSpace(input[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private static IReadOnlyDictionary<string, string> ParseAttributes(string segment)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(segment))
            {
                return result;
            }

            var index = 0;
            while (index < segment.Length)
            {
                index = SkipWhitespace(segment, index);
                if (index >= segment.Length)
                {
                    break;
                }

                var keyStart = index;
                while (index < segment.Length && !char.IsWhiteSpace(segment[index]) && segment[index] != '=')
                {
                    index++;
                }

                var keyEnd = index;
                var key = segment.Substring(keyStart, keyEnd - keyStart);
                key = key.Trim();
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                index = SkipWhitespace(segment, index);
                string value = string.Empty;

                if (index < segment.Length && segment[index] == '=')
                {
                    index++;
                    index = SkipWhitespace(segment, index);

                    if (index >= segment.Length)
                    {
                        throw new MarkupParseException($"Expected value for attribute '{key}'.");
                    }

                    if (segment[index] == '"' || segment[index] == '\'')
                    {
                        var quote = segment[index++];
                        var closing = segment.IndexOf(quote, index);
                        if (closing == -1)
                        {
                            throw new MarkupParseException("Unterminated quoted attribute value.");
                        }

                        value = segment.Substring(index, closing - index);
                        index = closing + 1;
                    }
                    else
                    {
                        var valueStart = index;
                        while (index < segment.Length && !char.IsWhiteSpace(segment[index]))
                        {
                            index++;
                        }

                        value = segment.Substring(valueStart, index - valueStart);
                    }
                }

                result[key] = value;
            }

            return result;
        }

        private static int SkipWhitespace(string input, int index)
        {
            while (index < input.Length && char.IsWhiteSpace(input[index]))
            {
                index++;
            }

            return index;
        }
    }
}
