using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Playback.Systems;

namespace FLFloppa.TextAnimator.Output
{
    /// <summary>
    /// Base class for ScriptableObject-backed text output applicators.
    /// </summary>
    public abstract class TextOutputApplicatorAsset : UnityEngine.ScriptableObject, ITextOutputApplicator
    {
        /// <inheritdoc />
        public abstract TextAnimatorSubsystemKey TargetSubsystem { get; }

        /// <inheritdoc />
        public abstract Type OutputType { get; }
    }

    /// <summary>
    /// Generic base class for strongly typed text output applicators.
    /// </summary>
    /// <typeparam name="TOutput">The supported text output type.</typeparam>
    /// <typeparam name="TState">The supported character state type.</typeparam>
    public abstract class TextOutputApplicatorAsset<TOutput, TState> : TextOutputApplicatorAsset, ITextOutputApplicator<TOutput, TState>, ITextOutputApplicatorBindingProvider<TState>
        where TOutput : class, ITextOutput
    {
        /// <inheritdoc />
        public override Type OutputType => typeof(TOutput);

        /// <inheritdoc />
        public void Apply(TOutput output, int characterIndex, TState state)
        {
            ApplyTyped(output, characterIndex, state);
        }

        /// <summary>
        /// Applies the specified state to the provided output.
        /// </summary>
        protected abstract void ApplyTyped(TOutput output, int characterIndex, TState state);

        /// <inheritdoc />
        public bool TryCreateBinding(ITextOutput output, out ITextOutputApplicatorBinding<TState> binding)
        {
            if (output is TOutput typedOutput)
            {
                binding = new ApplicatorBinding(typedOutput, ApplyTyped);
                return true;
            }

            binding = null!;
            return false;
        }

        private sealed class ApplicatorBinding : ITextOutputApplicatorBinding<TState>
        {
            private readonly TOutput _output;
            private readonly Action<TOutput, int, TState> _apply;

            public ApplicatorBinding(TOutput output, Action<TOutput, int, TState> apply)
            {
                _output = output;
                _apply = apply;
            }

            public void Apply(int characterIndex, TState state)
            {
                _apply(_output, characterIndex, state);
            }
        }
    }
}
