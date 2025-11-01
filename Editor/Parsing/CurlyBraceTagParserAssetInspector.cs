using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Parsing;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Parsing
{
    [CustomEditor(typeof(CurlyBraceTagParserAsset))]
    internal sealed class CurlyBraceTagParserAssetInspector : ScriptableObjectInspectorBase<CurlyBraceTagParserAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            var card = InspectorUi.Cards.Create(
                "Curly Brace Parser",
                out var content,
                "Generates tag parsers that interpret {tag=value} style markup.");

            var summary = InspectorUi.Controls.CreateSummaryLabel("Produces a new parser instance on demand.");
            content.Add(summary);

            root.Add(card);
        }
    }
}
