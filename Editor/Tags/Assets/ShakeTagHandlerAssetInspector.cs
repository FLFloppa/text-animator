using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(ShakeTagHandlerAsset))]
    internal sealed class ShakeTagHandlerAssetInspector : TagHandlerAssetInspectorBase<ShakeTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var amplitudeProperty = serializedObject.FindProperty("amplitude");
            var frequencyProperty = serializedObject.FindProperty("frequency");
            var synchronizeProperty = serializedObject.FindProperty("synchronize");

            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Amplitude controls offset strength, frequency drives oscillation speed, and synchronize toggles shared motion.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    FormatReference("Amplitude", amplitudeProperty.objectReferenceValue),
                    FormatReference("Frequency", frequencyProperty.objectReferenceValue),
                    FormatReference("Synchronize", synchronizeProperty.objectReferenceValue));
            }

            UpdateSummary();
            TrackPropertyValue(amplitudeProperty, _ => UpdateSummary());
            TrackPropertyValue(frequencyProperty, _ => UpdateSummary());
            TrackPropertyValue(synchronizeProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(amplitudeProperty, "Amplitude Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(frequencyProperty, "Frequency Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(synchronizeProperty, "Synchronize Parameter"));

            root.Add(card);
        }
    }
}
