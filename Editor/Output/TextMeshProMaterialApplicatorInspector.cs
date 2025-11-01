using FLFloppa.TextAnimator.Output.TextMeshPro;
using UnityEditor;

namespace FLFloppa.TextAnimator.Editor.Output
{
    [CustomEditor(typeof(TextMeshProMaterialApplicator))]
    internal sealed class TextMeshProMaterialApplicatorInspector : TextOutputApplicatorAssetInspectorBase<TextMeshProMaterialApplicator>
    {
        protected override string GetTitle() => "TextMeshPro Material Applicator";

        protected override string GetDescription() =>
            "Applies material subsystem overrides by writing to Renderer property blocks.";
    }
}
