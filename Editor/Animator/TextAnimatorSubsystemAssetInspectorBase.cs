using System.Text;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Editor.Common;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Animator
{
    internal abstract class TextAnimatorSubsystemAssetInspectorBase<TAsset> : ScriptableObjectInspectorBase<TAsset>
        where TAsset : TextAnimatorSubsystemAsset
    {
        protected void BuildSubsystemCard(VisualElement root, string title, string description)
        {
            var applicatorsProperty = serializedObject.FindProperty("applicators");

            var card = InspectorUi.Cards.Create(
                title,
                out var content,
                description);

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                var builder = new StringBuilder();
                builder.AppendLine($"Key: {TargetAsset.Key}");

                if (applicatorsProperty == null)
                {
                    builder.Append("Applicators: Not serialized");
                    summary.text = builder.ToString();
                    return;
                }

                var total = applicatorsProperty.arraySize;
                var assigned = 0;
                for (var i = 0; i < total; i++)
                {
                    if (applicatorsProperty.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    {
                        assigned++;
                    }
                }

                builder.Append($"Applicators: {assigned}/{total}");
                summary.text = builder.ToString();
            }

            UpdateSummary();
            TrackPropertyValue(applicatorsProperty, _ => UpdateSummary());

            if (applicatorsProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(applicatorsProperty, "Applicators", _ => UpdateSummary()));
            }

            root.Add(card);
        }
    }
}
