using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Playback.Systems;

namespace FLFloppa.TextAnimator.Output
{
    /// <summary>
    /// Provides metadata for text output applicators.
    /// </summary>
    public interface ITextOutputApplicator
    {
        /// <summary>
        /// Gets the subsystem key associated with this applicator.
        /// </summary>
        TextAnimatorSubsystemKey TargetSubsystem { get; }

        /// <summary>
        /// Gets the concrete output type supported by this applicator.
        /// </summary>
        Type OutputType { get; }
    }

    /// <summary>
    /// Defines a typed applicator that can apply character state to a supported text output implementation.
    /// </summary>
    /// <typeparam name="TOutput">The text output implementation type.</typeparam>
    /// <typeparam name="TState">The character state type supported by the applicator.</typeparam>
    public interface ITextOutputApplicator<in TOutput, in TState> : ITextOutputApplicator
        where TOutput : ITextOutput
    {
        /// <summary>
        /// Applies the specified character state to the provided text output.
        /// </summary>
        /// <param name="output">The text output that receives the state.</param>
        /// <param name="characterIndex">The index of the character to update.</param>
        /// <param name="state">The state to apply.</param>
        void Apply(TOutput output, int characterIndex, TState state);
    }

    /// <summary>
    /// Represents a reusable binding between a text output and a subsystem state applicator.
    /// </summary>
    /// <typeparam name="TState">The character state type handled by the binding.</typeparam>
    public interface ITextOutputApplicatorBinding<in TState>
    {
        /// <summary>
        /// Applies the cached binding to the specified character.
        /// </summary>
        /// <param name="characterIndex">The index of the character to update.</param>
        /// <param name="state">The state to apply.</param>
        void Apply(int characterIndex, TState state);
    }

    /// <summary>
    /// Provides a typed binding for a text output applicator.
    /// </summary>
    /// <typeparam name="TState">The character state type supported by the binding.</typeparam>
    public interface ITextOutputApplicatorBindingProvider<TState> : ITextOutputApplicator
    {
        /// <summary>
        /// Attempts to create a binding for the specified output.
        /// </summary>
        /// <param name="output">The text output target.</param>
        /// <param name="binding">When this method returns, contains the created binding if successful.</param>
        /// <returns><c>true</c> when the binding was created; otherwise, <c>false</c>.</returns>
        bool TryCreateBinding(ITextOutput output, out ITextOutputApplicatorBinding<TState> binding);
    }
}
