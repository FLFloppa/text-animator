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
    /// Provides <see cref="RotateModifier"/> instances for rotation animation tags.
    /// </summary>
    public sealed class RotateTagHandler : ICharacterModifierProvider
    {
        private readonly IParameterDefinition<float> _durationParameter;
        private readonly IParameterDefinition<bool> _loopParameter;
        private readonly AnimationCurve _rotationCurve;

        /// <summary>
        /// Initializes a new instance of the <see cref="RotateTagHandler"/> class with a default linear curve.
        /// </summary>
        /// <param name="durationParameter">Parameter definition describing animation duration.</param>
        /// <param name="loopParameter">Parameter definition controlling loop behavior.</param>
        public RotateTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<bool> loopParameter)
            : this(durationParameter, loopParameter, AnimationCurve.Linear(0f, 0f, 1f, 360f))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RotateTagHandler"/> class.
        /// </summary>
        /// <param name="durationParameter">Parameter definition describing animation duration.</param>
        /// <param name="loopParameter">Parameter definition controlling loop behavior.</param>
        /// <param name="rotationCurve">Curve where values directly represent rotation angle in degrees.</param>
        public RotateTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<bool> loopParameter,
            AnimationCurve rotationCurve)
        {
            _durationParameter = durationParameter ?? throw new ArgumentNullException(nameof(durationParameter));
            _loopParameter = loopParameter ?? throw new ArgumentNullException(nameof(loopParameter));
            _rotationCurve = rotationCurve != null ? new AnimationCurve(rotationCurve.keys) : AnimationCurve.Linear(0f, 0f, 1f, 360f);
        }

        /// <inheritdoc />
        public ISubsystemModifier CreateModifier(TagNode node)
        {
            var duration = Mathf.Max(0f, _durationParameter.Parse(node.Attributes));
            var loop = _loopParameter.Parse(node.Attributes);
            return new RotateModifier(duration, _rotationCurve, loop);
        }
    }
}
