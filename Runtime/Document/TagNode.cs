using System.Collections.Generic;

namespace FLFloppa.TextAnimator.Document
{
    /// <summary>
    /// Composite node representing a tag with attributes and child nodes.
    /// </summary>
    public sealed class TagNode : IDocumentNode
    {
        private readonly List<IDocumentNode> _children = new List<IDocumentNode>();

        public TagNode Parent { get; set; }
        public string TagName { get; }
        public IReadOnlyDictionary<string, string> Attributes { get; }
        public IReadOnlyList<IDocumentNode> Children => _children;

        public TagNode(string tagName, IReadOnlyDictionary<string, string> attributes)
        {
            TagName = tagName;
            Attributes = attributes;
        }

        public void AddChild(IDocumentNode child)
        {
            child.Parent = this;
            _children.Add(child);
        }
    }
}