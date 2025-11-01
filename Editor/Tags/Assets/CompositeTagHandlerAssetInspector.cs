using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(CompositeTagHandlerAsset))]
    internal sealed class CompositeTagHandlerAssetInspector : TagHandlerAssetInspectorBase<CompositeTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var childHandlersProperty = serializedObject.FindProperty("childHandlers");

            var card = InspectorUi.Cards.Create(
                "Child Handlers",
                out var content,
                "Composite handlers combine multiple tag handlers. Only handlers producing character modifiers are valid.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                var assignedCount = childHandlersProperty.arraySize;
                summary.text = assignedCount == 0
                    ? "No child handlers configured."
                    : $"Child handlers assigned: {assignedCount}";
            }

            UpdateSummary();
            TrackPropertyValue(childHandlersProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(childHandlersProperty, "Child Handlers"));
            root.Add(card);
        }
    }
}
