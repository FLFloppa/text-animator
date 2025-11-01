using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    internal abstract class TagHandlerAssetInspectorBase<TAsset> : ScriptableObjectInspectorBase<TAsset>
        where TAsset : TagHandlerAsset
    {
        protected void AddTagIdentifiersSection(VisualElement root)
        {
            var identifiersProperty = serializedObject.FindProperty("tagIdentifiers");
            var card = InspectorUi.Cards.Create(
                "Tag Identifiers",
                out var content,
                "List all aliases that should invoke this handler inside markup tags.");
            var summaryLabel = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summaryLabel);

            void UpdateSummary()
            {
                var count = identifiersProperty.arraySize;
                summaryLabel.text = count == 0
                    ? "No aliases configured. Tags will use the asset file name."
                    : $"Registered aliases: {count}";
            }

            UpdateSummary();
            TrackPropertyValue(identifiersProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(identifiersProperty, "Aliases", _ => UpdateSummary()));
            root.Add(card);
        }
    }
}
