using System.Text;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Editor.Common;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Animator
{
    [CustomEditor(typeof(TextAnimatorSubsystemBundleAsset))]
    internal sealed class TextAnimatorSubsystemBundleAssetInspector : ScriptableObjectInspectorBase<TextAnimatorSubsystemBundleAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            var subsystemsProperty = serializedObject.FindProperty("subsystems");

            var card = InspectorUi.Cards.Create(
                "Subsystem Bundle",
                out var content,
                "Groups multiple subsystem assets for easy reuse across animator configurations.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                if (subsystemsProperty == null)
                {
                    summary.text = "No subsystem array serialized.";
                    return;
                }

                var total = subsystemsProperty.arraySize;
                var assigned = 0;
                var builder = new StringBuilder();

                for (var i = 0; i < total; i++)
                {
                    if (subsystemsProperty.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    {
                        assigned++;
                    }
                }

                builder.AppendLine($"Total entries: {total}");
                builder.Append($"Assigned: {assigned}");
                summary.text = builder.ToString();
            }

            UpdateSummary();
            TrackPropertyValue(subsystemsProperty, _ => UpdateSummary());

            if (subsystemsProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(subsystemsProperty, "Subsystems", _ => UpdateSummary()));
            }

            root.Add(card);
        }
    }
}
