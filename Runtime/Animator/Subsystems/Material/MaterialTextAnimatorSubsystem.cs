using System.Collections.Generic;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Output.TextMeshPro;
using UnityEngine;

namespace FLFloppa.TextAnimator.Animator.Subsystems.Material
{
    /// <summary>
    /// Provides material-related pipeline segments.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MaterialTextAnimatorSubsystem",
        menuName = "FLFloppa/Text Animator/Subsystems/Material",
        order = 530)]
    public sealed class MaterialTextAnimatorSubsystem : TextAnimatorSubsystemAsset
    {
        /// <inheritdoc />
        public override TextAnimatorSubsystemKey Key => TextAnimatorSubsystemKeys.Material;

        /// <inheritdoc />
        public override ICharacterAnimationPipelineSegment CreatePipelineSegment()
        {
            return new MaterialCharacterAnimationSegment();
        }

        /// <inheritdoc />
        protected override IEnumerable<ITextOutputApplicator> CreateDefaultApplicators()
        {
            yield return CreateRuntimeApplicator<TextMeshProMaterialApplicator>();
        }
    }
}
