using FLFloppa.TextAnimator.Characters.States;

namespace FLFloppa.TextAnimator.Output
{
    /// <summary>
    /// Provides a strongly typed applicator contract for color subsystem state.
    /// </summary>
    /// <typeparam name="TOutput">The text output implementation type.</typeparam>
    public interface IColorTextOutputApplicator<in TOutput> : ITextOutputApplicator<TOutput, ColorCharacterState>
        where TOutput : ITextOutput
    {
    }
}