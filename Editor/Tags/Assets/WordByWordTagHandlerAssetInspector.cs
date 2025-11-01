using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(WordByWordTagHandlerAsset))]
    internal sealed class WordByWordTagHandlerAssetInspector : TagHandlerAssetInspectorBase<WordByWordTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var durationProperty = serializedObject.FindProperty("perWordDuration");
            var wordsPerBatchProperty = serializedObject.FindProperty("wordsPerBatch");
            var actionProperty = serializedObject.FindProperty("onWordAction");

            BuildParametersCard(root, durationProperty, wordsPerBatchProperty);
            BuildActionCard(root, actionProperty);
        }

        private void BuildParametersCard(VisualElement root, SerializedProperty durationProperty, SerializedProperty wordsPerBatchProperty)
        {
            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Configure per-word reveal timing and how many words appear in each batch.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    FormatReference("Per-Word Duration", durationProperty.objectReferenceValue),
                    FormatReference("Words per Batch", wordsPerBatchProperty.objectReferenceValue));
            }

            UpdateSummary();
            TrackPropertyValue(durationProperty, _ => UpdateSummary());
            TrackPropertyValue(wordsPerBatchProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(durationProperty, "Per Word Duration"));
            content.Add(InspectorUi.Controls.CreatePropertyField(wordsPerBatchProperty, "Words per Batch"));

            root.Add(card);
        }

        private void BuildActionCard(VisualElement root, SerializedProperty actionProperty)
        {
            var card = InspectorUi.Cards.Create(
                "On Word Action",
                out var content,
                "Optional action triggered when each word becomes visible.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = actionProperty.objectReferenceValue == null
                    ? "No action assigned."
                    : $"Action Asset: {actionProperty.objectReferenceValue.name}";
            }

            UpdateSummary();
            TrackPropertyValue(actionProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(actionProperty, "Action Asset"));
            root.Add(card);
        }
    }
}
