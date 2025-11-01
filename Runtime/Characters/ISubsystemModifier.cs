using FLFloppa.TextAnimator.Animator.Subsystems;

namespace FLFloppa.TextAnimator.Characters
{
    /// <summary>
    /// Represents a modifier associated with a specific text animator subsystem.
    /// </summary>
    public interface ISubsystemModifier
    {
        /// <summary>
        /// Gets the subsystem key associated with this modifier.
        /// </summary>
        TextAnimatorSubsystemKey TargetSubsystem { get; }
    }

    /// <summary>
    /// Defines a modifier for a specific text animator subsystem state type.
    /// </summary>
    /// <typeparam name="TState">The subsystem-specific state type.</typeparam>
    public interface ISubsystemModifier<in TState> : ISubsystemModifier
    {
        /// <summary>
        /// Applies the modifier to the provided subsystem state.
        /// </summary>
        /// <param name="state">The subsystem state to mutate.</param>
        /// <param name="characterIndex">The zero-based index of the character being modified.</param>
        /// <param name="sessionElapsedTime">The elapsed playback time for the session.</param>
        /// <param name="characterElapsedTime">The elapsed time since the character was revealed.</param>
        void Modify(TState state, int characterIndex, float sessionElapsedTime, float characterElapsedTime);
    }
}
