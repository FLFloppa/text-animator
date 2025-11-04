using System;
using System.Collections.Generic;
using System.Linq;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Catalog;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Settings;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Panels
{
    /// <summary>
    /// UI Toolkit panel rendering the catalog of tag handlers for the Animation Studio workspace.
    /// </summary>
    internal sealed class AnimationStudioCatalogPanel
    {
        private readonly IAnimationStudioWorkspaceCoordinator _coordinator;
        private readonly List<VisualElement> _catalogTiles = new List<VisualElement>();
        private AnimationStudioCatalogItemAsset? _selectedItem;
        private VisualElement? _selectedTile;
        private readonly Image _detailPreviewImage;
        private readonly Label _detailTitleLabel;
        private readonly Label _detailDescriptionLabel;
        private readonly Button _detailInsertButton;
        private readonly VisualElement _detailContainer;
        private readonly TextField _searchField;
        private string _currentSearchTerm = string.Empty;
        private readonly Dictionary<AnimationStudioCatalogGroupAsset, bool> _groupExpansionStates = new Dictionary<AnimationStudioCatalogGroupAsset, bool>();

        /// <summary>
        /// Event invoked when a handler tile is activated via click.
        /// </summary>
        public event Action<TagHandlerAsset>? HandlerInvoked;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationStudioCatalogPanel"/> class.
        /// </summary>
        /// <param name="coordinator">Coordinator providing workspace services.</param>
        public AnimationStudioCatalogPanel(IAnimationStudioWorkspaceCoordinator coordinator)
        {
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));

            Root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1f,
                    paddingLeft = 6f,
                    paddingRight = 6f,
                    paddingTop = 6f,
                    paddingBottom = 6f
                }
            };

            var header = InspectorUi.Layout.CreateHeader("Effects Catalog", "Drag handlers into the text to apply tags.");
            Root.Add(header);
            header.style.marginBottom = 6f;

            ScrollView = new ScrollView
            {
                style =
                {
                    flexGrow = 1f
                }
            };
            Root.Add(ScrollView);
            ScrollView.style.marginTop = 0f;

            var searchRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 6f
                }
            };
            Root.Insert(1, searchRow);

            var searchLabel = new Label("Search")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 12f,
                    minWidth = 54f,
                    marginRight = 6f
                }
            };
            searchRow.Add(searchLabel);

            _searchField = new TextField
            {
                style =
                {
                    flexGrow = 1f
                },
                tooltip = "Search tags by name or description."
            };
            _searchField.RegisterValueChangedCallback(OnSearchValueChanged);
            searchRow.Add(_searchField);

            CatalogGrid = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1f
                }
            };
            ScrollView.Add(CatalogGrid);

            _detailContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.FlexStart,
                    paddingTop = 10f,
                    paddingBottom = 10f,
                    paddingLeft = 12f,
                    paddingRight = 12f,
                    marginTop = 12f,
                    backgroundColor = EditorGUIUtility.isProSkin
                        ? new Color(0.18f, 0.18f, 0.18f, 1f)
                        : new Color(0.95f, 0.95f, 0.95f, 1f),
                    borderTopLeftRadius = 6f,
                    borderTopRightRadius = 6f,
                    borderBottomLeftRadius = 6f,
                    borderBottomRightRadius = 6f,
                    borderTopWidth = 1f,
                    borderBottomWidth = 1f,
                    borderLeftWidth = 1f,
                    borderRightWidth = 1f,
                    borderTopColor = new Color(0f, 0f, 0f, 0.25f),
                    borderBottomColor = new Color(0f, 0f, 0f, 0.25f),
                    borderLeftColor = new Color(0f, 0f, 0f, 0.25f),
                    borderRightColor = new Color(0f, 0f, 0f, 0.25f)
                }
            };

            _detailPreviewImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 96f,
                    height = 96f,
                    marginRight = 12f
                }
            };
            _detailContainer.Add(_detailPreviewImage);

            var detailTextColumn = new VisualElement
            {
                style =
                {
                    flexGrow = 1f,
                    flexDirection = FlexDirection.Column
                }
            };
            _detailContainer.Add(detailTextColumn);

            _detailTitleLabel = new Label(string.Empty)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 16f,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            detailTextColumn.Add(_detailTitleLabel);

            _detailDescriptionLabel = new Label(string.Empty)
            {
                style =
                {
                    marginTop = 6f,
                    whiteSpace = WhiteSpace.Normal,
                    color = EditorGUIUtility.isProSkin
                        ? new Color(1f, 1f, 1f, 0.8f)
                        : new Color(0f, 0f, 0f, 0.8f)
                }
            };
            detailTextColumn.Add(_detailDescriptionLabel);

            _detailInsertButton = InspectorUi.Controls.CreateActionButton("Insert", InvokeSelectedHandler);
            _detailInsertButton.style.marginTop = 10f;
            detailTextColumn.Add(_detailInsertButton);

            Root.Add(_detailContainer);
            ShowDetail(false);
        }

        /// <summary>
        /// Gets the root visual element of the panel.
        /// </summary>
        public VisualElement Root { get; }

        /// <summary>
        /// Gets the scroll view containing the catalog tiles.
        /// </summary>
        public ScrollView ScrollView { get; }

        /// <summary>
        /// Gets the element acting as a grid for catalog tiles.
        /// </summary>
        public VisualElement CatalogGrid { get; }

        /// <summary>
        /// Refreshes the catalog contents based on the current registry.
        /// </summary>
        public void Refresh()
        {
            var previousSelection = _selectedItem;
            CatalogGrid.Clear();
            _catalogTiles.Clear();
            _selectedTile = null;
            ShowDetail(false);

            var settings = AnimationStudioSettings.Instance;
            var registry = settings.CatalogRegistry;
            if (registry == null)
            {
                AddCatalogInfoMessage("Assign a Catalog Registry in Animation Studio settings to populate the catalog.", HelpBoxMessageType.Info);
                return;
            }

            var groups = registry.Groups;
            if (groups == null || groups.Count == 0)
            {
                AddCatalogInfoMessage("Catalog registry does not contain any groups.", HelpBoxMessageType.Info);
                return;
            }

            var filterActive = !string.IsNullOrWhiteSpace(_currentSearchTerm);
            var previousStates = new Dictionary<AnimationStudioCatalogGroupAsset, bool>(_groupExpansionStates);
            _groupExpansionStates.Clear();

            var selectionRestored = false;
            var groupCount = 0;

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group == null)
                {
                    AddCatalogInfoMessage($"Group slot {i + 1} is empty.", HelpBoxMessageType.Warning);
                    continue;
                }

                var groupItems = group.Items ?? Array.Empty<AnimationStudioCatalogItemAsset>();
                var matchedItems = new List<AnimationStudioCatalogItemAsset>();
                var missingIndexes = new List<int>();

                for (var j = 0; j < groupItems.Count; j++)
                {
                    var item = groupItems[j];
                    if (item == null)
                    {
                        if (!filterActive)
                        {
                            missingIndexes.Add(_catalogTiles.Count + matchedItems.Count + missingIndexes.Count);
                        }

                        continue;
                    }

                    if (MatchesFilter(item))
                    {
                        matchedItems.Add(item);
                    }
                    else if (!filterActive)
                    {
                        matchedItems.Add(item);
                    }
                }

                if (filterActive)
                {
                    matchedItems = matchedItems.Where(MatchesFilter).ToList();
                }

                if (matchedItems.Count == 0 && missingIndexes.Count == 0)
                {
                    continue;
                }

                var groupTitle = string.IsNullOrWhiteSpace(group.DisplayName) ? group.name : group.DisplayName;

                var foldout = new Foldout
                {
                    text = groupTitle
                };

                var initialState = filterActive
                    ? true
                    : previousStates.TryGetValue(group, out var storedState) ? storedState : true;
                foldout.value = initialState;
                foldout.style.marginBottom = 6f;
                foldout.RegisterValueChangedCallback(evt => _groupExpansionStates[group] = evt.newValue);
                _groupExpansionStates[group] = foldout.value;

                var headerToggle = foldout.Q<Toggle>();
                if (headerToggle != null)
                {
                    headerToggle.style.flexDirection = FlexDirection.Row;
                    headerToggle.style.alignItems = Align.Center;
                    headerToggle.style.paddingRight = 6f;
                    headerToggle.style.justifyContent = Justify.FlexStart;

                    var inputContainer = headerToggle.Q<VisualElement>("unity-toggle__input");
                    if (inputContainer != null)
                    {
                        inputContainer.style.flexDirection = FlexDirection.Row;
                        inputContainer.style.alignItems = Align.Center;
                        inputContainer.style.flexGrow = 1f;
                    }

                    var label = headerToggle.Q<Label>("unity-toggle__text") ?? headerToggle.Q<Label>();
                    if (label != null)
                    {
                        label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        label.style.fontSize = 15f;
                        label.style.marginRight = 6f;
                        label.style.unityTextAlign = TextAnchor.MiddleLeft;
                        label.style.flexShrink = 0;

                        var labelParent = label.parent;
                        if (labelParent != null)
                        {
                            labelParent.style.flexDirection = FlexDirection.Row;
                            labelParent.style.alignItems = Align.Center;
                            labelParent.style.flexGrow = 1f;

                            var divider = labelParent.Q<VisualElement>("animation-studio-catalog__group-divider");
                            if (divider == null)
                            {
                                divider = new VisualElement
                                {
                                    name = "animation-studio-catalog__group-divider",
                                    style =
                                    {
                                        flexGrow = 1f,
                                        height = 3f,
                                        alignSelf = Align.Center,
                                        backgroundColor = EditorGUIUtility.isProSkin
                                            ? new Color(0.25f, 0.25f, 0.25f, 1f)
                                            : new Color(0.15f, 0.15f, 0.15f, 0.9f),
                                        borderTopLeftRadius = 2f,
                                        borderTopRightRadius = 2f,
                                        borderBottomLeftRadius = 2f,
                                        borderBottomRightRadius = 2f
                                    }
                                };
                                labelParent.Add(divider);
                            }

                            var dividerIndex = labelParent.IndexOf(divider);
                            var labelIndex = labelParent.IndexOf(label);
                            if (dividerIndex <= labelIndex)
                            {
                                labelParent.Remove(divider);
                                labelParent.Insert(labelIndex + 1, divider);
                            }
                        }
                    }
                }

                var tileContainer = CreateGroupTileContainer();

                for (var j = 0; j < matchedItems.Count; j++)
                {
                    var item = matchedItems[j];
                    if (item == null)
                    {
                        continue;
                    }

                    var tile = CreateCatalogTile(item, _catalogTiles.Count);
                    tileContainer.Add(tile);
                    _catalogTiles.Add(tile);

                    if (!selectionRestored && previousSelection == item)
                    {
                        SelectItem(item, tile);
                        selectionRestored = true;
                    }
                }

                if (!filterActive)
                {
                    for (var j = 0; j < missingIndexes.Count; j++)
                    {
                        var tile = CreateMissingHandlerTile(_catalogTiles.Count);
                        tileContainer.Add(tile);
                        _catalogTiles.Add(tile);
                    }
                }

                foldout.Add(tileContainer);
                CatalogGrid.Add(foldout);
                groupCount++;
            }

            if (groupCount == 0)
            {
                var message = filterActive
                    ? "No catalog items match the search criteria."
                    : "Catalog registry does not contain any items.";
                AddCatalogInfoMessage(message, HelpBoxMessageType.Info);
            }

            if (previousSelection != null && !selectionRestored)
            {
                _selectedItem = null;
                ShowDetail(false);
            }
        }

        /// <summary>
        /// Attempts to resolve a handler by identifier or alias using the cached registry data.
        /// </summary>
        /// <param name="identifier">Identifier to resolve.</param>
        /// <returns>The handler if found; otherwise, <c>null</c>.</returns>
        public TagHandlerAsset? ResolveHandler(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            var settings = AnimationStudioSettings.Instance;
            var registry = settings.CatalogRegistry;
            if (registry == null)
            {
                return null;
            }

            var groups = registry.Groups;
            if (groups == null)
            {
                return null;
            }

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group == null)
                {
                    continue;
                }

                var items = group.Items;
                for (var j = 0; j < items.Count; j++)
                {
                    var item = items[j];
                    if (item?.Handler == null)
                    {
                        continue;
                    }

                    var handler = item.Handler;
                    if (string.Equals(handler.name, identifier, StringComparison.OrdinalIgnoreCase))
                    {
                        return handler;
                    }

                    var aliases = handler.TagIdentifiers;
                    for (var k = 0; k < aliases.Count; k++)
                    {
                        if (string.Equals(aliases[k], identifier, StringComparison.OrdinalIgnoreCase))
                        {
                            return handler;
                        }
                    }
                }
            }

            return null;
        }

        private void AddCatalogInfoMessage(string message, HelpBoxMessageType type)
        {
            CatalogGrid.Add(new HelpBox(message, type)
            {
                style =
                {
                    flexBasis = new StyleLength(new Length(100f, LengthUnit.Percent)),
                    marginLeft = 12f,
                    marginRight = 12f,
                    marginTop = 8f
                }
            });
        }

        private VisualElement CreateCatalogTile(AnimationStudioCatalogItemAsset item, int index)
        {
            var tile = CreateCatalogTileRoot();
            tile.userData = item;
            tile.tooltip = FormatItemTooltip(item, index);
            tile.AddManipulator(new CatalogDragManipulator(item));
            tile.RegisterCallback<ClickEvent>(evt => OnCatalogTileClicked(item, tile, evt));

            var previewTexture = item.PreviewTexture ?? item.Icon;
            var iconTexture = AnimationStudioSettings.Instance.ResolveCatalogIcon(previewTexture, item.Handler);
            tile.Add(CreateIconElement(iconTexture));

            var nameLabel = new Label(item.DisplayName)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13f,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            tile.Add(nameLabel);

            return tile;
        }

        private void OnCatalogTileClicked(AnimationStudioCatalogItemAsset item, VisualElement tile, ClickEvent evt)
        {
            SelectItem(item, tile);

            if (evt.clickCount > 1 && item.Handler != null)
            {
                HandlerInvoked?.Invoke(item.Handler);
            }

            evt.StopPropagation();
        }

        private VisualElement CreateMissingHandlerTile(int index)
        {
            var tile = CreateCatalogTileRoot();
            tile.tooltip = $"Slot {index + 1}: Catalog item missing";

            var icon = AnimationStudioSettings.Instance.ResolveCatalogIcon(null, null);
            tile.Add(CreateIconElement(icon));

            tile.Add(new Label("Empty Slot")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13f,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = new Color(0.85f, 0.55f, 0.15f, 1f)
                }
            });

            return tile;
        }

        private VisualElement CreateCatalogTileRoot()
        {
            var background = EditorGUIUtility.isProSkin
                ? new Color(0.21f, 0.21f, 0.21f, 1f)
                : new Color(0.92f, 0.92f, 0.92f, 1f);
            var border = new Color(0f, 0f, 0f, 0.25f);

            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Center,
                    justifyContent = Justify.FlexStart,
                    width = 140f,
                    minHeight = 140f,
                    paddingTop = 12f,
                    paddingBottom = 12f,
                    paddingLeft = 12f,
                    paddingRight = 12f,
                    marginBottom = 8f,
                    marginRight = 8f,
                    marginLeft = 8f,
                    backgroundColor = background,
                    borderTopWidth = 1f,
                    borderBottomWidth = 1f,
                    borderLeftWidth = 1f,
                    borderRightWidth = 1f,
                    borderTopColor = border,
                    borderBottomColor = border,
                    borderLeftColor = border,
                    borderRightColor = border,
                    borderTopLeftRadius = 6f,
                    borderTopRightRadius = 6f,
                    borderBottomLeftRadius = 6f,
                    borderBottomRightRadius = 6f
                }
            };
        }

        private void SelectItem(AnimationStudioCatalogItemAsset item, VisualElement tile)
        {
            if (_selectedTile != null)
            {
                _selectedTile.RemoveFromClassList("animation-studio-catalog__tile--selected");
                _selectedTile.style.borderBottomColor = new Color(0f, 0f, 0f, 0.25f);
                _selectedTile.style.borderTopColor = new Color(0f, 0f, 0f, 0.25f);
                _selectedTile.style.borderLeftColor = new Color(0f, 0f, 0f, 0.25f);
                _selectedTile.style.borderRightColor = new Color(0f, 0f, 0f, 0.25f);
            }

            _selectedItem = item;
            _selectedTile = tile;
            _selectedTile.AddToClassList("animation-studio-catalog__tile--selected");
            _selectedTile.style.borderBottomColor = new Color(0.35f, 0.45f, 0.9f, 1f);
            _selectedTile.style.borderTopColor = new Color(0.35f, 0.45f, 0.9f, 1f);
            _selectedTile.style.borderLeftColor = new Color(0.35f, 0.45f, 0.9f, 1f);
            _selectedTile.style.borderRightColor = new Color(0.35f, 0.45f, 0.9f, 1f);

            UpdateDetailPanel(item);
        }

        private void UpdateDetailPanel(AnimationStudioCatalogItemAsset item)
        {
            ShowDetail(true);

            var previewTexture = item.PreviewTexture ?? item.Icon;
            var resolvedIcon = AnimationStudioSettings.Instance.ResolveCatalogIcon(previewTexture, item.Handler);
            _detailPreviewImage.image = resolvedIcon;

            _detailTitleLabel.text = item.DisplayName;
            _detailDescriptionLabel.text = string.IsNullOrWhiteSpace(item.Description)
                ? ""
                : item.Description;

            var handlerAvailable = item.Handler != null;
            _detailInsertButton.SetEnabled(handlerAvailable);
        }

        private void ShowDetail(bool visible)
        {
            _detailContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void InvokeSelectedHandler()
        {
            if (_selectedItem?.Handler == null)
            {
                return;
            }

            HandlerInvoked?.Invoke(_selectedItem.Handler);
        }

        private string FormatItemTooltip(AnimationStudioCatalogItemAsset item, int index)
        {
            var handlerName = item.Handler != null ? item.Handler.name : "Not Assigned";
            return $"Slot {index + 1}\nHandler: {handlerName}";
        }

        private void OnSearchValueChanged(ChangeEvent<string> evt)
        {
            _currentSearchTerm = evt.newValue?.Trim() ?? string.Empty;
            Refresh();
        }

        private bool MatchesFilter(AnimationStudioCatalogItemAsset item)
        {
            if (item == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_currentSearchTerm))
            {
                return true;
            }

            var term = _currentSearchTerm;
            return item.DisplayName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   item.Description.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static VisualElement CreateGroupTileContainer()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.FlexStart,
                    alignContent = Align.FlexStart
                }
            };
        }

        private static VisualElement CreateIconElement(Texture2D iconTexture)
        {
            var icon = new Image
            {
                image = iconTexture,
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore,
                style =
                {
                    width = 56f,
                    height = 56f,
                    marginBottom = 8f
                }
            };

            return icon;
        }

        private sealed class CatalogDragManipulator : PointerManipulator
        {
            private const float DragThresholdSquared = 25f;

            private readonly AnimationStudioCatalogItemAsset _item;
            private bool _isPointerDown;
            private Vector3 _pressPosition;

            public CatalogDragManipulator(AnimationStudioCatalogItemAsset item)
            {
                _item = item;
                activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
                target.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
                target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
                target.UnregisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
                target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
            }

            private void OnPointerDownEvent(PointerDownEvent evt)
            {
                if (!CanStartManipulation(evt))
                {
                    return;
                }

                _isPointerDown = true;
                _pressPosition = evt.position;
                evt.StopPropagation();
            }

            private void OnPointerMoveEvent(PointerMoveEvent evt)
            {
                if (!_isPointerDown)
                {
                    return;
                }

                var delta = evt.position - _pressPosition;
                if (delta.sqrMagnitude < DragThresholdSquared)
                {
                    return;
                }

                StartDrag(evt);
            }

            private void OnPointerUpEvent(PointerUpEvent evt)
            {
                if (!CanStopManipulation(evt))
                {
                    return;
                }

                _isPointerDown = false;
            }

            private void StartDrag(PointerMoveEvent evt)
            {
                if (_item.Handler == null)
                {
                    _isPointerDown = false;
                    return;
                }

                _isPointerDown = false;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(AnimationStudioWindow.CatalogDragDataType, _item.Handler);
                DragAndDrop.StartDrag(_item.DisplayName);
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.StopPropagation();
            }
        }
    }
}
