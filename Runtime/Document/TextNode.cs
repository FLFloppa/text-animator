namespace FLFloppa.TextAnimator.Document
{
    /// <summary>
    /// Leaf node containing plain text content.
    /// </summary>
    public sealed class TextNode : IDocumentNode
    {
        public TagNode Parent { get; set; }
        public string Text { get; }

        public TextNode(string text)
        {
            Text = text;
        }
    }
}