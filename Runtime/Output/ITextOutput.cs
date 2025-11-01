namespace FLFloppa.TextAnimator.Output
{
    /// <summary>
    /// Abstraction for a text rendering target capable of applying subsystem state per character.
    /// </summary>
    public interface ITextOutput
    {
        void SetText(string text);
        void SetVisibleCharacterCount(int count);
        void BeginFrame();
        void FinalizeUpdate();
    }
}
