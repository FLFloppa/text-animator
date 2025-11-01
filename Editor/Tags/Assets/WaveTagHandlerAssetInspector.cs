using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(WaveTagHandlerAsset))]
    internal sealed class WaveTagHandlerAssetInspector : TagHandlerAssetInspectorBase<WaveTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var amplitudeProperty = serializedObject.FindProperty("amplitude");
            var frequencyProperty = serializedObject.FindProperty("frequency");
            var phaseProperty = serializedObject.FindProperty("phase");

            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Amplitude determines displacement, frequency controls oscillation speed, and phase sets the initial offset.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    FormatReference("Amplitude", amplitudeProperty.objectReferenceValue),
                    FormatReference("Frequency", frequencyProperty.objectReferenceValue),
                    FormatReference("Phase", phaseProperty.objectReferenceValue));
            }

            UpdateSummary();
            TrackPropertyValue(amplitudeProperty, _ => UpdateSummary());
            TrackPropertyValue(frequencyProperty, _ => UpdateSummary());
            TrackPropertyValue(phaseProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(amplitudeProperty, "Amplitude Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(frequencyProperty, "Frequency Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(phaseProperty, "Phase Parameter"));

            root.Add(card);
        }
    }
}
