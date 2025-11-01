using System.Text;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(TagHandlerRegistryAsset))]
    internal sealed class TagHandlerRegistryAssetInspector : ScriptableObjectInspectorBase<TagHandlerRegistryAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            var handlersProperty = serializedObject.FindProperty("handlers");

            var card = InspectorUi.Cards.Create(
                "Registered Handlers",
                out var content,
                "Controls which tag handler assets are exposed to the parser and their identifier aliases.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                if (handlersProperty == null)
                {
                    summary.text = "No handler list found.";
                    return;
                }

                var total = handlersProperty.arraySize;
                var assigned = 0;
                var missing = 0;

                for (var i = 0; i < total; i++)
                {
                    var element = handlersProperty.GetArrayElementAtIndex(i);
                    if (element.objectReferenceValue != null)
                    {
                        assigned++;
                    }
                    else
                    {
                        missing++;
                    }
                }

                var builder = new StringBuilder();
                builder.AppendLine($"Total slots: {total}");
                builder.AppendLine($"Assigned: {assigned}");
                builder.Append($"Missing: {missing}");
                summary.text = builder.ToString();
            }

            UpdateSummary();
            TrackPropertyValue(handlersProperty, _ => UpdateSummary());

            if (handlersProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(handlersProperty, "Handlers", _ => UpdateSummary()));
            }

            root.Add(card);
        }
    }
}
