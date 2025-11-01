using System.Globalization;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(RotateTagHandlerAsset))]
    internal sealed class RotateTagHandlerAssetInspector : TagHandlerAssetInspectorBase<RotateTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var durationProperty = serializedObject.FindProperty("duration");
            var loopProperty = serializedObject.FindProperty("loop");
            var rotationCurveProperty = serializedObject.FindProperty("rotationCurve");

            BuildParametersCard(root, durationProperty, loopProperty);
            BuildCurveCard(root, rotationCurveProperty);
        }

        private void BuildParametersCard(VisualElement root, SerializedProperty duration, SerializedProperty loop)
        {
            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Configure the timing and looping behaviour for rotation.");

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

        private void BuildCurveCard(VisualElement root, SerializedProperty curveProperty)
        {
            var card = InspectorUi.Cards.Create(
                "Rotation Curve",
                out var content,
                "Curve values are interpreted directly as rotation angles in degrees.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                var curve = curveProperty.animationCurveValue;
                summary.text = DescribeCurve(curve);
            }

            UpdateSummary();
            TrackPropertyValue(curveProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(curveProperty, "Rotation Curve"));
            root.Add(card);
        }

        private static string DescribeCurve(AnimationCurve curve)
        {
            if (curve == null || curve.keys.Length == 0)
            {
                return "Curve: Empty";
            }

            var culture = CultureInfo.InvariantCulture;
            var first = curve.keys[0].value;
            var last = curve.keys[^1].value;
            return $"Curve: Keys {curve.length}, {first.ToString("0.###", culture)}° → {last.ToString("0.###", culture)}°";
        }
    }
}
