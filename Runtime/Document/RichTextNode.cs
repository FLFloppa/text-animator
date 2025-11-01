using System;

namespace FLFloppa.TextAnimator.Document
{
    /// <summary>
    /// Represents inline rich-text markup that should be preserved in the rendered output
    /// without contributing characters for animation.
    /// </summary>
    public sealed class RichTextNode : IDocumentNode
    {
        public RichTextNode(string markup)
        {
            Markup = markup ?? throw new ArgumentNullException(nameof(markup));
        }

        public TagNode Parent { get; set; }

        public string Markup { get; }
    }
}
