using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(ActionTagHandlerAsset))]
    internal sealed class ActionTagHandlerAssetInspector : TagHandlerAssetInspectorBase<ActionTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var actionProperty = serializedObject.FindProperty("action");

            var overviewCard = InspectorUi.Cards.Create(
                "Action",
                out var overviewContent,
                "Invokes the assigned ActionAsset whenever the tag is processed by the playback timeline.");

            var summaryLabel = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            overviewContent.Add(summaryLabel);

            void UpdateSummary()
            {
                summaryLabel.text = actionProperty.objectReferenceValue == null
                    ? "No ActionAsset assigned. Tag will be ignored at runtime."
                    : $"Action Asset: {actionProperty.objectReferenceValue.name}";
            }

            UpdateSummary();
            RegisterUndoRedoCallback(UpdateSummary);

            overviewContent.Add(InspectorUi.Controls.CreatePropertyField(actionProperty, "Action Asset", _ => UpdateSummary()));
            root.Add(overviewCard);
        }
    }
}
