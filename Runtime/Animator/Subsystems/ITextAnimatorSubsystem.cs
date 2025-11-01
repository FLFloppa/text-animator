using System.Collections.Generic;
using FLFloppa.TextAnimator.Output;

namespace FLFloppa.TextAnimator.Animator.Subsystems
{
    /// <summary>
    /// Defines the contract for modular text animator subsystems that contribute effect applicators and animation logic.
    /// </summary>
    public interface ITextAnimatorSubsystem
    {
        /// <summary>
        /// Gets the identifier describing this subsystem.
        /// </summary>
        TextAnimatorSubsystemKey Key { get; }

        /// <summary>
        /// Creates a character animation pipeline segment bound to the provided text output.
        /// </summary>
        /// <returns>The created pipeline segment.</returns>
        ICharacterAnimationPipelineSegment CreatePipelineSegment();

        /// <summary>
        /// Gets the collection of text output applicators configured for this subsystem.
        /// </summary>
        /// <returns>The configured applicators.</returns>
        IEnumerable<ITextOutputApplicator> GetApplicators();
    }
}
