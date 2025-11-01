using System;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Effects.Modifiers;
using FLFloppa.TextAnimator.Parameters;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Provides <see cref="SwayModifier"/> instances for sway animation tags.
    /// </summary>
    public sealed class SwayTagHandler : ICharacterModifierProvider
    {
        private readonly IParameterDefinition<float> _frequencyParameter;
        private readonly IParameterDefinition<float> _amplitudeParameter;
        private readonly IParameterDefinition<bool> _groupedParameter;
        private readonly IParameterDefinition<float> _phaseOffsetParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwayTagHandler"/> class.
        /// </summary>
        /// <param name="frequencyParameter">Parameter definition describing oscillation frequency.</param>
        /// <param name="amplitudeParameter">Parameter definition describing maximum rotation angle in degrees.</param>
        /// <param name="groupedParameter">Parameter definition controlling grouped rotation behavior.</param>
        /// <param name="phaseOffsetParameter">Parameter definition describing per-character phase offset.</param>
        public SwayTagHandler(
            IParameterDefinition<float> frequencyParameter,
            IParameterDefinition<float> amplitudeParameter,
            IParameterDefinition<bool> groupedParameter,
            IParameterDefinition<float> phaseOffsetParameter)
        {
            _frequencyParameter = frequencyParameter ?? throw new ArgumentNullException(nameof(frequencyParameter));
            _amplitudeParameter = amplitudeParameter ?? throw new ArgumentNullException(nameof(amplitudeParameter));
            _groupedParameter = groupedParameter ?? throw new ArgumentNullException(nameof(groupedParameter));
            _phaseOffsetParameter = phaseOffsetParameter ?? throw new ArgumentNullException(nameof(phaseOffsetParameter));
        }

        /// <inheritdoc />
        public ISubsystemModifier CreateModifier(TagNode node)
        {
            var frequency = Mathf.Max(0f, _frequencyParameter.Parse(node.Attributes));
            var amplitude = _amplitudeParameter.Parse(node.Attributes);
            var grouped = _groupedParameter.Parse(node.Attributes);
            var phaseOffset = _phaseOffsetParameter.Parse(node.Attributes);
            return new SwayModifier(frequency, amplitude, grouped, phaseOffset, node);
        }
    }
}
