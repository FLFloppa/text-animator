using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(RainbowTagHandlerAsset))]
    internal sealed class RainbowTagHandlerAssetInspector : TagHandlerAssetInspectorBase<RainbowTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var durationProperty = serializedObject.FindProperty("duration");
            var loopProperty = serializedObject.FindProperty("loop");
            var colorShiftProperty = serializedObject.FindProperty("colorShift");
            var gradientProperty = serializedObject.FindProperty("gradient");

            BuildParametersCard(root, durationProperty, loopProperty, colorShiftProperty);
            BuildGradientCard(root, gradientProperty);
        }

        private void BuildParametersCard(VisualElement root, SerializedProperty duration, SerializedProperty loop, SerializedProperty colorShift)
        {
            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Assign reusable parameter definitions that control timing, looping and per-character color shifts.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    FormatReference("Duration", duration.objectReferenceValue),
                    FormatReference("Loop", loop.objectReferenceValue),
                    FormatReference("Color Shift", colorShift.objectReferenceValue));
            }

            UpdateSummary();
            RegisterUndoRedoCallback(UpdateSummary);

            content.Add(InspectorUi.Controls.CreatePropertyField(duration, "Duration Parameter", _ => UpdateSummary()));
            content.Add(InspectorUi.Controls.CreatePropertyField(loop, "Loop Parameter", _ => UpdateSummary()));
            content.Add(InspectorUi.Controls.CreatePropertyField(colorShift, "Color Shift Parameter", _ => UpdateSummary()));

            root.Add(card);
        }

        private void BuildGradientCard(VisualElement root, SerializedProperty gradientProperty)
        {
            var card = InspectorUi.Cards.Create(
                "Gradient",
                out var content,
                "Defines the rainbow colours sampled across characters and time.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                var gradient = gradientProperty.gradientValue;
                if (gradient == null)
                {
                    summary.text = "Gradient asset is missing.";
                    return;
                }

                summary.text = $"Colour Keys: {gradient.colorKeys.Length}, Alpha Keys: {gradient.alphaKeys.Length}";
            }

            UpdateSummary();
            RegisterUndoRedoCallback(UpdateSummary);

            content.Add(InspectorUi.Controls.CreatePropertyField(gradientProperty, "Gradient", _ => UpdateSummary()));
            root.Add(card);
        }
    }
}
