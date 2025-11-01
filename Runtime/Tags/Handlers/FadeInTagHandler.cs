using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Characters.Modifiers;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Effects.Modifiers;
using FLFloppa.TextAnimator.Parameters;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Provides <see cref="FadeInModifier"/> instances for appearance tags.
    /// </summary>
    public sealed class FadeInTagHandler : ICharacterModifierProvider
    {
        private readonly IParameterDefinition<float> _durationParameter;
        private readonly AnimationCurve _alphaCurve;

        public FadeInTagHandler(IParameterDefinition<float> durationParameter)
            : this(durationParameter, AnimationCurve.Linear(0f, 0f, 1f, 1f))
        {
        }

        public FadeInTagHandler(IParameterDefinition<float> durationParameter, AnimationCurve alphaCurve)
        {
            _durationParameter = durationParameter;
            _alphaCurve = alphaCurve != null ? new AnimationCurve(alphaCurve.keys) : AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }

        public ISubsystemModifier CreateModifier(TagNode node)
        {
            var duration = _durationParameter.Parse(node.Attributes);
            return new FadeInModifier(duration, _alphaCurve);
        }
    }
}
