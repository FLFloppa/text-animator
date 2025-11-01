namespace FLFloppa.TextAnimator.Parsing
{
    using FLFloppa.TextAnimator.Document;

    /// <summary>
    /// Parses a markup string into a document tree of nodes.
    /// </summary>
    public interface ITagParser
    {
        TagNode Parse(string source);
    }
}
