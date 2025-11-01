using System;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Characters.States;
using FLFloppa.TextAnimator.Playback.Systems;
using UnityEngine;

namespace FLFloppa.TextAnimator.Output.TextMeshPro
{
    /// <summary>
    /// Applies color subsystem state to a <see cref="TextMeshProOutput"/> instance.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TextMeshProColorApplicator",
        menuName = "FLFloppa/Text Animator/Applicators/TextMeshPro Color",
        order = 620)]
    public sealed class TextMeshProColorApplicator : TextOutputApplicatorAsset<TextMeshProOutput, ColorCharacterState>, IColorTextOutputApplicator<TextMeshProOutput>
    {
        /// <inheritdoc />
        public override TextAnimatorSubsystemKey TargetSubsystem => TextAnimatorSubsystemKeys.Color;

        /// <inheritdoc />
        protected override void ApplyTyped(TextMeshProOutput output, int characterIndex, ColorCharacterState state)
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
            var vertexIndex = charInfo.vertexIndex;
            var meshInfo = textInfo.meshInfo[materialIndex];
            var colors = meshInfo.colors32;
            if (colors == null || colors.Length == 0)
            {
                return;
            }

            var baseColor = output.GetBaseVertexColor(materialIndex, vertexIndex);
            Color32 color;
            
            if (state.OverrideRGB)
            {
                // New behavior: multiply RGB for effects like Rainbow, but take alpha from state directly
                var stateColor = state.Color;
                color = new Color32(
                    (byte)((baseColor.r / 255f) * stateColor.r * 255f),
                    (byte)((baseColor.g / 255f) * stateColor.g * 255f),
                    (byte)((baseColor.b / 255f) * stateColor.b * 255f),
                    (byte)(stateColor.a * 255f)
                );
            }
            else
            {
                // Original behavior: keep base RGB, only use state alpha
                color = (Color32)state.Color;
                color.r = baseColor.r;
                color.g = baseColor.g;
                color.b = baseColor.b;
            }

            colors[vertexIndex] = color;
            colors[vertexIndex + 1] = color;
            colors[vertexIndex + 2] = color;
            colors[vertexIndex + 3] = color;
        }
    }
}
