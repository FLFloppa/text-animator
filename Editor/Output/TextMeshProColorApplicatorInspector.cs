using FLFloppa.TextAnimator.Output.TextMeshPro;
using UnityEditor;

namespace FLFloppa.TextAnimator.Editor.Output
{
    [CustomEditor(typeof(TextMeshProColorApplicator))]
    internal sealed class TextMeshProColorApplicatorInspector : TextOutputApplicatorAssetInspectorBase<TextMeshProColorApplicator>
    {
        protected override string GetTitle() => "TextMeshPro Color Applicator";

        protected override string GetDescription() =>
            "Blends color subsystem results with TextMeshPro vertex colors, respecting OverrideRGB state.";
    }
}
