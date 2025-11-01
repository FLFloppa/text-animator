using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Output;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Output
{
    internal abstract class TextOutputApplicatorAssetInspectorBase<TAsset> : ScriptableObjectInspectorBase<TAsset>
        where TAsset : TextOutputApplicatorAsset
    {
        protected override void BuildInspector(VisualElement root)
        {
            BuildApplicatorCard(root, GetTitle(), GetDescription());
        }

        protected abstract string GetTitle();

        protected abstract string GetDescription();

        protected virtual void AppendApplicatorContent(VisualElement content)
        {
        }

        private void BuildApplicatorCard(VisualElement root, string title, string description)
        {
            var card = InspectorUi.Cards.Create(title, out var content, description);

            var subsystem = TargetAsset.TargetSubsystem;
            var outputType = TargetAsset.OutputType;
            var summary = InspectorUi.Controls.CreateSummaryLabel(
                $"Target Subsystem: {subsystem}\nOutput Type: {outputType?.Name ?? "Unknown"}");
            content.Add(summary);

            AppendApplicatorContent(content);
            root.Add(card);
        }
    }
}
