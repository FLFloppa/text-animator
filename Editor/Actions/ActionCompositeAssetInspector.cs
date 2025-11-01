using System.Text;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Tags.Assets.Actions;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Actions
{
    [CustomEditor(typeof(ActionCompositeAsset))]
    internal sealed class ActionCompositeAssetInspector : ScriptableObjectInspectorBase<ActionCompositeAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            var actionsProperty = serializedObject.FindProperty("actions");

            var card = InspectorUi.Cards.Create(
                "Composite Actions",
                out var content,
                "Executes each child action sequentially when invoked by a tag handler.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                if (actionsProperty == null)
                {
                    summary.text = "No actions list serialized.";
                    return;
                }

                var total = actionsProperty.arraySize;
                var assigned = 0;
                var builder = new StringBuilder();

                for (var i = 0; i < total; i++)
                {
                    var element = actionsProperty.GetArrayElementAtIndex(i);
                    if (element.objectReferenceValue != null)
                    {
                        assigned++;
                    }
                }

                builder.AppendLine($"Total actions: {total}");
                builder.Append($"Assigned: {assigned}");
                summary.text = builder.ToString();
            }

            UpdateSummary();
            TrackPropertyValue(actionsProperty, _ => UpdateSummary());

            if (actionsProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(actionsProperty, "Actions", _ => UpdateSummary()));
            }

            root.Add(card);
        }
    }
}
