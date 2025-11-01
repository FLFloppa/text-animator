using System.Collections.Generic;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Output.TextMeshPro;
using UnityEngine;

namespace FLFloppa.TextAnimator.Animator.Subsystems.Transform
{
    /// <summary>
    /// Provides transform-related pipeline segments.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TransformTextAnimatorSubsystem",
        menuName = "FLFloppa/Text Animator/Subsystems/Transform",
        order = 510)]
    public sealed class TransformTextAnimatorSubsystem : TextAnimatorSubsystemAsset
    {
        /// <inheritdoc />
        public override TextAnimatorSubsystemKey Key => TextAnimatorSubsystemKeys.Transform;

        /// <inheritdoc />
        public override ICharacterAnimationPipelineSegment CreatePipelineSegment()
        {
            return new TransformCharacterAnimationSegment();
        }

        /// <inheritdoc />
        protected override IEnumerable<ITextOutputApplicator> CreateDefaultApplicators()
        {
            yield return CreateRuntimeApplicator<TextMeshProTransformApplicator>();
        }
    }
}
