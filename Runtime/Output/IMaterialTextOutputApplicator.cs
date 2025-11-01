using FLFloppa.TextAnimator.Characters.States;

namespace FLFloppa.TextAnimator.Output
{
    /// <summary>
    /// Provides a strongly typed applicator contract for material subsystem state.
    /// </summary>
    /// <typeparam name="TOutput">The text output implementation type.</typeparam>
    public interface IMaterialTextOutputApplicator<in TOutput> : ITextOutputApplicator<TOutput, MaterialCharacterState>
        where TOutput : ITextOutput
    {
    }
}