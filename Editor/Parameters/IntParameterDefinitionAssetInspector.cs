using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Parameters;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Parameters
{
    [CustomEditor(typeof(IntParameterDefinitionAsset))]
    internal sealed class IntParameterDefinitionAssetInspector : ScriptableObjectInspectorBase<IntParameterDefinitionAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            var identifiersProperty = serializedObject.FindProperty("identifiers");
            var defaultValueProperty = serializedObject.FindProperty("defaultValue");

            var card = InspectorUi.Cards.Create(
                "Integer Parameter",
                out var content,
                "Supplies integer values to tags and other runtime systems.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                var identifierCount = identifiersProperty?.arraySize ?? 0;
                var defaultValue = defaultValueProperty != null ? defaultValueProperty.intValue : 0;
                summary.text = $"Identifiers: {identifierCount}\nDefault Value: {defaultValue}";
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
