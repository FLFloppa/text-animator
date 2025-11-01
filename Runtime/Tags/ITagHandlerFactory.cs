namespace FLFloppa.TextAnimator.Tags
{
    /// <summary>
    /// Creates tag handlers by tag identifier.
    /// </summary>
    public interface ITagHandlerFactory
    {
        ITagHandler CreateHandler(string identifier);
    }
}