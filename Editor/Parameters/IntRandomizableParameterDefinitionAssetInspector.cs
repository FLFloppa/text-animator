using System.Text;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.Common;
using FLFloppa.TextAnimator.Parameters;
using UnityEditor;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Parameters
{
    [CustomEditor(typeof(IntRandomizableParameterDefinitionAsset))]
    internal sealed class IntRandomizableParameterDefinitionAssetInspector : ScriptableObjectInspectorBase<IntRandomizableParameterDefinitionAsset>
    {
        protected override void BuildInspector(VisualElement root)
        {
            var identifiersProperty = serializedObject.FindProperty("identifiers");
            var defaultCountProperty = serializedObject.FindProperty("defaultCount");
            var defaultUseRandomProperty = serializedObject.FindProperty("defaultUseRandom");
            var defaultRandomMinProperty = serializedObject.FindProperty("defaultRandomMin");
            var defaultRandomMaxProperty = serializedObject.FindProperty("defaultRandomMax");

            var card = InspectorUi.Cards.Create(
                "Int Randomizable Parameter",
                out var content,
                "Generates integer values with optional random ranges for tag parameters.");

            var summary = InspectorUi.Controls.CreateSummaryLabel(string.Empty);
            content.Add(summary);

            void UpdateSummary()
            {
                var builder = new StringBuilder();
                builder.AppendLine($"Identifiers: {identifiersProperty?.arraySize ?? 0}");
                builder.AppendLine($"Default Count: {defaultCountProperty?.intValue ?? 0}");
                var useRandom = defaultUseRandomProperty?.boolValue ?? false;
                builder.AppendLine($"Use Random: {useRandom}");
                if (useRandom)
                {
                    builder.Append($"Range: {defaultRandomMinProperty?.intValue ?? 0} - {defaultRandomMaxProperty?.intValue ?? 0}");
                }
                else
                {
                    builder.Append($"Range: Fixed {defaultCountProperty?.intValue ?? 0}");
                }

                summary.text = builder.ToString();
            }

            UpdateSummary();
            TrackPropertyValue(identifiersProperty, _ => UpdateSummary());
            TrackPropertyValue(defaultCountProperty, _ => UpdateSummary());
            TrackPropertyValue(defaultUseRandomProperty, _ => UpdateSummary());
            TrackPropertyValue(defaultRandomMinProperty, _ => UpdateSummary());
            TrackPropertyValue(defaultRandomMaxProperty, _ => UpdateSummary());

            if (identifiersProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(identifiersProperty, "Identifiers", _ => UpdateSummary()));
            }

            if (defaultCountProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(defaultCountProperty, "Default Count", _ => UpdateSummary()));
            }

            if (defaultUseRandomProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(defaultUseRandomProperty, "Use Random", _ => UpdateSummary()));
            }

            if (defaultRandomMinProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(defaultRandomMinProperty, "Random Min", _ => UpdateSummary()));
            }

            if (defaultRandomMaxProperty != null)
            {
                content.Add(InspectorUi.Controls.CreatePropertyField(defaultRandomMaxProperty, "Random Max", _ => UpdateSummary()));
            }

            root.Add(card);
        }
    }
}
