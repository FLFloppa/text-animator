using FLFloppa.TextAnimator.Animator.Subsystems.Material;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Animator
{
    [CustomEditor(typeof(MaterialTextAnimatorSubsystem))]
    internal sealed class MaterialTextAnimatorSubsystemInspector : TextAnimatorSubsystemAssetInspectorBase<MaterialTextAnimatorSubsystem>
    {
        protected override void BuildInspector(VisualElement root)
        {
            BuildSubsystemCard(
                root,
                "Material Subsystem",
                "Controls material overrides such as shader property blocks applied per character.");
        }
    }
}
