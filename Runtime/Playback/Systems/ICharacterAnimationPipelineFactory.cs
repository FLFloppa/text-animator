using FLFloppa.TextAnimator.Output;

namespace FLFloppa.TextAnimator.Playback.Systems
{
    /// <summary>
    /// Creates character animation pipelines bound to a specific text output target.
    /// </summary>
    public interface ICharacterAnimationPipelineFactory
    {
        /// <summary>
        /// Creates a pipeline capable of processing character animation for the provided text output.
        /// </summary>
        /// <param name="textOutput">The text output that will receive property updates.</param>
        /// <returns>The created character animation pipeline.</returns>
        ICharacterAnimationPipeline Create(ITextOutput textOutput);
    }
}
