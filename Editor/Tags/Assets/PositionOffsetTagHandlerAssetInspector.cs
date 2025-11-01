using System.Globalization;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(PositionOffsetTagHandlerAsset))]
    internal sealed class PositionOffsetTagHandlerAssetInspector : TagHandlerAssetInspectorBase<PositionOffsetTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var durationProperty = serializedObject.FindProperty("duration");
            var loopProperty = serializedObject.FindProperty("loop");
            var overrideProperty = serializedObject.FindProperty("override");
            var posXProperty = serializedObject.FindProperty("positionXCurve");
            var posYProperty = serializedObject.FindProperty("positionYCurve");
            var posZProperty = serializedObject.FindProperty("positionZCurve");

            BuildParametersCard(root, durationProperty, loopProperty, overrideProperty);
            BuildCurvesCard(root, posXProperty, posYProperty, posZProperty);
        }

        private void BuildParametersCard(VisualElement root, SerializedProperty duration, SerializedProperty loop, SerializedProperty overrideProperty)
        {
            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Select parameter definitions controlling animation duration, looping behaviour and whether offsets override existing values.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    FormatReference("Duration", duration.objectReferenceValue),
                    FormatReference("Loop", loop.objectReferenceValue),
                    FormatReference("Override", overrideProperty.objectReferenceValue));
            }

            UpdateSummary();
            TrackPropertyValue(duration, _ => UpdateSummary());
            TrackPropertyValue(loop, _ => UpdateSummary());
            TrackPropertyValue(overrideProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(duration, "Duration Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(loop, "Loop Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(overrideProperty, "Override Parameter"));

            root.Add(card);
        }

        private void BuildCurvesCard(VisualElement root, SerializedProperty posX, SerializedProperty posY, SerializedProperty posZ)
        {
            var card = InspectorUi.Cards.Create(
                "Position Curves",
                out var content,
                "Each curve's value is applied directly to the respective axis of the position offset.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    DescribeCurve("X", posX.animationCurveValue),
                    DescribeCurve("Y", posY.animationCurveValue),
                    DescribeCurve("Z", posZ.animationCurveValue));
            }

            UpdateSummary();
            TrackPropertyValue(posX, _ => UpdateSummary());
            TrackPropertyValue(posY, _ => UpdateSummary());
            TrackPropertyValue(posZ, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(posX, "Offset X"));
            content.Add(InspectorUi.Controls.CreatePropertyField(posY, "Offset Y"));
            content.Add(InspectorUi.Controls.CreatePropertyField(posZ, "Offset Z"));

            root.Add(card);
        }

        private static string DescribeCurve(string axis, AnimationCurve curve)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return $"{axis}-Axis: Empty curve";
            }

            var culture = CultureInfo.InvariantCulture;
            var first = curve.keys[0].value;
            var last = curve.keys[^1].value;
            return $"{axis}-Axis: Keys {curve.length}, {first.ToString("0.###", culture)} → {last.ToString("0.###", culture)}";
        }
    }
}
