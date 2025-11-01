using FLFloppa.TextAnimator.Characters.States;

namespace FLFloppa.TextAnimator.Output
{
    /// <summary>
    /// Provides a strongly typed applicator contract for transform subsystem state.
    /// </summary>
    /// <typeparam name="TOutput">The text output implementation type.</typeparam>
    public interface ITransformTextOutputApplicator<in TOutput> : ITextOutputApplicator<TOutput, TransformCharacterState>
        where TOutput : ITextOutput
    {
    }
}