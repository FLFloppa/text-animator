using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Parsing.Assets;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Manages persisted user configuration for the Animation Studio window.
    /// </summary>
    internal sealed class AnimationStudioState
    {
        private const string EditorPrefsPrefix = "FLFloppa.TextAnimator.AnimationStudio.";
        private const string RegistryKey = EditorPrefsPrefix + "Registry";
        private const string ParserKey = EditorPrefsPrefix + "Parser";
        private const string SubsystemBundleKey = EditorPrefsPrefix + "SubsystemBundle";
        private const string SubsystemsKey = EditorPrefsPrefix + "Subsystems";
        private const string MarkupKey = EditorPrefsPrefix + "Markup";
        private const string PreviewSceneKey = EditorPrefsPrefix + "PreviewScene";
        private const string AutoRefreshKey = EditorPrefsPrefix + "AutoRefreshPreview";

        private readonly List<TextAnimatorSubsystemAsset> _additionalSubsystems = new List<TextAnimatorSubsystemAsset>();

        /// <summary>
        /// Gets or sets the registry asset used to resolve tag handlers.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public TagHandlerRegistryAsset? Registry { get; set; }

        /// <summary>
        /// Gets or sets the parser asset used to interpret the markup.
        /// </summary>
        public ParserAsset? Parser { get; set; }

        /// <summary>
        /// Gets or sets the subsystem bundle assigned to the preview animator.
        /// </summary>
        public TextAnimatorSubsystemBundleAsset? SubsystemBundle { get; set; }

        /// <summary>
        /// Gets the collection of additional subsystem assets assigned to the preview animator.
        /// </summary>
        public IReadOnlyList<TextAnimatorSubsystemAsset> AdditionalSubsystems => _additionalSubsystems;

        /// <summary>
        /// Gets or sets the current markup text edited by the user.
        /// </summary>
        public string Markup { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scene asset providing the preview setup.
        /// </summary>
        public SceneAsset? PreviewScene { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the preview should rebuild automatically.
        /// </summary>
        public bool AutoRefreshPreview { get; set; } = true;

        /// <summary>
        /// Replaces the additional subsystem list with the specified collection.
        /// </summary>
        /// <param name="subsystems">Subsystems to use for subsequent preview sessions.</param>
        public void SetAdditionalSubsystems(IEnumerable<TextAnimatorSubsystemAsset>? subsystems)
        {
            _additionalSubsystems.Clear();
            if (subsystems == null)
            {
                return;
            }

            foreach (var subsystem in subsystems)
            {
                if (subsystem == null)
                {
                    continue;
                }

                _additionalSubsystems.Add(subsystem);
            }
        }

        /// <summary>
        /// Loads persisted values from <see cref="EditorPrefs"/> and resolves referenced assets.
        /// </summary>
        public void Load()
        {
            Registry = LoadAsset<TagHandlerRegistryAsset>(RegistryKey);
            Parser = LoadAsset<ParserAsset>(ParserKey);
            SubsystemBundle = LoadAsset<TextAnimatorSubsystemBundleAsset>(SubsystemBundleKey);
            PreviewScene = LoadAsset<SceneAsset>(PreviewSceneKey);
            _additionalSubsystems.Clear();

            var serializedSubsystems = EditorPrefs.GetString(SubsystemsKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(serializedSubsystems))
            {
                var guids = serializedSubsystems.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < guids.Length; i++)
                {
                    var subsystem = LoadAssetByGuid<TextAnimatorSubsystemAsset>(guids[i]);
                    if (subsystem == null)
                    {
                        continue;
                    }

                    _additionalSubsystems.Add(subsystem);
                }
            }

            Markup = EditorPrefs.GetString(MarkupKey, string.Empty);
            AutoRefreshPreview = EditorPrefs.GetInt(AutoRefreshKey, 1) != 0;
        }

        /// <summary>
        /// Persists the current state to <see cref="EditorPrefs"/>.
        /// </summary>
        public void Save()
        {
            SaveAsset(RegistryKey, Registry);
            SaveAsset(ParserKey, Parser);
            SaveAsset(SubsystemBundleKey, SubsystemBundle);
            SaveAsset(PreviewSceneKey, PreviewScene);

            var subsystemGuids = new List<string>(_additionalSubsystems.Count);
            for (var i = 0; i < _additionalSubsystems.Count; i++)
            {
                var guid = TryGetAssetGuid(_additionalSubsystems[i]);
                if (string.IsNullOrEmpty(guid))
                {
                    continue;
                }

                subsystemGuids.Add(guid);
            }

            EditorPrefs.SetString(SubsystemsKey, string.Join(";", subsystemGuids));
            EditorPrefs.SetString(MarkupKey, Markup ?? string.Empty);
            EditorPrefs.SetInt(AutoRefreshKey, AutoRefreshPreview ? 1 : 0);
        }

        private static void SaveAsset<TAsset>(string key, TAsset? asset)
            where TAsset : UnityEngine.Object
        {
            var guid = TryGetAssetGuid(asset);
            EditorPrefs.SetString(key, guid ?? string.Empty);
        }

        private static TAsset? LoadAsset<TAsset>(string key)
            where TAsset : UnityEngine.Object
        {
            var guid = EditorPrefs.GetString(key, string.Empty);
            return LoadAssetByGuid<TAsset>(guid);
        }

        private static TAsset? LoadAssetByGuid<TAsset>(string guid)
            where TAsset : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<TAsset>(path);
        }

        private static string? TryGetAssetGuid(UnityEngine.Object? asset)
        {
            if (asset == null)
            {
                return null;
            }

            var success = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long _);
            return success ? guid : null;
        }
    }
}
