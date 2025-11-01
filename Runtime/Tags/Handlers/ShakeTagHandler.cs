using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Effects.Modifiers;
using FLFloppa.TextAnimator.Parameters;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Provides <see cref="ShakeModifier"/> instances for shake tags.
    /// </summary>
    public sealed class ShakeTagHandler : ICharacterModifierProvider
    {
        private readonly IParameterDefinition<float> _amplitudeParameter;
        private readonly IParameterDefinition<float> _frequencyParameter;
        private readonly IParameterDefinition<bool> _synchronizeParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShakeTagHandler"/> class.
        /// </summary>
        /// <param name="amplitudeParameter">Parameter definition describing shake amplitude.</param>
        /// <param name="frequencyParameter">Parameter definition describing shake frequency.</param>
        /// <param name="synchronizeParameter">Parameter definition describing synchronization behaviour.</param>
        public ShakeTagHandler(
            IParameterDefinition<float> amplitudeParameter,
            IParameterDefinition<float> frequencyParameter,
            IParameterDefinition<bool> synchronizeParameter)
        {
            _amplitudeParameter = amplitudeParameter ?? throw new System.ArgumentNullException(nameof(amplitudeParameter));
            _frequencyParameter = frequencyParameter ?? throw new System.ArgumentNullException(nameof(frequencyParameter));
            _synchronizeParameter = synchronizeParameter ?? throw new System.ArgumentNullException(nameof(synchronizeParameter));
        }

        /// <inheritdoc />
        public ISubsystemModifier CreateModifier(TagNode node)
        {
            var amplitude = Mathf.Max(0f, _amplitudeParameter.Parse(node.Attributes));
            var frequency = Mathf.Max(0f, _frequencyParameter.Parse(node.Attributes));
            var synchronize = _synchronizeParameter.Parse(node.Attributes);
            return new ShakeModifier(amplitude, frequency, synchronize);
        }
    }
}
