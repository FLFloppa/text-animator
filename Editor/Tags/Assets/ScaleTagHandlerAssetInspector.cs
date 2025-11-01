using System.Globalization;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(ScaleTagHandlerAsset))]
    internal sealed class ScaleTagHandlerAssetInspector : TagHandlerAssetInspectorBase<ScaleTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var durationProperty = serializedObject.FindProperty("duration");
            var loopProperty = serializedObject.FindProperty("loop");
            var scaleXProperty = serializedObject.FindProperty("scaleXCurve");
            var scaleYProperty = serializedObject.FindProperty("scaleYCurve");
            var scaleZProperty = serializedObject.FindProperty("scaleZCurve");

            BuildParametersCard(root, durationProperty, loopProperty);
            BuildCurvesCard(root, scaleXProperty, scaleYProperty, scaleZProperty);
        }

        private void BuildParametersCard(VisualElement root, SerializedProperty duration, SerializedProperty loop)
        {
            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Configure reusable parameters that drive animation duration and looping behaviour.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    FormatReference("Duration", duration.objectReferenceValue),
                    FormatReference("Loop", loop.objectReferenceValue));
            }

            UpdateSummary();
            TrackPropertyValue(duration, _ => UpdateSummary());
            TrackPropertyValue(loop, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(duration, "Duration Parameter"));
            content.Add(InspectorUi.Controls.CreatePropertyField(loop, "Loop Parameter"));

            root.Add(card);
        }

        private void BuildCurvesCard(VisualElement root, SerializedProperty scaleX, SerializedProperty scaleY, SerializedProperty scaleZ)
        {
            var card = InspectorUi.Cards.Create(
                "Scale Curves",
                out var content,
                "The value of each curve is applied directly to the respective axis of the character scale.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = string.Join('\n',
                    DescribeCurve("X", scaleX.animationCurveValue),
                    DescribeCurve("Y", scaleY.animationCurveValue),
                    DescribeCurve("Z", scaleZ.animationCurveValue));
            }

            UpdateSummary();
            TrackPropertyValue(scaleX, _ => UpdateSummary());
            TrackPropertyValue(scaleY, _ => UpdateSummary());
            TrackPropertyValue(scaleZ, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(scaleX, "Scale X"));
            content.Add(InspectorUi.Controls.CreatePropertyField(scaleY, "Scale Y"));
            content.Add(InspectorUi.Controls.CreatePropertyField(scaleZ, "Scale Z"));

            root.Add(card);
        }

        private static string DescribeCurve(string axis, AnimationCurve curve)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return $"{axis}-Axis: Empty curve";
            }

            var first = curve.keys[0].value;
            var last = curve.keys[^1].value;
            var culture = CultureInfo.InvariantCulture;
            return $"{axis}-Axis: Keys {curve.length}, {first.ToString("0.###", culture)} → {last.ToString("0.###", culture)}";
        }
    }
}
