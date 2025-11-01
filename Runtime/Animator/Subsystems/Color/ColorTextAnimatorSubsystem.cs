using System.Collections.Generic;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Output.TextMeshPro;
using UnityEngine;

namespace FLFloppa.TextAnimator.Animator.Subsystems.Color
{
    /// <summary>
    /// Provides color-related pipeline segments.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ColorTextAnimatorSubsystem",
        menuName = "FLFloppa/Text Animator/Subsystems/Color",
        order = 520)]
    public sealed class ColorTextAnimatorSubsystem : TextAnimatorSubsystemAsset
    {
        /// <inheritdoc />
        public override TextAnimatorSubsystemKey Key => TextAnimatorSubsystemKeys.Color;

        /// <inheritdoc />
        public override ICharacterAnimationPipelineSegment CreatePipelineSegment()
        {
            return new ColorCharacterAnimationSegment();
        }

        /// <inheritdoc />
        protected override IEnumerable<ITextOutputApplicator> CreateDefaultApplicators()
        {
            yield return CreateRuntimeApplicator<TextMeshProColorApplicator>();
        }
    }
}
