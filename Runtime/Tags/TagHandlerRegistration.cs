namespace FLFloppa.TextAnimator.Tags
{
    /// <summary>
    /// Describes a tag handler registration.
    /// </summary>
    public readonly struct TagHandlerRegistration
    {
        public TagHandlerRegistration(string identifier, ITagHandler handler)
        {
            Identifier = identifier;
            Handler = handler;
        }

        public string Identifier { get; }
        public ITagHandler Handler { get; }
    }
}