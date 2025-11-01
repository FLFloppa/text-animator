using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(WaitTagHandlerAsset))]
    internal sealed class WaitTagHandlerAssetInspector : TagHandlerAssetInspectorBase<WaitTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var durationProperty = serializedObject.FindProperty("duration");

            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Specifies the duration that playback should pause when this tag is encountered.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = FormatReference("Duration", durationProperty.objectReferenceValue);
            }

            UpdateSummary();
            TrackPropertyValue(durationProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(durationProperty, "Duration Parameter"));
            root.Add(card);
        }
    }
}
