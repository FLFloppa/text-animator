using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.States;
using FLFloppa.TextAnimator.Playback.Systems;
using UnityEngine;

namespace FLFloppa.TextAnimator.Output.TextMeshPro
{
    /// <summary>
    /// Applies transform subsystem state to a <see cref="TextMeshProOutput"/> instance.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TextMeshProTransformApplicator",
        menuName = "FLFloppa/Text Animator/Applicators/TextMeshPro Transform",
        order = 610)]
    public sealed class TextMeshProTransformApplicator : TextOutputApplicatorAsset<TextMeshProOutput, TransformCharacterState>, ITransformTextOutputApplicator<TextMeshProOutput>
    {
        /// <inheritdoc />
        public override TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Transform;

        /// <inheritdoc />
        protected override void ApplyTyped(TextMeshProOutput output, int characterIndex, TransformCharacterState state)
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

            ref var charInfo = ref textInfo.characterInfo[characterIndex];
            if (!charInfo.isVisible)
            {
                return;
            }

            var vertexIndex = charInfo.vertexIndex;
            var materialIndex = charInfo.materialReferenceIndex;
            var meshInfo = textInfo.meshInfo[materialIndex];
            var vertices = meshInfo.vertices;
            if (vertices == null || vertices.Length == 0)
            {
                return;
            }

            var center = (vertices[vertexIndex] + vertices[vertexIndex + 2]) * 0.5f;
            
            Vector3 pivotPoint = center;
            if (state.UseGroupPivot && state.GroupStartIndex >= 0 && state.GroupEndIndex > state.GroupStartIndex)
            {
                pivotPoint = ComputeGroupPivot(textInfo, state.GroupStartIndex, state.GroupEndIndex);
            }
            
            var matrix = Matrix4x4.TRS(Vector3.zero, state.Rotation, state.Scale);

            for (var i = 0; i < 4; i++)
            {
                var offset = vertices[vertexIndex + i] - pivotPoint;
                var transformedOffset = matrix.MultiplyPoint3x4(offset);
                vertices[vertexIndex + i] = pivotPoint + transformedOffset + state.PositionOffset;
            }
        }

        private Vector3 ComputeGroupPivot(TMPro.TMP_TextInfo textInfo, int groupStartIndex, int groupEndIndex)
        {
            var sum = Vector3.zero;
            var count = 0;

            for (var i = groupStartIndex; i < groupEndIndex && i < textInfo.characterCount; i++)
            {
                ref var charInfo = ref textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                {
                    continue;
                }

                var vertexIndex = charInfo.vertexIndex;
                var materialIndex = charInfo.materialReferenceIndex;
                var meshInfo = textInfo.meshInfo[materialIndex];
                var vertices = meshInfo.vertices;
                
                if (vertices != null && vertexIndex + 2 < vertices.Length)
                {
                    var charCenter = (vertices[vertexIndex] + vertices[vertexIndex + 2]) * 0.5f;
                    sum += charCenter;
                    count++;
                }
            }

            return count > 0 ? sum / count : Vector3.zero;
        }
    }
}
