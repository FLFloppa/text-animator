using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(CharByCharTagHandlerAsset))]
    internal sealed class CharByCharTagHandlerAssetInspector : TagHandlerAssetInspectorBase<CharByCharTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var perCharacterDurationProperty = serializedObject.FindProperty("perCharacterDuration");
            var revealCountProperty = serializedObject.FindProperty("revealCount");
            var actionProperty = serializedObject.FindProperty("onCharacterAction");

            BuildParametersCard(root, perCharacterDurationProperty, revealCountProperty);
            BuildActionCard(root, actionProperty);
        }

        private void BuildParametersCard(VisualElement root, SerializedProperty durationProperty, SerializedProperty revealCountProperty)
        {
            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Controls per-character reveal timing and the number of characters revealed per step.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    FormatReference("Per-Character Duration", durationProperty.objectReferenceValue),
                    FormatReference("Reveal Count", revealCountProperty.objectReferenceValue));
            }

            UpdateSummary();
            TrackPropertyValue(durationProperty, _ => UpdateSummary());
            TrackPropertyValue(revealCountProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(durationProperty, "Per Character Duration"));
            content.Add(InspectorUi.Controls.CreatePropertyField(revealCountProperty, "Reveal Count"));

            root.Add(card);
        }

        private void BuildActionCard(VisualElement root, SerializedProperty actionProperty)
        {
            var card = InspectorUi.Cards.Create(
                "On Character Action",
                out var content,
                "Optional action invoked for each revealed character.");

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
