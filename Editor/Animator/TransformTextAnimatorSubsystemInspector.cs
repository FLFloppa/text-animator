using FLFloppa.TextAnimator.Animator.Subsystems.Transform;
using FLFloppa.TextAnimator.Editor.Animator;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Animator
{
    [CustomEditor(typeof(TransformTextAnimatorSubsystem))]
    internal sealed class TransformTextAnimatorSubsystemInspector : TextAnimatorSubsystemAssetInspectorBase<TransformTextAnimatorSubsystem>
    {
        protected override void BuildInspector(VisualElement root)
        {
            BuildSubsystemCard(
                root,
                "Transform Subsystem",
                "Generates transform pipeline segments affecting per-character position, rotation, and scale.");
        }
    }
}
