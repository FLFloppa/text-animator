using FLFloppa.TextAnimator.Animator.Subsystems.Color;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Animator
{
    [CustomEditor(typeof(ColorTextAnimatorSubsystem))]
    internal sealed class ColorTextAnimatorSubsystemInspector : TextAnimatorSubsystemAssetInspectorBase<ColorTextAnimatorSubsystem>
    {
        protected override void BuildInspector(VisualElement root)
        {
            BuildSubsystemCard(
                root,
                "Color Subsystem",
                "Produces color pipeline segments and registers color applicators for supported outputs.");
        }
    }
}
