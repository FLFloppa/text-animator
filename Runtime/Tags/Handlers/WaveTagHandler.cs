using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Effects.Modifiers;
using FLFloppa.TextAnimator.Parameters;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Provides <see cref="WaveModifier"/> instances for wave tags.
    /// </summary>
    public sealed class WaveTagHandler : ICharacterModifierProvider
    {
        private readonly IParameterDefinition<float> _amplitudeParameter;
        private readonly IParameterDefinition<float> _frequencyParameter;
        private readonly IParameterDefinition<float> _phaseParameter;

        public WaveTagHandler(
            IParameterDefinition<float> amplitudeParameter,
            IParameterDefinition<float> frequencyParameter,
            IParameterDefinition<float> phaseParameter)
        {
            _amplitudeParameter = amplitudeParameter;
            _frequencyParameter = frequencyParameter;
            _phaseParameter = phaseParameter;
        }

        public ISubsystemModifier CreateModifier(TagNode node)
        {
            var amplitude = _amplitudeParameter.Parse(node.Attributes);
            var frequency = _frequencyParameter.Parse(node.Attributes);
            var phase = _phaseParameter.Parse(node.Attributes);
            return new WaveModifier(amplitude, frequency, phase);
        }
    }
}
