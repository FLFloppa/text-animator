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
    /// Provides <see cref="RainbowModifier"/> instances for rainbow animation tags.
    /// </summary>
    public sealed class RainbowTagHandler : ICharacterModifierProvider
    {
        private readonly IParameterDefinition<float> _durationParameter;
        private readonly IParameterDefinition<bool> _loopParameter;
        private readonly IParameterDefinition<float> _colorShiftParameter;
        private readonly Gradient _gradient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainbowTagHandler"/> class.
        /// </summary>
        /// <param name="durationParameter">Parameter definition describing animation duration.</param>
        /// <param name="loopParameter">Parameter definition controlling loop behavior.</param>
        /// <param name="colorShiftParameter">Parameter definition for color offset per character index.</param>
        /// <param name="gradient">Gradient defining the rainbow colors.</param>
        public RainbowTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<bool> loopParameter,
            IParameterDefinition<float> colorShiftParameter,
            Gradient gradient)
        {
            _durationParameter = durationParameter ?? throw new ArgumentNullException(nameof(durationParameter));
            _loopParameter = loopParameter ?? throw new ArgumentNullException(nameof(loopParameter));
            _colorShiftParameter = colorShiftParameter ?? throw new ArgumentNullException(nameof(colorShiftParameter));
            _gradient = gradient ?? CreateDefaultRainbowGradient();
        }

        /// <inheritdoc />
        public ISubsystemModifier CreateModifier(TagNode node)
        {
            var duration = Mathf.Max(0f, _durationParameter.Parse(node.Attributes));
            var loop = _loopParameter.Parse(node.Attributes);
            var colorShift = _colorShiftParameter.Parse(node.Attributes);
            return new RainbowModifier(duration, colorShift, _gradient, loop);
        }

        private static Gradient CreateDefaultRainbowGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.yellow, 0.166f),
                    new GradientColorKey(Color.green, 0.333f),
                    new GradientColorKey(Color.cyan, 0.5f),
                    new GradientColorKey(Color.blue, 0.666f),
                    new GradientColorKey(Color.magenta, 0.833f),
                    new GradientColorKey(Color.red, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
            return gradient;
        }
    }
}
