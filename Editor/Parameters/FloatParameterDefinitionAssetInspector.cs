using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Parameters;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Parameters
{
    [CustomEditor(typeof(FloatParameterDefinitionAsset))]
    internal sealed class FloatParameterDefinitionAssetInspector : ScriptableObjectInspectorBase<FloatParameterDefinitionAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            var identifiersProperty = serializedObject.FindProperty("identifiers");
            var defaultValueProperty = serializedObject.FindProperty("defaultValue");

            var card = InspectorUi.Cards.Create(
                "Float Parameter",
                out var content,
                "Defines numeric values usable by tag handlers and other systems.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                var identifierCount = identifiersProperty?.arraySize ?? 0;
                var defaultValue = defaultValueProperty != null ? defaultValueProperty.floatValue : 0f;
                summary.text = $"Identifiers: {identifierCount}\nDefault Value: {defaultValue:0.###}";
            }

            UpdateSummary();
            TrackPropertyValue(identifiersProperty, _ => UpdateSummary());
            TrackPropertyValue(defaultValueProperty, _ => UpdateSummary());

            if (identifiersProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(identifiersProperty, "Identifiers", _ => UpdateSummary()));
            }

            if (defaultValueProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(defaultValueProperty, "Default Value", _ => UpdateSummary()));
            }

            root.Add(card);
        }
    }
}
