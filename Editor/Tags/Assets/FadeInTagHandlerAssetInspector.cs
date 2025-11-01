using System.Globalization;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Tags.Assets
{
    [CustomEditor(typeof(FadeInTagHandlerAsset))]
    internal sealed class FadeInTagHandlerAssetInspector : TagHandlerAssetInspectorBase<FadeInTagHandlerAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            AddTagIdentifiersSection(root);

            var durationProperty = serializedObject.FindProperty("duration");
            var alphaCurveProperty = serializedObject.FindProperty("alphaCurve");

            BuildParametersCard(root, durationProperty);
            BuildCurveCard(root, alphaCurveProperty);
        }

        private void BuildParametersCard(VisualElement root, SerializedProperty durationProperty)
        {
            var card = InspectorUi.Cards.Create(
                "Parameters",
                out var content,
                "Duration determines how long the fade-in animation lasts.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = FormatReference("Duration", durationProperty.objectReferenceValue);
            }

            UpdateSummary();
            TrackPropertyValue(durationProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(durationProperty, "Duration Parameter"));
            root.Add(card);
        }

        private void BuildCurveCard(VisualElement root, SerializedProperty alphaCurveProperty)
        {
            var card = InspectorUi.Cards.Create(
                "Alpha Curve",
                out var content,
                "Curve values are applied directly to character alpha over the animation duration.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                summary.text = DescribeCurve(alphaCurveProperty.animationCurveValue);
            }

            UpdateSummary();
            TrackPropertyValue(alphaCurveProperty, _ => UpdateSummary());

            content.Add(InspectorUi.Controls.CreatePropertyField(alphaCurveProperty, "Alpha Curve"));
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
            return $"Curve: Keys {curve.length}, {first.ToString("0.###", culture)} → {last.ToString("0.###", culture)}";
        }
    }
}
