using FLFloppa.TextAnimator.Output.TextMeshPro;
using UnityEditor;

namespace FLFloppa.TextAnimator.Editor.Output
{
    [CustomEditor(typeof(TextMeshProTransformApplicator))]
    internal sealed class TextMeshProTransformApplicatorInspector : TextOutputApplicatorAssetInspectorBase<TextMeshProTransformApplicator>
    {
        protected override string GetTitle() => "TextMeshPro Transform Applicator";

        protected override string GetDescription() =>
            "Applies transform subsystem state (position, rotation, scale) to TextMeshPro outputs.";
    }
}
