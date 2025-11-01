namespace FLFloppa.TextAnimator.Document
{
    /// <summary>
    /// Represents the base contract for all nodes in the parsed text document tree.
    /// </summary>
    public interface IDocumentNode
    {
        TagNode Parent { get; set; }
    }
}