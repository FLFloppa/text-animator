using System.Collections.Generic;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Catalog;
using FLFloppa.TextAnimator.Parsing.Assets;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Settings
{
    /// <summary>
    /// Project-scoped configuration for the Animation Studio authoring tools.
    /// </summary>
    [FilePath("ProjectSettings/FLFloppaAnimationStudioSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class AnimationStudioSettings : ScriptableSingleton<AnimationStudioSettings>
    {
        private const string DefaultIconName = "ScriptableObject Icon";

        [SerializeField]
        private Texture2D? _defaultCatalogIcon;

        [SerializeField]
        private AnimationStudioCatalogRegistryAsset? _catalogRegistry;

        [SerializeField]
        private TagHandlerRegistryAsset? _handlerRegistry;

        [SerializeField]
        private ParserAsset? _parser;

        [SerializeField]
        private TextAnimatorSubsystemBundleAsset? _subsystemBundle;

        [SerializeField]
        private List<TextAnimatorSubsystemAsset> _additionalSubsystems = new List<TextAnimatorSubsystemAsset>();

        [SerializeField]
        private SceneAsset? _previewScene;

        [SerializeField]
        private bool _autoRefreshPreview = true;

        /// <summary>
        /// Gets the singleton instance for retrieving settings values.
        /// </summary>
        internal static AnimationStudioSettings Instance => instance;

        /// <summary>
        /// Gets or sets the default icon used when catalog entries do not provide one.
        /// </summary>
        internal Texture2D? DefaultCatalogIcon
        {
            get => _defaultCatalogIcon;
            set
            {
                if (_defaultCatalogIcon == value)
                {
                    return;
                }

                _defaultCatalogIcon = value;
                Save(true);
            }
        }

        /// <summary>
        /// Gets or sets the catalog registry providing items for the catalog panel.
        /// </summary>
        internal AnimationStudioCatalogRegistryAsset? CatalogRegistry
        {
            get => _catalogRegistry;
            set
            {
                if (_catalogRegistry == value)
                {
                    return;
                }

                _catalogRegistry = value;
                Save(true);
            }
        }

        /// <summary>
        /// Resolves the icon that should be displayed for the provided handler.
        /// </summary>
        /// <param name="handler">Handler to resolve.</param>
        /// <returns>Texture representing the handler icon.</returns>
        internal Texture2D ResolveCatalogIcon(Texture2D? preferredTexture, TagHandlerAsset? handler)
        {
            if (preferredTexture != null)
            {
                return preferredTexture;
            }

            var icon = TryResolveObjectIcon(handler);
            return icon ?? GetFallbackIcon();
        }

        /// <summary>
        /// Gets the tag handler registry assigned for workspace usage.
        /// </summary>
        internal TagHandlerRegistryAsset? HandlerRegistry => _handlerRegistry;

        /// <summary>
        /// Gets the parser asset configured for workspace usage.
        /// </summary>
        internal ParserAsset? Parser => _parser;

        /// <summary>
        /// Gets the subsystem bundle configured for preview playback.
        /// </summary>
        internal TextAnimatorSubsystemBundleAsset? SubsystemBundle => _subsystemBundle;

        /// <summary>
        /// Gets the additional subsystem overrides.
        /// </summary>
        internal IReadOnlyList<TextAnimatorSubsystemAsset> AdditionalSubsystems => _additionalSubsystems;

        /// <summary>
        /// Gets the preview scene assigned in project settings.
        /// </summary>
        internal SceneAsset? PreviewScene => _previewScene;

        /// <summary>
        /// Gets a value indicating whether previews should auto-refresh by default.
        /// </summary>
        internal bool AutoRefreshPreview => _autoRefreshPreview;

        private Texture2D GetFallbackIcon()
        {
            if (_defaultCatalogIcon != null)
            {
                return _defaultCatalogIcon;
            }

            var content = EditorGUIUtility.IconContent(DefaultIconName);
            return content.image as Texture2D ?? Texture2D.grayTexture;
        }

        private static Texture2D? TryResolveObjectIcon(Object? target)
        {
            if (target == null)
            {
                return null;
            }

            var content = EditorGUIUtility.ObjectContent(target, target.GetType());
            return content.image as Texture2D;
        }

        [SettingsProvider]
        private static SettingsProvider CreateProjectSettingsProvider()
        {
            return new SettingsProvider("Project/FLFloppa/Animation Studio", SettingsScope.Project)
            {
                label = "Animation Studio",
                guiHandler = _ => DrawSettingsGUI(),
                keywords = new[] { "Animation", "Studio", "Text", "Catalog" }
            };
        }

        private static void DrawSettingsGUI()
        {
            var settings = Instance;

            EditorGUILayout.LabelField("Catalog", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var icon = (Texture2D?)EditorGUILayout.ObjectField(
                new GUIContent("Default Catalog Icon", "Icon used for catalog tiles when a handler does not supply one."),
                settings._defaultCatalogIcon,
                typeof(Texture2D),
                false);
            if (icon != settings._defaultCatalogIcon)
            {
                settings._defaultCatalogIcon = icon;
                settings.Save(true);
            }

            if (settings._defaultCatalogIcon == null)
            {
                EditorGUILayout.HelpBox(
                    "When no icon is assigned, Unity's built-in ScriptableObject icon will be shown.",
                    MessageType.Info);
            }

            var registry = (AnimationStudioCatalogRegistryAsset?)EditorGUILayout.ObjectField(
                new GUIContent("Catalog Registry", "Registry containing catalog items displayed in the Animation Studio catalog window."),
                settings._catalogRegistry,
                typeof(AnimationStudioCatalogRegistryAsset),
                false);
            if (registry != settings._catalogRegistry)
            {
                settings._catalogRegistry = registry;
                settings.Save(true);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(6f);

            EditorGUILayout.LabelField("Workspace", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            var handlerRegistry = (TagHandlerRegistryAsset?)EditorGUILayout.ObjectField(
                new GUIContent("Handler Registry", "Default registry supplying tag handlers to the Animation Studio."),
                settings._handlerRegistry,
                typeof(TagHandlerRegistryAsset),
                false);
            if (handlerRegistry != settings._handlerRegistry)
            {
                settings._handlerRegistry = handlerRegistry;
                settings.Save(true);
            }

            var parser = (ParserAsset?)EditorGUILayout.ObjectField(
                new GUIContent("Parser", "Parser asset used for interpreting markup in the studio."),
                settings._parser,
                typeof(ParserAsset),
                false);
            if (parser != settings._parser)
            {
                settings._parser = parser;
                settings.Save(true);
            }

            var subsystemBundle = (TextAnimatorSubsystemBundleAsset?)EditorGUILayout.ObjectField(
                new GUIContent("Subsystem Bundle", "Primary subsystem bundle applied to the preview animator."),
                settings._subsystemBundle,
                typeof(TextAnimatorSubsystemBundleAsset),
                false);
            if (subsystemBundle != settings._subsystemBundle)
            {
                settings._subsystemBundle = subsystemBundle;
                settings.Save(true);
            }

            var serialized = new SerializedObject(settings);
            serialized.Update();
            var subsystemsProperty = serialized.FindProperty("_additionalSubsystems");
            EditorGUILayout.PropertyField(
                subsystemsProperty,
                new GUIContent("Additional Subsystems", "Optional subsystems appended to the bundle during preview playback."),
                true);
            if (serialized.ApplyModifiedProperties())
            {
                settings.Save(true);
            }

            var previewScene = (SceneAsset?)EditorGUILayout.ObjectField(
                new GUIContent("Preview Scene", "Scene loaded into the Animation Studio preview."),
                settings._previewScene,
                typeof(SceneAsset),
                false);
            if (previewScene != settings._previewScene)
            {
                settings._previewScene = previewScene;
                settings.Save(true);
            }

            var autoRefresh = EditorGUILayout.Toggle(
                new GUIContent("Auto Refresh Preview", "Whether previews should rebuild automatically by default."),
                settings._autoRefreshPreview);
            if (autoRefresh != settings._autoRefreshPreview)
            {
                settings._autoRefreshPreview = autoRefresh;
                settings.Save(true);
            }

            EditorGUI.indentLevel--;
        }
    }
}
