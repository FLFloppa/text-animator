using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Document;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Panels;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Settings;
using FLFloppa.TextAnimator.Parsing;
using FLFloppa.TextAnimator.Parsing.Assets;
using FLFloppa.TextAnimator.Playback;
using FLFloppa.TextAnimator.Parameters;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Primary editor window orchestrating the Animation Studio workspace and auxiliary panels.
    /// </summary>
    internal sealed class AnimationStudioWindow : EditorWindow
    {
        internal const string CatalogDragDataType = "FLFloppa.TextAnimator.AnimationStudio.TagHandlerDrag";

        private readonly AnimationStudioState _state = new AnimationStudioState();
        private readonly List<TextAnimatorSubsystemAsset?> _subsystemEntries = new List<TextAnimatorSubsystemAsset?>();
        private Label? _previewStatusLabel;

        private AnimationStudioWorkspaceCoordinator? _coordinator;
        private AnimationStudioCatalogWindow? _catalogWindow;
        private AnimationStudioMarkupWindow? _markupWindow;
        private AnimationStudioPreviewWindow? _previewWindow;
        private AnimationStudioPlaybackWindow? _playbackWindow;
        private AnimationStudioInspectorWindow? _inspectorWindow;

        private AnimationPlaybackState _playbackState = AnimationPlaybackState.Stopped;
        private bool _isAutoRefreshingPreview = true;
        private bool _previewDirty = true;
        private (int start, int end) _lastExplicitSelectionRange;
        private (int start, int end) _lastObservedMarkupSelection = (-1, -1);
        private bool _isProcessingInspectorReorder;
        private bool _inspectorReorderPending;

        private Scene _loadedPreviewScene;
        private bool _previewSceneLoaded;
        private bool _previewSceneWasLoadedExternally;
        private string? _previewScenePath;
        private GameObject? _previewRootObject;
        private Camera? _previewCamera;
        private TextAnimator.Animator.TextAnimator? _previewAnimator;
        private TMP_Text? _previewText;
        private PlaybackSession? _previewSession;
        private double _lastPreviewUpdate;
        private RenderTexture? _previewRenderTexture;
        private int _previewTextureWidth = 512;
        private int _previewTextureHeight = 512;

        /// <summary>
        /// Opens the Animation Studio window.
        /// </summary>
        [MenuItem("FLFloppa/Text Animator/Animation Studio", priority = 1000)]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimationStudioWindow>();
            window.titleContent = new GUIContent("Animation Studio");
            window.minSize = new Vector2(900f, 600f);
            window.Show();
        }

        private static bool TryFindTagSpanByOccurrence(string markup, string tagName, int occurrenceIndex, out int tagStart, out int tagEnd)
        {
            tagStart = -1;
            tagEnd = -1;

            if (string.IsNullOrEmpty(markup) || string.IsNullOrEmpty(tagName) || occurrenceIndex < 0)
            {
                return false;
            }

            var searchIndex = 0;
            for (var occurrence = 0; occurrence <= occurrenceIndex; occurrence++)
            {
                if (!TryFindNextTagSpan(markup, tagName, searchIndex, out tagStart, out tagEnd))
                {
                    tagStart = -1;
                    tagEnd = -1;
                    return false;
                }

                searchIndex = tagEnd;
            }

            return true;
        }

        private static bool TryFindNextTagSpan(string markup, string tagName, int startIndex, out int tagStart, out int tagEnd)
        {
            tagStart = -1;
            tagEnd = -1;

            if (string.IsNullOrEmpty(markup) || string.IsNullOrEmpty(tagName))
            {
                return false;
            }

            var index = Mathf.Clamp(startIndex, 0, Math.Max(0, markup.Length - 1));
            while (index < markup.Length)
            {
                var openIndex = markup.IndexOf('{', index);
                if (openIndex == -1)
                {
                    break;
                }

                if (openIndex + 1 < markup.Length && markup[openIndex + 1] == '/')
                {
                    index = openIndex + 1;
                    continue;
                }

                var closeIndex = markup.IndexOf('}', openIndex);
                if (closeIndex == -1)
                {
                    break;
                }

                var inner = markup.Substring(openIndex + 1, closeIndex - openIndex - 1).TrimStart();
                if (inner.StartsWith(tagName, StringComparison.OrdinalIgnoreCase))
                {
                    tagStart = openIndex;
                    tagEnd = closeIndex + 1;
                    return true;
                }

                index = closeIndex + 1;
            }

            return false;
        }

        /// <summary>
        /// Resolves a tag handler asset from the configured registry using the supplied tag identifier.
        /// </summary>
        /// <param name="tagName">Tag identifier or declaration to resolve.</param>
        /// <returns>Resolved <see cref="TagHandlerAsset"/> when available; otherwise, <c>null</c>.</returns>
        private TagHandlerAsset? ResolveHandlerForTag(string? tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return null;
            }

            var identifier = NormalizeTagIdentifier(tagName);
            if (string.IsNullOrEmpty(identifier))
            {
                return null;
            }

            var registry = _state.Registry;
            if (registry == null)
            {
                return null;
            }

            var serializedRegistry = new SerializedObject(registry);
            var handlersProperty = serializedRegistry.FindProperty("handlers");
            if (handlersProperty == null)
            {
                return null;
            }

            for (var i = 0; i < handlersProperty.arraySize; i++)
            {
                var element = handlersProperty.GetArrayElementAtIndex(i);
                var handler = element?.objectReferenceValue as TagHandlerAsset;
                if (handler == null)
                {
                    continue;
                }

                var identifiers = handler.TagIdentifiers;
                for (var j = 0; j < identifiers.Count; j++)
                {
                    if (string.Equals(identifiers[j], identifier, StringComparison.OrdinalIgnoreCase))
                    {
                        return handler;
                    }
                }
            }

            return null;
        }

        private static string NormalizeTagIdentifier(string raw)
        {
            var identifier = raw.Trim();
            if (identifier.Length == 0)
            {
                return string.Empty;
            }

            if (identifier[0] == '{')
            {
                identifier = identifier.Substring(1).TrimStart();
            }

            if (identifier.EndsWith("}", StringComparison.Ordinal))
            {
                identifier = identifier.Substring(0, identifier.Length - 1).TrimEnd();
            }

            if (identifier.Length > 0 && identifier[0] == '/')
            {
                identifier = identifier.Substring(1).TrimStart();
            }

            return identifier;
        }

        /// <summary>
        /// Extracts parameter definition assets serialized on the provided handler.
        /// </summary>
        /// <param name="handler">Handler to inspect.</param>
        /// <returns>List pairing serialized field names with their parameter definitions.</returns>
        private static List<(string fieldName, BaseParameterDefinitionAsset definition)> CollectParameterDefinitions(TagHandlerAsset handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var results = new List<(string, BaseParameterDefinitionAsset)>();
            var serializedHandler = new SerializedObject(handler);
            serializedHandler.Update();

            var iterator = serializedHandler.GetIterator();
            if (!iterator.NextVisible(true))
            {
                return results;
            }

            do
            {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }

                if (iterator.name == "m_Script")
                {
                    continue;
                }

                if (iterator.objectReferenceValue is BaseParameterDefinitionAsset parameter)
                {
                    results.Add((iterator.displayName ?? iterator.name, parameter));
                }
            }
            while (iterator.NextVisible(false));

            return results;
        }

        /// <summary>
        /// Creates bindings between handler parameter definitions and markup attribute values.
        /// </summary>
        private static List<InspectorParameterBinding> BuildParameterBindings(
            TagHandlerAsset handler,
            IReadOnlyDictionary<string, string> attributes,
            string tagName,
            int occurrenceIndex)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            attributes ??= new Dictionary<string, string>();

            var definitions = CollectParameterDefinitions(handler);
            var bindings = new List<InspectorParameterBinding>(definitions.Count);

            for (var i = 0; i < definitions.Count; i++)
            {
                var (label, definitionAsset) = definitions[i];
                if (definitionAsset == null)
                {
                    continue;
                }

                IParameterDefinition runtimeDefinition;
                try
                {
                    runtimeDefinition = definitionAsset.BuildRuntimeDefinition();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Animation Studio] Failed to build parameter definition '{label}' for handler '{handler.name}': {exception.Message}");
                    continue;
                }

                var aliases = runtimeDefinition.Identifiers ?? Array.Empty<string>();
                var aliasArray = new string[aliases.Count];
                for (var j = 0; j < aliases.Count; j++)
                {
                    aliasArray[j] = aliases[j];
                }

                var primaryIdentifier = aliasArray.Length > 0 && !string.IsNullOrEmpty(aliasArray[0])
                    ? aliasArray[0]
                    : definitionAsset.name;

                var matchedIdentifier = string.Empty;
                var rawValue = string.Empty;
                var isPresent = false;

                for (var j = 0; j < aliasArray.Length; j++)
                {
                    if (TryGetAttributeValue(attributes, aliasArray[j], out var candidate))
                    {
                        matchedIdentifier = aliasArray[j];
                        rawValue = candidate ?? string.Empty;
                        isPresent = true;
                        break;
                    }
                }

                var hasContent = isPresent && !string.IsNullOrEmpty(rawValue);
                var defaultValue = GetDefaultValueString(runtimeDefinition);

                bindings.Add(new InspectorParameterBinding(
                    tagName,
                    occurrenceIndex,
                    label,
                    definitionAsset,
                    aliasArray,
                    primaryIdentifier,
                    matchedIdentifier,
                    rawValue,
                    isPresent,
                    hasContent,
                    defaultValue));
            }

            return bindings;
        }

        private static bool TryGetAttributeValue(
            IReadOnlyDictionary<string, string> attributes,
            string key,
            out string value)
        {
            if (attributes.TryGetValue(key, out value))
            {
                return true;
            }

            foreach (var pair in attributes)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        private static string GetDefaultValueString(IParameterDefinition definition)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            var property = definition.GetType().GetProperty("DefaultValue", BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                return string.Empty;
            }

            var value = property.GetValue(definition);
            return value != null ? value.ToString() ?? string.Empty : string.Empty;
        }

        private static string BuildParameterLabel(InspectorParameterBinding binding)
        {
            if (binding.IsPresent)
            {
                if (binding.HasContent)
                {
                    return $"Markup '{binding.MatchedIdentifier}' = {binding.Value}";
                }

                return $"Markup '{binding.MatchedIdentifier}' is present but empty.";
            }

            if (string.IsNullOrEmpty(binding.DefaultValue))
            {
                return "Not specified in markup.";
            }

            return $"Not specified; using default value {binding.DefaultValue}.";
        }

        private static string BuildTagContentPreview(TagNode node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            var buffer = new StringBuilder();
            AppendTagContent(node, buffer);
            var normalized = NormalizeWhitespace(buffer.ToString());

            if (string.IsNullOrEmpty(normalized))
            {
                return "(no enclosed text)";
            }

            const int maxLength = 80;
            if (normalized.Length > maxLength)
            {
                normalized = normalized.Substring(0, maxLength).TrimEnd() + "…";
            }

            return $"\"{normalized}\"";
        }

        private static void AppendTagContent(IDocumentNode node, StringBuilder buffer)
        {
            if (node == null || buffer == null)
            {
                return;
            }

            switch (node)
            {
                case TextNode textNode:
                    buffer.Append(textNode.Text);
                    break;
                case TagNode tagNode:
                    var children = tagNode.Children;
                    for (var i = 0; i < children.Count; i++)
                    {
                        AppendTagContent(children[i], buffer);
                    }

                    break;
            }
        }

        private static string NormalizeWhitespace(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            var previousWasWhitespace = false;
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (char.IsWhiteSpace(ch))
                {
                    if (previousWasWhitespace)
                    {
                        continue;
                    }

                    builder.Append(' ');
                    previousWasWhitespace = true;
                }
                else
                {
                    builder.Append(ch);
                    previousWasWhitespace = false;
                }
            }

            return builder.ToString().Trim();
        }

        /// <summary>
        /// Gets the current workspace state instance.
        /// </summary>
        internal AnimationStudioState State => _state;

        /// <summary>
        /// Gets the markup string currently loaded in the studio.
        /// </summary>
        internal string CurrentMarkup => _state.Markup ?? string.Empty;

        /// <summary>
        /// Gets a value indicating whether preview rebuilds occur automatically.
        /// </summary>
        internal bool IsPreviewAutoRefreshing => _isAutoRefreshingPreview;

        private void OnEnable()
        {
            _state.Load();
            LoadSettingsIntoState();
            _coordinator = new AnimationStudioWorkspaceCoordinator(this);

            ResetPreviewSceneState();

            ConstructUi();
            OpenAuxiliaryWindows();
            EnsurePreviewHandler();
        }

        private void OnDisable()
        {
            PersistState();
            DisposePreviewResources();
            UnloadPreviewScene();
            EditorApplication.update -= OnEditorUpdate;
            CloseAuxiliaryWindows();
        }

        /// <summary>
        /// Sets the markup text, optionally triggering a preview rebuild.
        /// </summary>
        /// <param name="markup">Markup string to apply.</param>
        /// <param name="requestPreviewRestart">Indicates whether the preview should be refreshed.</param>
        internal void SetMarkup(string markup, bool requestPreviewRestart)
        {
            _state.Markup = markup ?? string.Empty;
            PersistState();

            if (_markupWindow != null)
            {
                _markupWindow.SetMarkupSilently(_state.Markup);
            }

            if (requestPreviewRestart && _isAutoRefreshingPreview)
            {
                RequestPreviewRebuild();
            }

            UpdateInspectorSelection();
        }

        /// <summary>
        /// Adjusts the auto-refresh mode for the preview.
        /// </summary>
        /// <param name="value">New auto-refresh value.</param>
        internal void SetPreviewAutoRefreshing(bool value)
        {
            if (_isAutoRefreshingPreview == value)
            {
                return;
            }

            _isAutoRefreshingPreview = value;
            _state.AutoRefreshPreview = value;
            PersistState();

            if (_isAutoRefreshingPreview && _previewDirty)
            {
                RebuildPreview();
            }
        }

        private void DisposePreviewResources()
        {
            EditorApplication.update -= OnEditorUpdate;

            if (_previewSession != null)
            {
                _previewSession = null;
            }

            if (_previewAnimator != null)
            {
                try
                {
                    _previewAnimator.Stop();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Animation Studio] Failed to stop preview animator during cleanup: {ex.Message}");
                }
            }

            _previewAnimator = null;
            _previewText = null;
            ReleaseRenderTexture();
        }

        /// <summary>
        /// Requests an explicit preview rebuild.
        /// </summary>
        internal void RequestPreviewRebuild()
        {
            _previewDirty = true;
            if (_isAutoRefreshingPreview)
            {
                RebuildPreview();
            }
            else
            {
                UpdatePreviewStatus("Preview refresh pending (auto-refresh disabled)");
            }
        }

        /// <summary>
        /// Refreshes the effect catalog window.
        /// </summary>
        internal void RefreshCatalog()
        {
            _catalogWindow?.RefreshCatalog();
        }

        /// <summary>
        /// Starts or resumes the preview playback session.
        /// </summary>
        internal void PlayPreview()
        {
            if (!EnsurePreviewReady())
            {
                _playbackState = AnimationPlaybackState.Stopped;
                UpdatePlaybackTransport();
                return;
            }

            if (_playbackState == AnimationPlaybackState.Paused && _previewSession != null)
            {
                ResumePreviewPlayback();
                return;
            }

            RestartPreviewPlayback();
        }

        /// <summary>
        /// Pauses the preview playback session.
        /// </summary>
        internal void PausePreview()
        {
            if (_playbackState != AnimationPlaybackState.Playing || _previewSession == null)
            {
                return;
            }

            _playbackState = AnimationPlaybackState.Paused;
            UpdatePlaybackTransport();
            PausePreviewPlayback();
        }

        /// <summary>
        /// Stops the preview playback session.
        /// </summary>
        internal void StopPreview()
        {
            if (_playbackState == AnimationPlaybackState.Stopped && _previewSession == null)
            {
                return;
            }

            StopPreviewPlayback();
            _playbackState = AnimationPlaybackState.Stopped;
            UpdatePlaybackTransport();
            DisplayStaticMarkupSnapshot();
            UpdatePreviewStatus("Preview stopped.");
        }

        private void ConstructUi()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.flexGrow = 1f;

            rootVisualElement.Add(CreatePreviewStatusBar());
        }

        private VisualElement CreatePreviewStatusBar()
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 10f,
                    paddingRight = 10f,
                    paddingTop = 6f,
                    paddingBottom = 6f,
                    flexShrink = 0f
                }
            };
            var previewRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 6f
                }
            };

            _previewStatusLabel = new Label("Preview rebuild required")
            {
                style =
                {
                    flexGrow = 1f,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            previewRow.Add(_previewStatusLabel);

            var manualRefreshButton = InspectorUi.Controls.CreateActionButton("Refresh", RebuildPreview);
            manualRefreshButton.style.marginLeft = 6f;
            previewRow.Add(manualRefreshButton);

            container.Add(previewRow);
            return container;
        }

        private void OpenAuxiliaryWindows()
        {
            if (_coordinator == null)
            {
                return;
            }

            _catalogWindow = AnimationStudioCatalogWindow.Open(_coordinator);
            _catalogWindow.HandlerInvoked += OnCatalogHandlerInvoked;

            _markupWindow = AnimationStudioMarkupWindow.Open(_coordinator, TryResolveDraggedHandler, OnMarkupSelectionChanged);
            _markupWindow.RegisterSelectionChanged(OnMarkupSelectionChanged);
            _markupWindow.UpdateMarkup(CurrentMarkup);

            _previewWindow = AnimationStudioPreviewWindow.Open(_coordinator);
            _previewWindow.SetPreviewGuiHandler(OnPreviewGui);

            _playbackWindow = AnimationStudioPlaybackWindow.Open(_coordinator);
            _playbackWindow.SyncAutoRefresh();
            UpdatePlaybackTransport();

            _inspectorWindow = AnimationStudioInspectorWindow.Open(BuildInspectorContent);
            UpdateInspectorSelection();

            RefreshCatalog();
        }

        private void CloseAuxiliaryWindows()
        {
            _catalogWindow?.Close();
            if (_catalogWindow != null)
            {
                _catalogWindow.HandlerInvoked -= OnCatalogHandlerInvoked;
            }
            _catalogWindow = null;

            _markupWindow?.Close();
            _markupWindow = null;

            _previewWindow?.Close();
            _previewWindow = null;

            _playbackWindow?.Close();
            _playbackWindow = null;

            _inspectorWindow?.Close();
            _inspectorWindow = null;
        }

        private void PersistState()
        {
            _state.Save();
        }

        private TagHandlerAsset? TryResolveDraggedHandler()
        {
            if (DragAndDrop.GetGenericData(CatalogDragDataType) is TagHandlerAsset direct)
            {
                return direct;
            }

            var references = DragAndDrop.objectReferences;
            if (references == null)
            {
                return null;
            }

            for (var i = 0; i < references.Length; i++)
            {
                if (references[i] is TagHandlerAsset asset)
                {
                    return asset;
                }
            }

            return null;
        }

        private void LoadSettingsIntoState()
        {
            var settings = AnimationStudioSettings.Instance;

            _state.Registry = settings.HandlerRegistry;
            _state.Parser = settings.Parser;
            _state.SubsystemBundle = settings.SubsystemBundle;
            _state.SetAdditionalSubsystems(settings.AdditionalSubsystems);
            _state.PreviewScene = settings.PreviewScene;
            _state.AutoRefreshPreview = settings.AutoRefreshPreview;
            _isAutoRefreshingPreview = settings.AutoRefreshPreview;

            _subsystemEntries.Clear();
            var additional = settings.AdditionalSubsystems;
            for (var i = 0; i < additional.Count; i++)
            {
                _subsystemEntries.Add(additional[i]);
            }
        }

        private void OnMarkupSelectionChanged()
        {
            if (_markupWindow == null)
            {
                UpdateInspectorSelection();
                return;
            }

            var range = _markupWindow.GetSelectionRange();
            if (range.start != range.end)
            {
                _lastExplicitSelectionRange = range;
            }

            if (range == _lastObservedMarkupSelection)
            {
                return;
            }

            _lastObservedMarkupSelection = range;
            UpdateInspectorSelection();
        }

        private void CaptureCurrentSelectionRange()
        {
            if (_markupWindow == null)
            {
                return;
            }

            var range = _markupWindow.GetCurrentSelectionRange();
            if (range.start != range.end)
            {
                _lastExplicitSelectionRange = range;
                return;
            }

            range = _markupWindow.GetSelectionRange();
            if (range.start != range.end)
            {
                _lastExplicitSelectionRange = range;
                return;
            }

            range = _markupWindow.GetLastNonEmptySelectionRange();
            if (range.start != range.end)
            {
                _lastExplicitSelectionRange = range;
            }
        }

        private void UpdateInspectorSelection()
        {
            _inspectorWindow?.Refresh();
        }

        private void OnCatalogHandlerInvoked(TagHandlerAsset handler)
        {
            if (_markupWindow == null)
            {
                return;
            }

            CaptureCurrentSelectionRange();
            var range = _lastExplicitSelectionRange;
            if (range.start == range.end)
            {
                range = _markupWindow.GetSelectionRange();
            }

            if (range.start == range.end)
            {
                range = _markupWindow.GetLastNonEmptySelectionRange();
            }

            if (range.start == range.end)
            {
                return;
            }

            _markupWindow.ApplyHandlerToRange(handler, range.start, range.end);
            _markupWindow.FocusMarkup();
        }

        private void BuildInspectorContent(VisualElement container)
        {
            container.Clear();

            var markup = CurrentMarkup ?? string.Empty;
            if (markup.Length == 0)
            {
                container.Add(new HelpBox("The markup text is empty. Enter text to inspect tag metadata.", HelpBoxMessageType.Info));
                return;
            }

            var range = _markupWindow != null ? _markupWindow.GetSelectionRange() : (start: 0, end: 0);
            var start = range.start;
            var end = range.end;
            if (start == end && _markupWindow != null)
            {
                var fallback = _markupWindow.GetLastNonEmptySelectionRange();
                start = fallback.start;
                end = fallback.end;
            }

            if (start == end && _lastExplicitSelectionRange.start != _lastExplicitSelectionRange.end)
            {
                start = _lastExplicitSelectionRange.start;
                end = _lastExplicitSelectionRange.end;
            }

            var selectionLength = Math.Max(0, end - start);
            var normalizedStart = Math.Max(0, Math.Min(Math.Min(start, end), markup.Length));
            var normalizedEnd = Math.Max(0, Math.Min(Math.Max(start, end), markup.Length));

            var selectionHeader = InspectorUi.Layout.CreateHeader("Selection", selectionLength > 0
                ? $"Range: {start.ToString(CultureInfo.InvariantCulture)} - {end.ToString(CultureInfo.InvariantCulture)}"
                : "Place the caret within the markup to inspect tags.");
            container.Add(selectionHeader);

            if (selectionLength > 0)
            {
                var snippetLength = Math.Min(64, selectionLength);
                var snippet = markup.Substring(normalizedStart, Math.Min(snippetLength, normalizedEnd - normalizedStart));

                container.Add(new Label($"Selection Preview: {snippet}")
                {
                    style =
                    {
                        whiteSpace = WhiteSpace.Normal
                    }
                });

                var nearestTag = GetNearestTagDeclaration(markup, start);
                if (!string.IsNullOrEmpty(nearestTag))
                {
                    container.Add(new Label($"Nearest Tag: {nearestTag}")
                    {
                        style =
                        {
                            whiteSpace = WhiteSpace.Normal
                        }
                    });
                }
            }
            else
            {
                container.Add(new HelpBox("No active selection. Click inside a tag to view details.", HelpBoxMessageType.Info));
            }

            if (!TryBuildDocumentTree(markup, out var root, out var errorMessage))
            {
                container.Add(new HelpBox($"Failed to parse markup: {errorMessage}", HelpBoxMessageType.Error));
                return;
            }

            var summaries = new List<TagSummary>();
            CollectTagSummaries(root, 0, summaries);

            if (summaries.Count == 0)
            {
                container.Add(new HelpBox("No tags detected in the current markup.", HelpBoxMessageType.Info));
                return;
            }

            container.Add(InspectorUi.Layout.CreateHeader("Tags", $"Total: {summaries.Count.ToString(CultureInfo.InvariantCulture)}"));

            var list = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };

            var occurrenceLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var tagRanges = CalculateTagRanges(markup);
            var rangeLookup = BuildTagRangeLookup(tagRanges);
            var entries = new List<TagInspectorEntry>();

            foreach (var summary in summaries)
            {
                var tagName = summary.Node.TagName;
                var occurrenceIndex = occurrenceLookup.TryGetValue(tagName, out var occurrence)
                    ? occurrence
                    : 0;
                occurrenceLookup[tagName] = occurrenceIndex + 1;

                if (!TryGetTagRange(rangeLookup, tagName, occurrenceIndex, out var tagRange))
                {
                    continue;
                }

                if (!TagRangeContainsSelection(tagRange, normalizedStart, normalizedEnd))
                {
                    continue;
                }

                var handler = ResolveHandlerForTag(tagName);
                var openingMarkup = markup.Substring(tagRange.OpeningStart, tagRange.OpeningEnd - tagRange.OpeningStart);
                var closingMarkup = markup.Substring(tagRange.ClosingStart, tagRange.ClosingEnd - tagRange.ClosingStart);

                entries.Add(new TagInspectorEntry(summary, tagRange, handler, occurrenceIndex, openingMarkup, closingMarkup));
            }

            if (entries.Count == 0)
            {
                container.Add(new HelpBox("No tags encompass the current selection.", HelpBoxMessageType.Info));
                return;
            }

            var listView = CreateTagInspectorListView(entries, normalizedStart, normalizedEnd);
            container.Add(listView);
        }

        private void AppendHandlerAssetProperties(VisualElement container, TagHandlerAsset handler)
        {
            if (container == null || handler == null)
            {
                return;
            }

            var serializedHandler = new SerializedObject(handler);
            serializedHandler.Update();

            var propertyPaths = CollectHandlerValuePropertyPaths(serializedHandler);
            if (propertyPaths.Count == 0)
            {
                return;
            }

            var header = new Label("Handler Asset Values")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 6f,
                    marginBottom = 4f
                }
            };
            container.Add(header);

            for (var i = 0; i < propertyPaths.Count; i++)
            {
                var property = serializedHandler.FindProperty(propertyPaths[i]);
                if (property == null)
                {
                    continue;
                }

                var field = new PropertyField(property)
                {
                    style =
                    {
                        marginBottom = 4f
                    }
                };
                field.Bind(serializedHandler);
                container.Add(field);
            }
        }

        private static List<string> CollectHandlerValuePropertyPaths(SerializedObject serializedHandler)
        {
            var results = new List<string>();
            if (serializedHandler == null)
            {
                return results;
            }

            var iterator = serializedHandler.GetIterator();
            if (!iterator.NextVisible(true))
            {
                return results;
            }

            var enterChildren = false;
            do
            {
                if (ShouldDisplayHandlerProperty(iterator))
                {
                    results.Add(iterator.propertyPath);
                }

                enterChildren = iterator.hasChildren && iterator.propertyType == SerializedPropertyType.Generic;
            }
            while (iterator.NextVisible(enterChildren));

            return results;
        }

        private static bool ShouldDisplayHandlerProperty(SerializedProperty property)
        {
            if (property == null)
            {
                return false;
            }

            if (property.propertyPath == "m_Script")
            {
                return false;
            }

            if (property.propertyPath.StartsWith("tagIdentifiers", StringComparison.Ordinal))
            {
                return false;
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue is BaseParameterDefinitionAsset)
                {
                    return false;
                }

                var typeName = property.type ?? string.Empty;
                if (typeName.IndexOf("ParameterDefinitionAsset", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TagRangeContainsSelection(TagRange range, int selectionStart, int selectionEnd)
        {
            if (!range.IsValid)
            {
                return false;
            }

            if (selectionEnd < selectionStart)
            {
                (selectionStart, selectionEnd) = (selectionEnd, selectionStart);
            }

            if (selectionStart == selectionEnd)
            {
                return selectionStart >= range.ContentStart && selectionStart < range.ContentEnd;
            }

            return selectionStart >= range.ContentStart && selectionEnd <= range.ContentEnd;
        }

        private static Dictionary<string, List<TagRange>> BuildTagRangeLookup(List<TagRange> ranges)
        {
            var lookup = new Dictionary<string, List<TagRange>>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];
                if (!range.IsValid)
                {
                    continue;
                }

                if (!lookup.TryGetValue(range.TagName, out var list))
                {
                    list = new List<TagRange>();
                    lookup.Add(range.TagName, list);
                }

                while (list.Count <= range.OccurrenceIndex)
                {
                    list.Add(default);
                }

                list[range.OccurrenceIndex] = range;
            }

            return lookup;
        }

        private static bool TryGetTagRange(Dictionary<string, List<TagRange>> lookup, string tagName, int occurrenceIndex, out TagRange range)
        {
            range = default;
            if (!lookup.TryGetValue(tagName, out var list))
            {
                return false;
            }

            if (occurrenceIndex < 0 || occurrenceIndex >= list.Count)
            {
                return false;
            }

            range = list[occurrenceIndex];
            return range.IsValid;
        }

        private static List<TagRange> CalculateTagRanges(string markup)
        {
            var ranges = new List<TagRange>();
            if (string.IsNullOrEmpty(markup))
            {
                return ranges;
            }

            var occurrenceLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<TagOpenFrame>();
            var index = 0;

            while (index < markup.Length)
            {
                var current = markup[index];
                if (current != '{')
                {
                    index++;
                    continue;
                }

                var closeIndex = markup.IndexOf('}', index);
                if (closeIndex == -1)
                {
                    break;
                }

                var inner = markup.Substring(index + 1, closeIndex - index - 1).Trim();
                if (inner.Length == 0)
                {
                    index = closeIndex + 1;
                    continue;
                }

                if (inner[0] == '/')
                {
                    var closingName = ExtractTagIdentifier(inner.Substring(1).TrimStart());
                    if (!string.IsNullOrEmpty(closingName) && TryPopMatchingFrame(stack, closingName, out var frame))
                    {
                        ranges.Add(new TagRange(
                            frame.TagName,
                            frame.OccurrenceIndex,
                            frame.OpeningStart,
                            frame.OpeningEnd,
                            frame.ContentStart,
                            index,
                            index,
                            closeIndex + 1));
                    }

                    index = closeIndex + 1;
                    continue;
                }

                var tagName = ExtractTagIdentifier(inner);
                if (string.IsNullOrEmpty(tagName))
                {
                    index = closeIndex + 1;
                    continue;
                }

                var occurrence = occurrenceLookup.TryGetValue(tagName, out var count) ? count : 0;
                occurrenceLookup[tagName] = occurrence + 1;
                stack.Push(new TagOpenFrame(tagName, occurrence, index, closeIndex + 1, closeIndex + 1));
                index = closeIndex + 1;
            }

            return ranges;
        }

        private static string ExtractTagIdentifier(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var length = raw.Length;
            var index = 0;
            while (index < length && !char.IsWhiteSpace(raw[index]))
            {
                index++;
            }

            return raw.Substring(0, index);
        }

        private static bool TryPopMatchingFrame(Stack<TagOpenFrame> stack, string tagName, out TagOpenFrame frame)
        {
            frame = default;
            if (stack.Count == 0)
            {
                return false;
            }

            var top = stack.Peek();
            if (!string.Equals(top.TagName, tagName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            frame = stack.Pop();
            return true;
        }

        private ListView CreateTagInspectorListView(List<TagInspectorEntry> entries, int selectionStart, int selectionEnd)
        {
            var listView = new ListView(entries, itemHeight: -1, MakeInspectorListItem, (element, index) =>
            {
                if (index < 0 || index >= entries.Count)
                {
                    element.Clear();
                    return;
                }

                PopulateTagCard(element, entries[index]);
            })
            {
                selectionType = SelectionType.None,
                reorderable = true
            };

            listView.reorderMode = ListViewReorderMode.Animated;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
            listView.style.flexGrow = 1f;
            listView.style.flexDirection = FlexDirection.Column;

            listView.itemIndexChanged += (_, __) => ScheduleInspectorTagReorder(entries, selectionStart, selectionEnd);

            return listView;
        }

        private VisualElement MakeInspectorListItem()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 6f
                }
            };
        }

        private void PopulateTagCard(VisualElement root, TagInspectorEntry entry)
        {
            root.Clear();

            var summary = entry.Summary;
            var tagName = entry.Range.TagName;
            var description = $"Depth: {summary.Depth.ToString(CultureInfo.InvariantCulture)} | Children: {summary.Node.Children.Count}";
            var contentPreview = BuildTagContentPreview(summary.Node);
            var title = string.IsNullOrEmpty(contentPreview)
                ? $"Tag: {tagName}"
                : $"Tag: {tagName} — {contentPreview}";

            var card = InspectorUi.Cards.Create(title, out var body, description);
            card.style.marginBottom = 6f;

            var toolbar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    marginBottom = 4f
                }
            };

            var removeButton = InspectorUi.Controls.CreateActionButton("Remove", () => RemoveTag(entry.Range));
            removeButton.style.minWidth = 72f;
            toolbar.Add(removeButton);
            body.Add(toolbar);

            if (entry.Handler != null)
            {
                var handler = entry.Handler;
                var parameterBindings = BuildParameterBindings(handler, summary.Node.Attributes, tagName, entry.OccurrenceIndex);
                if (parameterBindings.Count == 0)
                {
                    body.Add(new Label("Handler defines no parameters.")
                    {
                        style =
                        {
                            color = new Color(0.65f, 0.65f, 0.65f, 1f)
                        }
                    });
                }
                else
                {
                    var matchedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var binding in parameterBindings)
                    {
                        if (binding.IsPresent && !string.IsNullOrEmpty(binding.MatchedIdentifier))
                        {
                            matchedAttributes.Add(binding.MatchedIdentifier);
                        }

                        var control = CreateParameterControl(handler, binding);
                        body.Add(control);
                    }

                    foreach (var attribute in summary.Node.Attributes)
                    {
                        if (matchedAttributes.Contains(attribute.Key))
                        {
                            continue;
                        }

                        body.Add(new Label($"Unmapped attribute {attribute.Key}: {attribute.Value}")
                        {
                            style =
                            {
                                color = new Color(0.65f, 0.65f, 0.65f, 1f),
                                whiteSpace = WhiteSpace.Normal
                            }
                        });
                    }
                }

                AppendHandlerAssetProperties(body, handler);
            }
            else if (summary.Node.Attributes.Count == 0)
            {
                body.Add(new Label("No attributes defined.")
                {
                    style =
                    {
                        color = new Color(0.65f, 0.65f, 0.65f, 1f)
                    }
                });
            }
            else
            {
                foreach (var attribute in summary.Node.Attributes)
                {
                    body.Add(new Label($"{attribute.Key}: {attribute.Value}")
                    {
                        style =
                        {
                            whiteSpace = WhiteSpace.Normal
                        }
                    });
                }
            }

            root.Add(card);
        }

        private void HandleInspectorTagReorder(List<TagInspectorEntry> entries, int selectionStart, int selectionEnd)
        {
            if (_isProcessingInspectorReorder)
            {
                return;
            }

            if (entries == null || entries.Count <= 1)
            {
                return;
            }

            var markup = CurrentMarkup ?? string.Empty;
            if (string.IsNullOrEmpty(markup))
            {
                return;
            }

            ApplyTagReorder(markup, entries, selectionStart, selectionEnd);
        }

        private void ScheduleInspectorTagReorder(List<TagInspectorEntry> entries, int selectionStart, int selectionEnd)
        {
            if (_isProcessingInspectorReorder || _inspectorReorderPending)
            {
                return;
            }

            _inspectorReorderPending = true;
            EditorApplication.delayCall += () =>
            {
                _inspectorReorderPending = false;
                HandleInspectorTagReorder(entries, selectionStart, selectionEnd);
            };
        }

        private void ApplyTagReorder(string markup, List<TagInspectorEntry> orderedEntries, int selectionStart, int selectionEnd)
        {
            if (orderedEntries.Count == 0)
            {
                return;
            }

            var sortedByOpening = orderedEntries.OrderBy(e => e.Range.OpeningStart).ToList();
            var sortedByClosing = orderedEntries.OrderBy(e => e.Range.ClosingStart).ToList();
            var reversedOrder = orderedEntries.AsEnumerable().Reverse().ToList();

            var replacements = new List<TagReplacement>(orderedEntries.Count * 2);

            for (var i = 0; i < sortedByOpening.Count; i++)
            {
                var positionEntry = sortedByOpening[i];
                var replacementEntry = orderedEntries[i];
                replacements.Add(new TagReplacement(
                    positionEntry.Range.OpeningStart,
                    positionEntry.Range.OpeningEnd - positionEntry.Range.OpeningStart,
                    replacementEntry.OpeningMarkup));
            }

            for (var i = 0; i < sortedByClosing.Count; i++)
            {
                var positionEntry = sortedByClosing[i];
                var replacementEntry = reversedOrder[i];
                replacements.Add(new TagReplacement(
                    positionEntry.Range.ClosingStart,
                    positionEntry.Range.ClosingEnd - positionEntry.Range.ClosingStart,
                    replacementEntry.ClosingMarkup));
            }

            replacements.Sort((a, b) => a.Start.CompareTo(b.Start));

            var builder = new StringBuilder(markup.Length + replacements.Sum(r => r.Replacement.Length - r.Length));
            var index = 0;
            var deltaBeforeSelection = 0;
            var deltaWithinSelection = 0;

            for (var i = 0; i < replacements.Count; i++)
            {
                var replacement = replacements[i];
                if (replacement.Start < index)
                {
                    continue;
                }

                var unchangedLength = replacement.Start - index;
                if (unchangedLength > 0)
                {
                    builder.Append(markup, index, unchangedLength);
                    index += unchangedLength;
                }

                builder.Append(replacement.Replacement);
                index += replacement.Length;

                var delta = replacement.Replacement.Length - replacement.Length;
                if (replacement.Start < selectionStart)
                {
                    deltaBeforeSelection += delta;
                }
                else if (replacement.Start < selectionEnd)
                {
                    deltaWithinSelection += delta;
                }
            }

            if (index < markup.Length)
            {
                builder.Append(markup, index, markup.Length - index);
            }

            var updatedMarkup = builder.ToString();

            var adjustedSelectionStart = selectionStart + deltaBeforeSelection;
            var adjustedSelectionEnd = selectionEnd + deltaBeforeSelection + deltaWithinSelection;

            if (adjustedSelectionStart < 0)
            {
                adjustedSelectionStart = 0;
            }

            if (adjustedSelectionEnd < adjustedSelectionStart)
            {
                adjustedSelectionEnd = adjustedSelectionStart;
            }

            try
            {
                _isProcessingInspectorReorder = true;
                _lastExplicitSelectionRange = (adjustedSelectionStart, adjustedSelectionEnd);
                _lastObservedMarkupSelection = (-1, -1);
                _coordinator?.UpdateMarkup(updatedMarkup, true);
                ApplyMarkupSelectionAfterDelay(adjustedSelectionStart, adjustedSelectionEnd);
            }
            finally
            {
                _isProcessingInspectorReorder = false;
            }
        }

        private void ApplyMarkupSelectionAfterDelay(int selectionStart, int selectionEnd)
        {
            EditorApplication.delayCall += () =>
            {
                if (_markupWindow == null)
                {
                    return;
                }

                var markup = CurrentMarkup ?? string.Empty;
                var clampedStart = Mathf.Clamp(selectionStart, 0, Math.Max(0, markup.Length));
                var clampedEnd = Mathf.Clamp(selectionEnd, 0, Math.Max(0, markup.Length));
                _markupWindow.ApplySelection(clampedStart, clampedEnd);
                _markupWindow.FocusMarkup();
            };
        }

        private void RemoveTag(TagRange range)
        {
            if (!range.IsValid)
            {
                return;
            }

            var currentMarkup = CurrentMarkup ?? string.Empty;
            if (string.IsNullOrEmpty(currentMarkup))
            {
                return;
            }

            if (range.OpeningStart < 0 || range.OpeningEnd > currentMarkup.Length ||
                range.ClosingStart < 0 || range.ClosingEnd > currentMarkup.Length)
            {
                return;
            }

            var withoutClosing = currentMarkup.Remove(range.ClosingStart, range.ClosingEnd - range.ClosingStart);
            var withoutTag = withoutClosing.Remove(range.OpeningStart, range.OpeningEnd - range.OpeningStart);

            var contentLength = Math.Max(0, range.ContentEnd - range.ContentStart);
            var selectionStart = range.OpeningStart;
            var selectionEnd = selectionStart + contentLength;

            _lastExplicitSelectionRange = (selectionStart, selectionEnd);

            _coordinator?.UpdateMarkup(withoutTag, true);

            EditorApplication.delayCall += () =>
            {
                if (_markupWindow == null)
                {
                    return;
                }

                var clampedStart = Mathf.Clamp(selectionStart, 0, Math.Max(0, withoutTag.Length));
                var clampedEnd = Mathf.Clamp(selectionEnd, 0, Math.Max(0, withoutTag.Length));
                _markupWindow.ApplySelection(clampedStart, clampedEnd);
                _markupWindow.FocusMarkup();
                UpdateInspectorSelection();
            };
        }

        private VisualElement CreateParameterControl(TagHandlerAsset handler, InspectorParameterBinding binding)
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Stretch,
                    paddingLeft = 4f,
                    paddingRight = 4f,
                    paddingTop = 4f,
                    paddingBottom = 4f
                }
            };

            var headerRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.SpaceBetween,
                    marginBottom = 2f
                }
            };

            var titleLabel = new Label(binding.Label)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 12f
                }
            };
            headerRow.Add(titleLabel);

            var identifierLabel = new Label(binding.PrimaryIdentifier)
            {
                style =
                {
                    color = new Color(0.55f, 0.55f, 0.55f, 1f),
                    fontSize = 11f
                }
            };
            headerRow.Add(identifierLabel);

            container.Add(headerRow);

            var control = CreateValueControl(handler, binding);
            container.Add(control);

            var statusLabel = new Label(BuildParameterLabel(binding))
            {
                style =
                {
                    color = new Color(0.55f, 0.55f, 0.55f, 1f),
                    fontSize = 11f,
                    marginTop = 2f,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            container.Add(statusLabel);

            return container;
        }

        private VisualElement CreateValueControl(TagHandlerAsset handler, InspectorParameterBinding binding)
        {
            if (binding.Definition is BoolParameterDefinitionAsset)
            {
                var toggle = new Toggle
                {
                    text = string.Empty
                };

                var current = InterpretBooleanValue(binding);
                toggle.SetValueWithoutNotify(current);
                toggle.tooltip = binding.IsPresent
                    ? "Markup stores this boolean flag explicitly."
                    : "Flag not present in markup; toggling will add or remove it.";
                toggle.RegisterValueChangedCallback(evt =>
                {
                    var alias = !string.IsNullOrEmpty(binding.MatchedIdentifier)
                        ? binding.MatchedIdentifier
                        : binding.PrimaryIdentifier;
                    var value = evt.newValue ? "true" : string.Empty;
                    var updatedBinding = binding.WithValue(alias, value, evt.newValue);
                    OnParameterFieldChanged(handler, updatedBinding, value);
                });
                return toggle;
            }

            var field = new TextField(string.Empty)
            {
                isDelayed = true,
                tooltip = binding.IsPresent
                    ? "Value specified explicitly in markup."
                    : (!string.IsNullOrEmpty(binding.DefaultValue)
                        ? $"Using default value: {binding.DefaultValue}"
                        : "Value not specified in markup."),
                style = { flexGrow = 1f }
            };

            field.SetValueWithoutNotify(binding.HasContent ? binding.Value : string.Empty);
            field.RegisterValueChangedCallback(evt =>
            {
                var alias = !string.IsNullOrEmpty(binding.MatchedIdentifier)
                    ? binding.MatchedIdentifier
                    : binding.PrimaryIdentifier;
                var value = evt.newValue ?? string.Empty;
                var updatedBinding = binding.WithValue(alias, value, !string.IsNullOrEmpty(value));
                OnParameterFieldChanged(handler, updatedBinding, value);
            });
            return field;
        }

        private static bool InterpretBooleanValue(InspectorParameterBinding binding)
        {
            if (!binding.IsPresent)
            {
                return string.Equals(binding.DefaultValue, "true", StringComparison.OrdinalIgnoreCase);
            }

            return !string.IsNullOrEmpty(binding.Value) &&
                   (string.Equals(binding.Value, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(binding.Value, "1", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(binding.Value, "yes", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(binding.Value, "on", StringComparison.OrdinalIgnoreCase));
        }

        private void OnParameterFieldChanged(TagHandlerAsset handler, InspectorParameterBinding binding, string newValue)
        {
            ApplyInspectorParameterValue(handler, binding, newValue);
        }

        private void EnsurePreviewHandler()
        {
            if (_previewWindow == null)
            {
                return;
            }

            _previewWindow.SetPreviewGuiHandler(OnPreviewGui);
        }

        private void RebuildPreview()
        {
            if (!EnsurePreviewSceneLoaded())
            {
                _previewDirty = true;
                return;
            }

            _previewDirty = false;
            UpdatePreviewStatus("Preview updated at " + DateTime.Now.ToLongTimeString());
            _previewWindow?.SetPreviewGuiHandler(OnPreviewGui);
            Repaint();
        }

        private void OnPreviewGui()
        {
            EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);

            if (_previewDirty)
            {
                EditorGUILayout.HelpBox("Preview rebuild pending. Click 'Refresh Preview' to update.", MessageType.Info);
                return;
            }

            if (_previewAnimator == null)
            {
                EditorGUILayout.HelpBox("Configured preview scene does not contain a valid TextAnimator.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Preview scene loaded. Use the Playback window to control animation.", MessageType.None);
            EditorGUILayout.Space(8f);

            EditorGUILayout.LabelField("Current State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Playback", _playbackState.ToString());
            if (_previewSession != null)
            {
                EditorGUILayout.LabelField("Elapsed", _previewSession.ElapsedTime.ToString("F2") + "s");
            }
            else
            {
                EditorGUILayout.LabelField("Elapsed", "0.00s");
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Markup Preview:");
            EditorGUILayout.TextArea(CurrentMarkup, GUILayout.Height(100f));

            var previewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (previewRect.width > 32f && previewRect.height > 32f)
            {
                EnsureRenderTexture((int)previewRect.width, (int)previewRect.height);
                RenderPreviewCamera();

                if (_previewRenderTexture != null)
                {
                    EditorGUI.DrawPreviewTexture(previewRect, _previewRenderTexture, null, ScaleMode.ScaleToFit);
                }
            }
        }

        private void UpdatePlaybackTransport()
        {
            var isPlaying = _playbackState == AnimationPlaybackState.Playing;
            var isPaused = _playbackState == AnimationPlaybackState.Paused;
            _playbackWindow?.UpdateTransportState(isPlaying, isPaused);
        }

        private void ResetPreviewSceneState()
        {
            _loadedPreviewScene = default;
            _previewSceneLoaded = false;
            _previewSceneWasLoadedExternally = false;
            _previewScenePath = null;
            _previewRootObject = null;
            _previewCamera = null;
            _previewAnimator = null;
            _previewText = null;
            _previewSession = null;
            _lastPreviewUpdate = EditorApplication.timeSinceStartup;
            ReleaseRenderTexture();
        }

        private bool EnsurePreviewSceneLoaded()
        {
            var sceneAsset = _state.PreviewScene;
            if (sceneAsset == null)
            {
                UpdatePreviewStatus("Assign a Preview Scene asset to enable the animation preview.");
                UnloadPreviewScene();
                return false;
            }

            var path = AssetDatabase.GetAssetPath(sceneAsset);
            if (string.IsNullOrWhiteSpace(path))
            {
                UpdatePreviewStatus("Unable to resolve the path for the assigned Preview Scene asset.");
                UnloadPreviewScene();
                return false;
            }

            if (_previewSceneLoaded && string.Equals(_previewScenePath, path, StringComparison.Ordinal))
            {
                if (!ValidateLoadedPreviewScene())
                {
                    UnloadPreviewScene();
                }
                else
                {
                    LocatePreviewObjects();
                    return _previewRootObject != null && _previewCamera != null;
                }
            }

            UnloadPreviewScene();

            var existingScene = SceneManager.GetSceneByPath(path);
            try
            {
                if (existingScene.IsValid() && existingScene.isLoaded)
                {
                    _loadedPreviewScene = existingScene;
                    _previewSceneWasLoadedExternally = true;
                }
                else
                {
                    _loadedPreviewScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    _previewSceneWasLoadedExternally = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Animation Studio] Failed to open preview scene '{path}': {ex.Message}");
                UpdatePreviewStatus("Failed to open preview scene. See console for details.");
                return false;
            }

            _previewSceneLoaded = _loadedPreviewScene.IsValid() && _loadedPreviewScene.isLoaded;
            if (!_previewSceneLoaded)
            {
                UpdatePreviewStatus("Loaded preview scene is not valid or failed to load.");
                UnloadPreviewScene();
                return false;
            }

            _previewScenePath = path;
            LocatePreviewObjects();

            if (_previewCamera == null)
            {
                UpdatePreviewStatus("Preview scene is missing a Camera component.");
                return false;
            }

            if (_previewRootObject == null)
            {
                UpdatePreviewStatus("Preview scene does not contain a TextAnimator instance.");
                return false;
            }

            if (!ConfigurePreviewAnimator())
            {
                return false;
            }

            UpdatePreviewStatus("Preview scene loaded successfully.");
            return true;
        }

        private bool ValidateLoadedPreviewScene()
        {
            if (!_previewSceneLoaded)
            {
                return false;
            }

            if (!_loadedPreviewScene.IsValid() || !_loadedPreviewScene.isLoaded)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(_previewScenePath) && !string.Equals(_loadedPreviewScene.path, _previewScenePath, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        private void LocatePreviewObjects()
        {
            _previewRootObject = null;
            _previewCamera = null;
            _previewAnimator = null;
            _previewText = null;

            if (!_previewSceneLoaded)
            {
                return;
            }

            var roots = _loadedPreviewScene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (_previewRootObject == null)
                {
                    var animator = root.GetComponentInChildren<TextAnimator.Animator.TextAnimator>(true);
                    if (animator != null)
                    {
                        _previewRootObject = animator.gameObject;
                        _previewAnimator = animator;
                        _previewText = animator.GetComponentInChildren<TMP_Text>(true);
                    }
                }

                if (_previewCamera == null)
                {
                    var camera = root.GetComponentInChildren<Camera>(true);
                    if (camera != null)
                    {
                        _previewCamera = camera;
                    }
                }

                if (_previewRootObject != null && _previewCamera != null)
                {
                    break;
                }
            }

            if (_previewCamera == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null && mainCamera.gameObject.scene == _loadedPreviewScene)
                {
                    _previewCamera = mainCamera;
                }
            }

            if (_previewAnimator == null && _previewRootObject != null)
            {
                _previewAnimator = _previewRootObject.GetComponentInChildren<TextAnimator.Animator.TextAnimator>(true);
            }

            if (_previewText == null && _previewAnimator != null)
            {
                _previewText = _previewAnimator.GetComponentInChildren<TMP_Text>(true);
            }
        }

        private bool ConfigurePreviewAnimator()
        {
            if (_previewAnimator == null)
            {
                UpdatePreviewStatus("Preview scene requires a TextAnimator component.");
                return false;
            }

            var bundle = _state.SubsystemBundle;
            var additionalSubsystems = _state.AdditionalSubsystems;
            if (bundle == null && (additionalSubsystems == null || additionalSubsystems.Count == 0))
            {
                UpdatePreviewStatus("Assign a subsystem bundle or additional subsystems to play the preview.");
                return false;
            }

            var serializedAnimator = new SerializedObject(_previewAnimator);
            serializedAnimator.Update();

            var registryProperty = serializedAnimator.FindProperty("handlerRegistry");
            if (registryProperty != null)
            {
                registryProperty.objectReferenceValue = _state.Registry;
            }

            var parserProperty = serializedAnimator.FindProperty("parserAsset");
            if (parserProperty != null)
            {
                parserProperty.objectReferenceValue = _state.Parser;
            }

            var bundleProperty = serializedAnimator.FindProperty("subsystemBundle");
            if (bundleProperty != null)
            {
                bundleProperty.objectReferenceValue = bundle;
            }

            var subsystemsProperty = serializedAnimator.FindProperty("subsystems");
            if (subsystemsProperty != null)
            {
                var count = additionalSubsystems?.Count ?? 0;
                subsystemsProperty.arraySize = count;
                for (var i = 0; i < count; i++)
                {
                    subsystemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = additionalSubsystems![i];
                }
            }

            var initialMarkupProperty = serializedAnimator.FindProperty("initialMarkup");
            if (initialMarkupProperty != null)
            {
                initialMarkupProperty.stringValue = CurrentMarkup;
            }

            var playOnEnableProperty = serializedAnimator.FindProperty("playOnEnable");
            if (playOnEnableProperty != null)
            {
                playOnEnableProperty.boolValue = false;
            }

            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();

            try
            {
                _previewAnimator.Reinitialize();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Animation Studio] Failed to configure preview TextAnimator: {ex.Message}");
                UpdatePreviewStatus("Failed to configure preview animator. Check console for details.");
                return false;
            }

            if (_previewText != null)
            {
                _previewText.text = string.Empty;
            }

            return true;
        }

        private void UnloadPreviewScene()
        {
            if (!_previewSceneLoaded)
            {
                return;
            }

            if (_loadedPreviewScene.IsValid() && !_previewSceneWasLoadedExternally)
            {
                EditorSceneManager.CloseScene(_loadedPreviewScene, true);
            }

            ResetPreviewSceneState();
        }

        private void UpdatePreviewStatus(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (_previewStatusLabel is INotifyValueChanged<string> notify)
            {
                notify.SetValueWithoutNotify(message);
                return;
            }

            if (_previewStatusLabel != null)
            {
                _previewStatusLabel.text = message;
            }
        }

        private bool EnsurePreviewReady()
        {
            if (!EnsurePreviewSceneLoaded())
            {
                UpdatePreviewStatus("Preview scene is not ready. Assign a valid scene with camera and TextAnimator.");
                return false;
            }

            if (_previewAnimator == null)
            {
                UpdatePreviewStatus("Preview scene requires a TextAnimator component.");
                return false;
            }

            if (_previewCamera == null)
            {
                UpdatePreviewStatus("Preview scene requires a Camera to render preview.");
                return false;
            }

            if (_previewText == null)
            {
                UpdatePreviewStatus("Preview TextAnimator does not have a TMP_Text target set.");
                return false;
            }

            return true;
        }

        private void ResumePreviewPlayback()
        {
            if (_previewSession == null)
            {
                RestartPreviewPlayback();
                return;
            }

            _playbackState = AnimationPlaybackState.Playing;
            UpdatePlaybackTransport();
            _lastPreviewUpdate = EditorApplication.timeSinceStartup;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            UpdatePreviewStatus("Preview resumed.");
            RenderPreviewCamera();
            RepaintAllPreviewWindows();
        }

        private void RestartPreviewPlayback()
        {
            if (_previewAnimator == null)
            {
                UpdatePreviewStatus("Preview scene requires a TextAnimator component.");
                return;
            }

            StopPreviewPlayback();
            try
            {
                _previewAnimator.Play(CurrentMarkup);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Animation Studio] Failed to start preview playback: {ex.Message}");
                UpdatePreviewStatus("Preview playback failed. Check console for details.");
                return;
            }

            _previewSession = GetActiveSession(_previewAnimator);
            if (_previewSession == null)
            {
                UpdatePreviewStatus("Preview session unavailable. Check TextAnimator configuration.");
                return;
            }

            _lastPreviewUpdate = EditorApplication.timeSinceStartup;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            _playbackState = AnimationPlaybackState.Playing;
            UpdatePlaybackTransport();
            UpdatePreviewStatus("Preview playing...");
            RenderPreviewCamera();
            RepaintAllPreviewWindows();
        }

        private void PausePreviewPlayback()
        {
            if (_previewSession == null)
            {
                return;
            }

            EditorApplication.update -= OnEditorUpdate;
            UpdatePreviewStatus("Preview paused.");
        }

        private void StopPreviewPlayback()
        {
            EditorApplication.update -= OnEditorUpdate;

            if (_previewAnimator != null)
            {
                try
                {
                    _previewAnimator.Stop();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Animation Studio] Failed to stop preview animator: {ex.Message}");
                }
            }

            _previewSession = null;
            _lastPreviewUpdate = EditorApplication.timeSinceStartup;
        }

        private void DisplayStaticMarkupSnapshot()
        {
            if (_previewText == null)
            {
                return;
            }

            var plainText = BuildPlainTextSnapshot(CurrentMarkup);
            _previewText.text = plainText;
            _previewText.ForceMeshUpdate(true, true);
            _previewText.maxVisibleCharacters = _previewText.textInfo.characterCount;
            RenderPreviewCamera();
            RepaintAllPreviewWindows();
        }

        private void OnEditorUpdate()
        {
            if (_previewSession == null)
            {
                return;
            }

            var now = EditorApplication.timeSinceStartup;
            var delta = (float)(now - _lastPreviewUpdate);
            _lastPreviewUpdate = now;

            try
            {
                var completed = _previewSession.Update(delta);
                if (completed)
                {
                    _previewSession = null;
                    _playbackState = AnimationPlaybackState.Stopped;
                    UpdatePlaybackTransport();
                    EditorApplication.update -= OnEditorUpdate;
                    UpdatePreviewStatus("Preview finished.");
                    DisplayStaticMarkupSnapshot();
                    return;
                }

                RenderPreviewCamera();
                RepaintAllPreviewWindows();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Animation Studio] Error updating preview playback: {ex.Message}");
                StopPreviewPlayback();
                UpdatePreviewStatus("Preview playback encountered an error. See console for details.");
                DisplayStaticMarkupSnapshot();
            }
        }

        private void RepaintAllPreviewWindows()
        {
            Repaint();
            _previewWindow?.Repaint();
        }

        private string BuildPlainTextSnapshot(string markup)
        {
            if (string.IsNullOrEmpty(markup))
            {
                return string.Empty;
            }

            if (!TryBuildDocumentTree(markup, out var root, out _))
            {
                return markup;
            }

            var builder = new StringBuilder(markup.Length);
            AppendPlainText(root, builder);
            return builder.ToString();
        }

        private static void AppendPlainText(TagNode? node, StringBuilder builder)
        {
            if (node == null)
            {
                return;
            }

            foreach (var child in node.Children)
            {
                switch (child)
                {
                    case TextNode textNode when !string.IsNullOrEmpty(textNode.Text):
                        builder.Append(textNode.Text);
                        break;
                    case TagNode tagNode:
                        AppendPlainText(tagNode, builder);
                        break;
                }
            }
        }

        private static PlaybackSession? GetActiveSession(TextAnimator.Animator.TextAnimator animator)
        {
            if (animator == null)
            {
                return null;
            }

            var activeSessionField = typeof(TextAnimator.Animator.TextAnimator).GetField("_activeSession", BindingFlags.NonPublic | BindingFlags.Instance);
            return activeSessionField?.GetValue(animator) as PlaybackSession;
        }

        private void EnsureRenderTexture(int width, int height)
        {
            width = Mathf.Clamp(width, 128, 4096);
            height = Mathf.Clamp(height, 128, 4096);

            if (_previewRenderTexture != null && (width != _previewTextureWidth || height != _previewTextureHeight))
            {
                ReleaseRenderTexture();
            }

            if (_previewRenderTexture == null)
            {
                _previewTextureWidth = width;
                _previewTextureHeight = height;
                _previewRenderTexture = new RenderTexture(_previewTextureWidth, _previewTextureHeight, 24, RenderTextureFormat.ARGB32)
                {
                    name = "AnimationStudioPreviewRT",
                    enableRandomWrite = false,
                    useMipMap = false,
                    autoGenerateMips = false
                };
                _previewRenderTexture.Create();
            }

            if (_previewCamera != null)
            {
                _previewCamera.targetTexture = _previewRenderTexture;
            }
        }

        private void RenderPreviewCamera()
        {
            if (_previewCamera == null || _previewRenderTexture == null)
            {
                return;
            }

            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = _previewRenderTexture;
                _previewCamera.targetTexture = _previewRenderTexture;
                _previewCamera.Render();
            }
            finally
            {
                RenderTexture.active = previous;
            }
        }

        private void ReleaseRenderTexture()
        {
            if (_previewCamera != null)
            {
                _previewCamera.targetTexture = null;
            }

            if (_previewRenderTexture != null)
            {
                _previewRenderTexture.Release();
                UnityEngine.Object.DestroyImmediate(_previewRenderTexture);
                _previewRenderTexture = null;
            }
        }

        private bool TryBuildDocumentTree(string markup, out TagNode? root, out string errorMessage)
        {
            root = null;
            errorMessage = string.Empty;

            ITagParser parser = null;
            if (_state.Parser != null)
            {
                try
                {
                    parser = _state.Parser.BuildParser();
                }
                catch (Exception ex)
                {
                    errorMessage = $"Parser asset '{_state.Parser.name}' failed to instantiate: {ex.Message}";
                    return false;
                }
            }

            parser ??= new CurlyBraceTagParser();

            try
            {
                root = parser.Parse(markup);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static void CollectTagSummaries(TagNode? node, int depth, List<TagSummary> results)
        {
            if (node == null)
            {
                return;
            }

            if (depth > 0)
            {
                results.Add(new TagSummary(node, depth));
            }

            foreach (var child in node.Children)
            {
                if (child is TagNode childTag)
                {
                    CollectTagSummaries(childTag, depth + 1, results);
                }
            }
        }

        private static string? GetNearestTagDeclaration(string markup, int selectionStart)
        {
            if (string.IsNullOrEmpty(markup))
            {
                return null;
            }

            var index = Mathf.Clamp(selectionStart, 0, Mathf.Max(0, markup.Length - 1));
            var openIndex = markup.LastIndexOf('{', index);
            if (openIndex == -1)
            {
                return null;
            }

            var closeIndex = markup.IndexOf('}', openIndex);
            if (closeIndex == -1)
            {
                return null;
            }

            var content = markup.Substring(openIndex, closeIndex - openIndex + 1);
            return string.IsNullOrWhiteSpace(content) ? null : content;
        }

        private void ApplyInspectorParameterValue(TagHandlerAsset handler, InspectorParameterBinding binding, string newValue)
        {
            if (handler == null)
            {
                return;
            }

            if (binding.TagName == null)
            {
                return;
            }

            var markupWindow = _markupWindow;
            if (markupWindow == null)
            {
                return;
            }

            var currentMarkup = CurrentMarkup ?? string.Empty;
            if (string.IsNullOrEmpty(currentMarkup))
            {
                return;
            }

            if (!TryFindTagSpanByOccurrence(currentMarkup, binding.TagName, binding.OccurrenceIndex, out var tagStart, out var tagEnd))
            {
                return;
            }

            var tagSegment = currentMarkup.Substring(tagStart, tagEnd - tagStart);
            var updatedTag = UpdateTagAttributeSegment(tagSegment, binding.WithRawValue(newValue));
            if (updatedTag == null)
            {
                return;
            }

            if (string.Equals(tagSegment, updatedTag, StringComparison.Ordinal))
            {
                return;
            }

            var updatedMarkup = currentMarkup.Substring(0, tagStart) + updatedTag + currentMarkup.Substring(tagEnd);

            var selectionStart = tagStart;
            var selectionEnd = tagStart + updatedTag.Length;
            _lastExplicitSelectionRange = (selectionStart, selectionEnd);

            _coordinator?.UpdateMarkup(updatedMarkup, true);

            var selectionStartCaptured = selectionStart;
            var selectionEndCaptured = selectionEnd;
            EditorApplication.delayCall += () =>
            {
                if (_markupWindow == null)
                {
                    return;
                }

                _markupWindow.ApplySelection(selectionStartCaptured, selectionEndCaptured);
                _markupWindow.FocusMarkup();
                UpdateInspectorSelection();
            };
        }

        private static bool TryFindTagSpan(string markup, int searchIndex, string tagName, out int tagStart, out int tagEnd)
        {
            tagStart = -1;
            tagEnd = -1;

            if (string.IsNullOrEmpty(markup) || string.IsNullOrEmpty(tagName))
            {
                return false;
            }

            var index = Mathf.Clamp(searchIndex, 0, Math.Max(0, markup.Length - 1));
            while (index >= 0)
            {
                var openIndex = markup.LastIndexOf('{', index);
                if (openIndex == -1)
                {
                    break;
                }

                if (openIndex + 1 < markup.Length && markup[openIndex + 1] == '/')
                {
                    index = openIndex - 1;
                    continue;
                }

                var closeIndex = markup.IndexOf('}', openIndex);
                if (closeIndex == -1)
                {
                    break;
                }

                var inner = markup.Substring(openIndex + 1, closeIndex - openIndex - 1).TrimStart();
                if (inner.StartsWith(tagName, StringComparison.OrdinalIgnoreCase))
                {
                    tagStart = openIndex;
                    tagEnd = closeIndex + 1;
                    return true;
                }

                index = openIndex - 1;
            }

            return false;
        }

        private static string? UpdateTagAttributeSegment(string tagSegment, InspectorParameterBinding binding)
        {
            if (string.IsNullOrEmpty(tagSegment))
            {
                return null;
            }

            var closeIndex = tagSegment.IndexOf('}');
            if (!tagSegment.StartsWith("{", StringComparison.Ordinal) || closeIndex <= 1)
            {
                return null;
            }

            var inner = tagSegment.Substring(1, closeIndex - 1);
            var attributes = ParseTagAttributes(inner, out var tagName);

            var identifierCandidates = BuildIdentifierCandidates(binding);
            var effectiveIdentifier = !string.IsNullOrEmpty(binding.MatchedIdentifier) ? binding.MatchedIdentifier : binding.PrimaryIdentifier;
            if (string.IsNullOrEmpty(effectiveIdentifier))
            {
                return null;
            }

            var sanitizedValue = binding.Value ?? string.Empty;

            var existingIndex = FindAttributeIndex(attributes, identifierCandidates);
            if (string.IsNullOrEmpty(sanitizedValue))
            {
                if (existingIndex >= 0)
                {
                    attributes.RemoveAt(existingIndex);
                }

                // Remove other aliases if they exist.
                for (var i = attributes.Count - 1; i >= 0; i--)
                {
                    if (AliasesContain(identifierCandidates, attributes[i].Key))
                    {
                        attributes.RemoveAt(i);
                    }
                }
            }
            else if (existingIndex >= 0)
            {
                attributes[existingIndex] = new KeyValuePair<string, string>(attributes[existingIndex].Key, sanitizedValue);
            }
            else
            {
                attributes.Add(new KeyValuePair<string, string>(effectiveIdentifier, sanitizedValue));
            }

            var builder = new StringBuilder();
            builder.Append(tagName);
            for (var i = 0; i < attributes.Count; i++)
            {
                builder.Append(' ');
                builder.Append(attributes[i].Key);
                builder.Append('=');
                builder.Append('"');
                builder.Append(attributes[i].Value);
                builder.Append('"');
            }

            builder.Append('}');
            return "{" + builder.ToString();
        }

        private static List<string> BuildIdentifierCandidates(InspectorParameterBinding binding)
        {
            var aliases = binding.Aliases;
            var buffer = new List<string>(aliases.Count + 2);
            for (var i = 0; i < aliases.Count; i++)
            {
                TryAddIdentifier(buffer, aliases[i]);
            }

            TryAddIdentifier(buffer, binding.PrimaryIdentifier);
            TryAddIdentifier(buffer, binding.MatchedIdentifier);

            return buffer;
        }

        private static void TryAddIdentifier(List<string> buffer, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return;
            }

            for (var i = 0; i < buffer.Count; i++)
            {
                if (string.Equals(buffer[i], identifier, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            buffer.Add(identifier);
        }

        private static List<KeyValuePair<string, string>> ParseTagAttributes(string inner, out string tagName)
        {
            var attributes = new List<KeyValuePair<string, string>>();
            var length = inner.Length;
            var index = 0;

            while (index < length && char.IsWhiteSpace(inner[index]))
            {
                index++;
            }

            var nameStart = index;
            while (index < length && !char.IsWhiteSpace(inner[index]))
            {
                index++;
            }

            tagName = inner.Substring(nameStart, index - nameStart);

            while (index < length)
            {
                while (index < length && char.IsWhiteSpace(inner[index]))
                {
                    index++;
                }

                if (index >= length)
                {
                    break;
                }

                var keyStart = index;
                while (index < length && !char.IsWhiteSpace(inner[index]) && inner[index] != '=')
                {
                    index++;
                }

                if (keyStart == index)
                {
                    index++;
                    continue;
                }

                var key = inner.Substring(keyStart, index - keyStart);

                while (index < length && char.IsWhiteSpace(inner[index]))
                {
                    index++;
                }

                if (index >= length || inner[index] != '=')
                {
                    continue;
                }

                index++;

                while (index < length && char.IsWhiteSpace(inner[index]))
                {
                    index++;
                }

                if (index >= length)
                {
                    attributes.Add(new KeyValuePair<string, string>(key, string.Empty));
                    break;
                }

                string value;
                if (inner[index] == '"')
                {
                    index++;
                    var valueStart = index;
                    while (index < length && inner[index] != '"')
                    {
                        index++;
                    }

                    value = inner.Substring(valueStart, index - valueStart);
                    if (index < length && inner[index] == '"')
                    {
                        index++;
                    }
                }
                else
                {
                    var valueStart = index;
                    while (index < length && !char.IsWhiteSpace(inner[index]))
                    {
                        index++;
                    }

                    value = inner.Substring(valueStart, index - valueStart);
                }

                attributes.Add(new KeyValuePair<string, string>(key, value));
            }

            return attributes;
        }

        private static int FindAttributeIndex(List<KeyValuePair<string, string>> attributes, IReadOnlyList<string> aliases)
        {
            for (var i = 0; i < attributes.Count; i++)
            {
                if (AliasesContain(aliases, attributes[i].Key))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool AliasesContain(IReadOnlyList<string> aliases, string candidate)
        {
            for (var i = 0; i < aliases.Count; i++)
            {
                if (string.Equals(aliases[i], candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private readonly struct InspectorParameterBinding
        {
            public InspectorParameterBinding(
                string tagName,
                int occurrenceIndex,
                string label,
                BaseParameterDefinitionAsset definition,
                IReadOnlyList<string> aliases,
                string primaryIdentifier,
                string matchedIdentifier,
                string value,
                bool isPresent,
                bool hasContent,
                string defaultValue)
            {
                TagName = tagName;
                OccurrenceIndex = occurrenceIndex;
                Label = label;
                Definition = definition;
                Aliases = aliases;
                PrimaryIdentifier = primaryIdentifier;
                MatchedIdentifier = matchedIdentifier;
                Value = value;
                IsPresent = isPresent;
                HasContent = hasContent;
                DefaultValue = defaultValue;
            }

            public string TagName { get; }

            public int OccurrenceIndex { get; }

            public string Label { get; }

            public BaseParameterDefinitionAsset Definition { get; }

            public IReadOnlyList<string> Aliases { get; }

            public string PrimaryIdentifier { get; }

            public string MatchedIdentifier { get; }

            public string Value { get; }

            public bool IsPresent { get; }

            public bool HasContent { get; }

            public string DefaultValue { get; }

            public InspectorParameterBinding WithValue(string identifier, string value, bool isPresent)
            {
                return new InspectorParameterBinding(
                    TagName,
                    OccurrenceIndex,
                    Label,
                    Definition,
                    Aliases,
                    PrimaryIdentifier,
                    identifier,
                    value,
                    isPresent,
                    !string.IsNullOrEmpty(value),
                    DefaultValue);
            }

            public InspectorParameterBinding WithRawValue(string value)
            {
                return new InspectorParameterBinding(
                    TagName,
                    OccurrenceIndex,
                    Label,
                    Definition,
                    Aliases,
                    PrimaryIdentifier,
                    MatchedIdentifier,
                    value,
                    !string.IsNullOrEmpty(value),
                    !string.IsNullOrEmpty(value),
                    DefaultValue);
            }
        }

        private readonly struct TagSummary
        {
            public TagSummary(TagNode node, int depth)
            {
                Node = node;
                Depth = depth;
            }

            public TagNode Node { get; }

            public int Depth { get; }
        }

        private sealed class TagInspectorEntry
        {
            public TagInspectorEntry(TagSummary summary, TagRange range, TagHandlerAsset? handler, int occurrenceIndex, string openingMarkup, string closingMarkup)
            {
                Summary = summary;
                Range = range;
                Handler = handler;
                OccurrenceIndex = occurrenceIndex;
                OpeningMarkup = openingMarkup;
                ClosingMarkup = closingMarkup;
            }

            public TagSummary Summary { get; }

            public TagRange Range { get; }

            public TagHandlerAsset? Handler { get; }

            public int OccurrenceIndex { get; }

            public string OpeningMarkup { get; }

            public string ClosingMarkup { get; }
        }

        private sealed class TagReplacement
        {
            public TagReplacement(int start, int length, string replacement)
            {
                Start = start;
                Length = length;
                Replacement = replacement ?? string.Empty;
            }

            public int Start { get; }

            public int Length { get; }

            public string Replacement { get; }
        }

        private readonly struct TagRange
        {
            public TagRange(
                string tagName,
                int occurrenceIndex,
                int openingStart,
                int openingEnd,
                int contentStart,
                int contentEnd,
                int closingStart,
                int closingEnd)
            {
                TagName = tagName;
                OccurrenceIndex = occurrenceIndex;
                OpeningStart = openingStart;
                OpeningEnd = openingEnd;
                ContentStart = contentStart;
                ContentEnd = contentEnd;
                ClosingStart = closingStart;
                ClosingEnd = closingEnd;
            }

            public string TagName { get; }

            public int OccurrenceIndex { get; }

            public int OpeningStart { get; }

            public int OpeningEnd { get; }

            public int ContentStart { get; }

            public int ContentEnd { get; }

            public int ClosingStart { get; }

            public int ClosingEnd { get; }

            public bool IsValid =>
                !string.IsNullOrEmpty(TagName) &&
                OpeningStart >= 0 &&
                OpeningEnd >= OpeningStart &&
                ContentStart >= OpeningEnd &&
                ContentEnd >= ContentStart &&
                ClosingStart >= ContentEnd &&
                ClosingEnd >= ClosingStart;
        }

        private readonly struct TagOpenFrame
        {
            public TagOpenFrame(string tagName, int occurrenceIndex, int openingStart, int openingEnd, int contentStart)
            {
                TagName = tagName;
                OccurrenceIndex = occurrenceIndex;
                OpeningStart = openingStart;
                OpeningEnd = openingEnd;
                ContentStart = contentStart;
            }

            public string TagName { get; }

            public int OccurrenceIndex { get; }

            public int OpeningStart { get; }

            public int OpeningEnd { get; }

            public int ContentStart { get; }
        }
    }
}
