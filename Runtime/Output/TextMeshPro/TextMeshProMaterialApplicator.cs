using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.States;
using FLFloppa.TextAnimator.Playback.Systems;
using UnityEngine;

namespace FLFloppa.TextAnimator.Output.TextMeshPro
{
    /// <summary>
    /// Applies material subsystem state to a <see cref="TextMeshProOutput"/> instance.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TextMeshProMaterialApplicator",
        menuName = "FLFloppa/Text Animator/Applicators/TextMeshPro Material",
        order = 630)]
    public sealed class TextMeshProMaterialApplicator : TextOutputApplicatorAsset<TextMeshProOutput, MaterialCharacterState>, IMaterialTextOutputApplicator<TextMeshProOutput>
    {
        /// <inheritdoc />
        public override TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Material;

        /// <inheritdoc />
        protected override void ApplyTyped(TextMeshProOutput output, int characterIndex, MaterialCharacterState state)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (state == null)
            {
                return;
            }

            var textComponent = output.TextComponent;
            var textInfo = textComponent.textInfo;
            if (characterIndex < 0 || characterIndex >= textInfo.characterCount)
            {
                return;
            }

            var charInfo = textInfo.characterInfo[characterIndex];
            if (!charInfo.isVisible)
            {
                return;
            }

            var materialIndex = charInfo.materialReferenceIndex;
            var renderer = output.GetRenderer();
            if (renderer == null)
            {
                return;
            }

            var propertyBlock = output.GetPropertyBlock(materialIndex);
            propertyBlock.Clear();
            renderer.GetPropertyBlock(propertyBlock, materialIndex);

            foreach (var kvp in state.FloatOverrides)
            {
                propertyBlock.SetFloat(kvp.Key, kvp.Value);
            }

            renderer.SetPropertyBlock(propertyBlock, materialIndex);
        }
    }
}
