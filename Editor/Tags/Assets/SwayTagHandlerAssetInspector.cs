using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(SwayTagHandlerAsset))]
    internal sealed class SwayTagHandlerAssetInspector : TagHandlerAssetInspectorBase<SwayTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var frequencyProperty = serializedObject.FindProperty("frequency");
            var amplitudeProperty = serializedObject.FindProperty("amplitude");
            var groupedProperty = serializedObject.FindProperty("grouped");
            var phaseOffsetProperty = serializedObject.FindProperty("phaseOffset");

            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Assign parameter definitions controlling sway frequency, amplitude, grouping and phase offset.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    FormatReference("Frequency", frequencyProperty.objectReferenceValue),
                    FormatReference("Amplitude", amplitudeProperty.objectReferenceValue),
                    FormatReference("Grouped", groupedProperty.objectReferenceValue),
                    FormatReference("Phase Offset", phaseOffsetProperty.objectReferenceValue));
            }

            UpdateSummary();
            TrackPropertyValue(frequencyProperty, _ => UpdateSummary());
            TrackPropertyValue(amplitudeProperty, _ => UpdateSummary());
            TrackPropertyValue(groupedProperty, _ => UpdateSummary());
            TrackPropertyValue(phaseOffsetProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(frequencyProperty, "Frequency Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(amplitudeProperty, "Amplitude Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(groupedProperty, "Grouped Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(phaseOffsetProperty, "Phase Offset Parameter"));

            root.Add(card);
        }
    }
}
