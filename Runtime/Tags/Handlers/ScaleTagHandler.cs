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
    /// Provides <see cref="ScaleModifier"/> instances for scale animation tags.
    /// </summary>
    public sealed class ScaleTagHandler : ICharacterModifierProvider
    {
        private readonly IParameterDefinition<float> _durationParameter;
        private readonly IParameterDefinition<bool> _loopParameter;
        private readonly AnimationCurve _scaleXCurve;
        private readonly AnimationCurve _scaleYCurve;
        private readonly AnimationCurve _scaleZCurve;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleTagHandler"/> class with default linear curves.
        /// </summary>
        /// <param name="durationParameter">Parameter definition describing animation duration.</param>
        /// <param name="loopParameter">Parameter definition controlling loop behavior.</param>
        public ScaleTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<bool> loopParameter)
            : this(
                durationParameter, 
                loopParameter, 
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                AnimationCurve.Linear(0f, 0f, 1f, 1f),
                AnimationCurve.Linear(0f, 0f, 1f, 1f))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleTagHandler"/> class.
        /// </summary>
        /// <param name="durationParameter">Parameter definition describing animation duration.</param>
        /// <param name="loopParameter">Parameter definition controlling loop behavior.</param>
        /// <param name="scaleXCurve">Curve describing X-axis scale values over the animation duration.</param>
        /// <param name="scaleYCurve">Curve describing Y-axis scale values over the animation duration.</param>
        /// <param name="scaleZCurve">Curve describing Z-axis scale values over the animation duration.</param>
        public ScaleTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<bool> loopParameter,
            AnimationCurve scaleXCurve,
            AnimationCurve scaleYCurve,
            AnimationCurve scaleZCurve)
        {
            _durationParameter = durationParameter ?? throw new ArgumentNullException(nameof(durationParameter));
            _loopParameter = loopParameter ?? throw new ArgumentNullException(nameof(loopParameter));
            _scaleXCurve = scaleXCurve != null ? new AnimationCurve(scaleXCurve.keys) : AnimationCurve.Linear(0f, 0f, 1f, 1f);
            _scaleYCurve = scaleYCurve != null ? new AnimationCurve(scaleYCurve.keys) : AnimationCurve.Linear(0f, 0f, 1f, 1f);
            _scaleZCurve = scaleZCurve != null ? new AnimationCurve(scaleZCurve.keys) : AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }

        /// <inheritdoc />
        public ISubsystemModifier CreateModifier(TagNode node)
        {
            var duration = Mathf.Max(0f, _durationParameter.Parse(node.Attributes));
            var loop = _loopParameter.Parse(node.Attributes);
            return new ScaleModifier(duration, _scaleXCurve, _scaleYCurve, _scaleZCurve, loop);
        }
    }
}
