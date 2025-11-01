using System;

namespace FLFloppa.TextAnimator.Parsing
{
    /// <summary>
    /// Represents parsing errors for markup documents.
    /// </summary>
    public sealed class MarkupParseException : Exception
    {
        public MarkupParseException(string message)
            : base(message)
        {
        }

        public MarkupParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
