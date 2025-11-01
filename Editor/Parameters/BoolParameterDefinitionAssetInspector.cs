using System.Text;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Parameters;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Parameters
{
    [CustomEditor(typeof(BoolParameterDefinitionAsset))]
    internal sealed class BoolParameterDefinitionAssetInspector : ScriptableObjectInspectorBase<BoolParameterDefinitionAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            var identifiersProperty = serializedObject.FindProperty("identifiers");
            var defaultValueProperty = serializedObject.FindProperty("defaultValue");
            var trueValuesProperty = serializedObject.FindProperty("trueValues");
            var falseValuesProperty = serializedObject.FindProperty("falseValues");
            var ignoreCaseProperty = serializedObject.FindProperty("ignoreCase");

            var card = InspectorUi.Cards.Create(
                "Boolean Parameter",
                out var content,
                "Configures alias tokens interpreted as true or false in markup.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                var builder = new StringBuilder();
                builder.AppendLine($"Identifiers: {identifiersProperty?.arraySize ?? 0}");
                builder.AppendLine($"Default Value: {(defaultValueProperty?.boolValue ?? false)}");
                builder.AppendLine($"True Tokens: {trueValuesProperty?.arraySize ?? 0}");
                builder.AppendLine($"False Tokens: {falseValuesProperty?.arraySize ?? 0}");
                builder.Append($"Ignore Case: {(ignoreCaseProperty?.boolValue ?? true)}");
                summary.text = builder.ToString();
            }

            UpdateSummary();
            TrackPropertyValue(identifiersProperty, _ => UpdateSummary());
            TrackPropertyValue(defaultValueProperty, _ => UpdateSummary());
            TrackPropertyValue(trueValuesProperty, _ => UpdateSummary());
            TrackPropertyValue(falseValuesProperty, _ => UpdateSummary());
            TrackPropertyValue(ignoreCaseProperty, _ => UpdateSummary());

            if (identifiersProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(identifiersProperty, "Identifiers", _ => UpdateSummary()));
            }

            if (defaultValueProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(defaultValueProperty, "Default Value", _ => UpdateSummary()));
            }

            if (trueValuesProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(trueValuesProperty, "True Tokens", _ => UpdateSummary()));
            }

            if (falseValuesProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(falseValuesProperty, "False Tokens", _ => UpdateSummary()));
            }

            if (ignoreCaseProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(ignoreCaseProperty, "Ignore Case", _ => UpdateSummary()));
            }

            root.Add(card);
        }
    }
}
