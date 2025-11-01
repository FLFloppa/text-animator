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
    /// Provides <see cref="PositionOffsetModifier"/> instances for position offset animation tags.
    /// </summary>
    public sealed class PositionOffsetTagHandler : ICharacterModifierProvider
    {
        private readonly IParameterDefinition<float> _durationParameter;
        private readonly IParameterDefinition<bool> _loopParameter;
        private readonly IParameterDefinition<bool> _overrideParameter;
        private readonly AnimationCurve _positionXCurve;
        private readonly AnimationCurve _positionYCurve;
        private readonly AnimationCurve _positionZCurve;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionOffsetTagHandler"/> class with default flat curves.
        /// </summary>
        /// <param name="durationParameter">Parameter definition describing animation duration.</param>
        /// <param name="loopParameter">Parameter definition controlling loop behavior.</param>
        /// <param name="overrideParameter">Parameter definition controlling override behavior.</param>
        public PositionOffsetTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<bool> loopParameter,
            IParameterDefinition<bool> overrideParameter)
            : this(
                durationParameter, 
                loopParameter, 
                overrideParameter, 
                AnimationCurve.Constant(0f, 1f, 0f),
                AnimationCurve.Constant(0f, 1f, 0f),
                AnimationCurve.Constant(0f, 1f, 0f))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionOffsetTagHandler"/> class.
        /// </summary>
        /// <param name="durationParameter">Parameter definition describing animation duration.</param>
        /// <param name="loopParameter">Parameter definition controlling loop behavior.</param>
        /// <param name="overrideParameter">Parameter definition controlling override behavior.</param>
        /// <param name="positionXCurve">Curve where values directly represent X-axis position offset.</param>
        /// <param name="positionYCurve">Curve where values directly represent Y-axis position offset.</param>
        /// <param name="positionZCurve">Curve where values directly represent Z-axis position offset.</param>
        public PositionOffsetTagHandler(
            IParameterDefinition<float> durationParameter,
            IParameterDefinition<bool> loopParameter,
            IParameterDefinition<bool> overrideParameter,
            AnimationCurve positionXCurve,
            AnimationCurve positionYCurve,
            AnimationCurve positionZCurve)
        {
            _durationParameter = durationParameter ?? throw new ArgumentNullException(nameof(durationParameter));
            _loopParameter = loopParameter ?? throw new ArgumentNullException(nameof(loopParameter));
            _overrideParameter = overrideParameter ?? throw new ArgumentNullException(nameof(overrideParameter));
            _positionXCurve = positionXCurve != null ? new AnimationCurve(positionXCurve.keys) : AnimationCurve.Constant(0f, 1f, 0f);
            _positionYCurve = positionYCurve != null ? new AnimationCurve(positionYCurve.keys) : AnimationCurve.Constant(0f, 1f, 0f);
            _positionZCurve = positionZCurve != null ? new AnimationCurve(positionZCurve.keys) : AnimationCurve.Constant(0f, 1f, 0f);
        }

        /// <inheritdoc />
        public ISubsystemModifier CreateModifier(TagNode node)
        {
            var duration = Mathf.Max(0f, _durationParameter.Parse(node.Attributes));
            var loop = _loopParameter.Parse(node.Attributes);
            var @override = _overrideParameter.Parse(node.Attributes);
            return new PositionOffsetModifier(duration, _positionXCurve, _positionYCurve, _positionZCurve, loop, @override);
        }
    }
}
