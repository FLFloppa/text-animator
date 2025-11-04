using System;
using FLFloppa.EditorHelpers;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Panels
{
    /// <summary>
    /// UI Toolkit panel responsible for markup editing and handler drop interactions.
    /// </summary>
    internal sealed class AnimationStudioMarkupPanel
    {
        private const float MinimumMarkupHeight = 220f;
        private const float DefaultMarkupHeight = 360f;

        private readonly IAnimationStudioWorkspaceCoordinator _coordinator;
        private VisualElement? _markupTextInputElement;
        private readonly Func<TagHandlerAsset?> _draggedHandlerResolver;
        private readonly TextField _markupField;
        private readonly VisualElement _resizeHandle;
        private bool _markupDropOriginalColorCaptured;
        private Color _markupDropOriginalColor;
        private bool _markupDropHighlightActive;
        private bool _isResizingMarkup;
        private Vector2 _resizePointerStart;
        private float _resizeInitialHeight;
        private float _currentMarkupHeight = DefaultMarkupHeight;
        private int _currentSelectionStart;
        private int _currentSelectionEnd;
        private int _lastNonEmptySelectionStart;
        private int _lastNonEmptySelectionEnd;
        private bool _isPointerSelecting;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationStudioMarkupPanel"/> class.
        /// </summary>
        /// <param name="coordinator">Coordinator providing workspace services.</param>
        /// <param name="draggedHandlerResolver">Resolver used to convert drag data into a handler.</param>
        public AnimationStudioMarkupPanel(
            IAnimationStudioWorkspaceCoordinator coordinator,
            Func<TagHandlerAsset?> draggedHandlerResolver)
        {
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            if (draggedHandlerResolver == null)
            {
                throw new ArgumentNullException(nameof(draggedHandlerResolver));
            }

            Root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1f,
                    paddingLeft = 8f,
                    paddingRight = 8f,
                    paddingTop = 8f,
                    paddingBottom = 8f
                }
            };

            _markupField = new TextField
            {
                multiline = true,
                value = coordinator.CurrentMarkup,
                style =
                {
                    flexGrow = 0f,
                    flexShrink = 0f,
                    minHeight = MinimumMarkupHeight,
                    whiteSpace = WhiteSpace.Normal,
                    fontSize = 14f
                }
            };
            _markupField.selectAllOnFocus = false;
            _markupField.selectAllOnMouseUp = false;
            _markupField.RegisterValueChangedCallback(evt => OnMarkupChanged(evt.newValue));
            Root.Add(_markupField);
            ApplyMarkupHeight(DefaultMarkupHeight);

            ConfigureDragAndDrop(_markupField, draggedHandlerResolver);
            RegisterSelectionCallbacks(_markupField);

            _markupField.schedule.Execute(() =>
            {
                _markupTextInputElement ??= _markupField.Q("unity-text-input");
                if (_markupTextInputElement == null)
                {
                    return;
                }

                if (!_markupDropOriginalColorCaptured)
                {
                    _markupDropOriginalColor = _markupTextInputElement.resolvedStyle.backgroundColor;
                    _markupDropOriginalColorCaptured = true;
                }

                ApplyMarkupHeight(_currentMarkupHeight);

                _markupTextInputElement.RegisterCallback<PointerDownEvent>(OnTextPointerDown, TrickleDown.TrickleDown);
                _markupTextInputElement.RegisterCallback<PointerMoveEvent>(OnTextPointerMove, TrickleDown.TrickleDown);
                _markupTextInputElement.RegisterCallback<PointerUpEvent>(OnTextPointerUp, TrickleDown.TrickleDown);
            }).ExecuteLater(0);

            _resizeHandle = new VisualElement
            {
                style =
                {
                    height = 6f,
                    marginTop = 6f,
                    backgroundColor = new Color(0.32f, 0.32f, 0.32f, 1f),
                    cursor = new StyleCursor((StyleKeyword)MouseCursor.ResizeVertical)
                }
            };
            _resizeHandle.RegisterCallback<PointerDownEvent>(OnResizeHandlePointerDown);
            _resizeHandle.RegisterCallback<PointerMoveEvent>(OnResizeHandlePointerMove);
            _resizeHandle.RegisterCallback<PointerUpEvent>(OnResizeHandlePointerUp);
            Root.Add(_resizeHandle);
        }

        /// <summary>
        /// Gets the root visual element of the panel.
        /// </summary>
        public VisualElement Root { get; }

        /// <summary>
        /// Sets the markup content without triggering change callbacks.
        /// </summary>
        /// <param name="markup">Markup string to apply.</param>
        public void SetMarkup(string markup)
        {
            _markupField.SetValueWithoutNotify(markup);
        }

        /// <summary>
        /// Focuses the markup field.
        /// </summary>
        public void FocusMarkup() => _markupField.Focus();

        /// <summary>
        /// Gets the current selection range within the markup field.
        /// </summary>
        public (int start, int end) GetSelectionRange()
        {
            var range = ComputeSelectionRange();
            return range;
        }

        /// <summary>
        /// Gets the most recently captured selection range without applying fallbacks.
        /// </summary>
        public (int start, int end) GetCurrentSelectionRange()
        {
            return (_currentSelectionStart, _currentSelectionEnd);
        }

        private void OnMarkupChanged(string? value)
        {
            _coordinator.UpdateMarkup(value ?? string.Empty, true);
        }

        public event Action? SelectionChanged;

        private void OnSelectionChanged()
        {
            ScheduleSelectionUpdate();
        }

        /// <summary>
        /// Gets the most recent non-empty selection range recorded within the markup field.
        /// </summary>
        public (int start, int end) GetLastNonEmptySelectionRange() => (_lastNonEmptySelectionStart, _lastNonEmptySelectionEnd);

        /// <summary>
        /// Applies the specified selection range to the markup text field.
        /// </summary>
        /// <param name="start">Inclusive start index.</param>
        /// <param name="end">Exclusive end index.</param>
        public void ApplySelection(int start, int end)
        {
            var value = _markupField.value ?? string.Empty;
            start = Mathf.Clamp(start, 0, value.Length);
            end = Mathf.Clamp(end, 0, value.Length);
            if (end < start)
            {
                (start, end) = (end, start);
            }

            _markupField.SelectRange(start, end);
            _currentSelectionStart = start;
            _currentSelectionEnd = end;

            if (start != end)
            {
                _lastNonEmptySelectionStart = start;
                _lastNonEmptySelectionEnd = end;
            }

            ScheduleSelectionUpdate();
        }

        private (int start, int end) ComputeSelectionRange()
        {
            var cursor = _markupField.cursorIndex;
            var selection = _markupField.selectIndex;
            var length = _markupField.value?.Length ?? 0;
            var start = Mathf.Clamp(Math.Min(cursor, selection), 0, length);
            var end = Mathf.Clamp(Math.Max(cursor, selection), 0, length);
            _currentSelectionStart = start;
            _currentSelectionEnd = end;
            if (start != end)
            {
                _lastNonEmptySelectionStart = start;
                _lastNonEmptySelectionEnd = end;
            }
            if (start == end && _lastNonEmptySelectionStart != _lastNonEmptySelectionEnd)
            {
                return (_lastNonEmptySelectionStart, _lastNonEmptySelectionEnd);
            }

            return (start, end);
        }

        private void ScheduleSelectionUpdate()
        {
            if (_markupField == null)
            {
                return;
            }

            _markupField.schedule.Execute(() =>
            {
                ComputeSelectionRange();
                SelectionChanged?.Invoke();
            }).ExecuteLater(0);
        }

        private void OnTextPointerDown(PointerDownEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse)
            {
                return;
            }

            _isPointerSelecting = true;
            ScheduleSelectionUpdate();
        }

        private void OnTextPointerMove(PointerMoveEvent evt)
        {
            if (!_isPointerSelecting)
            {
                return;
            }

            ScheduleSelectionUpdate();
        }

        private void OnTextPointerUp(PointerUpEvent evt)
        {
            if (!_isPointerSelecting)
            {
                return;
            }

            _isPointerSelecting = false;
            ScheduleSelectionUpdate();
        }

        private void ConfigureDragAndDrop(TextField field, Func<TagHandlerAsset?> draggedHandlerResolver)
        {
            field.RegisterCallback<DragUpdatedEvent>(evt => OnDragUpdated(evt, draggedHandlerResolver));
            field.RegisterCallback<DragPerformEvent>(evt => OnDragPerform(evt, draggedHandlerResolver));
            field.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            field.RegisterCallback<DragExitedEvent>(OnDragExited);
        }

        private void RegisterSelectionCallbacks(TextField field)
        {
            field.RegisterCallback<KeyUpEvent>(_ => OnSelectionChanged());
            field.RegisterCallback<NavigationMoveEvent>(_ => OnSelectionChanged());
        }

        private void OnDragUpdated(DragUpdatedEvent evt, Func<TagHandlerAsset?> resolver)
        {
            var handler = resolver();
            if (handler == null)
            {
                SetMarkupDropHighlight(false);
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            SetMarkupDropHighlight(true);
            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt, Func<TagHandlerAsset?> resolver)
        {
            var handler = resolver();
            if (handler == null)
            {
                SetMarkupDropHighlight(false);
                return;
            }

            DragAndDrop.AcceptDrag();
            ApplyHandlerToSelection(handler);
            SetMarkupDropHighlight(false);
            evt.StopPropagation();
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            SetMarkupDropHighlight(false);
            evt.StopPropagation();
        }

        private void OnDragExited(DragExitedEvent evt)
        {
            SetMarkupDropHighlight(false);
            evt.StopPropagation();
        }

        private void SetMarkupDropHighlight(bool enabled)
        {
            if (_markupTextInputElement == null)
            {
                return;
            }

            if (enabled)
            {
                if (_markupDropHighlightActive)
                {
                    return;
                }

                if (!_markupDropOriginalColorCaptured)
                {
                    _markupDropOriginalColor = _markupTextInputElement.resolvedStyle.backgroundColor;
                    _markupDropOriginalColorCaptured = true;
                }

                var color = EditorGUIUtility.isProSkin ? new Color(0.26f, 0.45f, 0.78f, 0.35f) : new Color(0.36f, 0.56f, 0.88f, 0.45f);
                _markupTextInputElement.style.backgroundColor = color;
                _markupDropHighlightActive = true;
                return;
            }

            if (!_markupDropHighlightActive)
            {
                return;
            }

            _markupTextInputElement.style.backgroundColor = _markupDropOriginalColor;
            _markupDropHighlightActive = false;
        }

        /// <summary>
        /// Wraps the current selection with the specified handler tag.
        /// </summary>
        public void ApplyHandlerToSelection(TagHandlerAsset handler)
        {
            var (start, end) = GetSelectionRange();
            ApplyHandlerToRange(handler, start, end);
        }

        public void ApplyHandlerToRange(TagHandlerAsset handler, int start, int end)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var identifier = GetPrimaryIdentifier(handler) ?? handler.name;
            WrapRangeWithTag(identifier, start, end);
        }

        private void WrapRangeWithTag(string identifier, int start, int end)
        {
            var text = _markupField.value ?? string.Empty;
            start = Mathf.Clamp(start, 0, text.Length);
            end = Mathf.Clamp(end, 0, text.Length);
            if (end < start)
            {
                (start, end) = (end, start);
            }

            var before = text[..start];
            var selected = text[start..end];
            var after = text[end..];

            var openingTag = "{" + identifier + "}";
            var closingTag = "{/" + identifier + "}";
            var updated = before + openingTag + selected + closingTag + after;

            _markupField.SetValueWithoutNotify(updated);
            var selectionStart = start + openingTag.Length;
            _markupField.SelectRange(selectionStart, selectionStart + selected.Length);
            _coordinator.UpdateMarkup(updated, true);
            SelectionChanged?.Invoke();
        }

        private void OnResizeHandlePointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            _isResizingMarkup = true;
            _resizePointerStart = evt.position;
            _resizeInitialHeight = _currentMarkupHeight;
            _resizeHandle.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnResizeHandlePointerMove(PointerMoveEvent evt)
        {
            if (!_isResizingMarkup || !_resizeHandle.HasPointerCapture(evt.pointerId))
            {
                return;
            }

            var delta = evt.position.y - _resizePointerStart.y;
            var targetHeight = Mathf.Max(MinimumMarkupHeight, _resizeInitialHeight + delta);
            ApplyMarkupHeight(targetHeight);
            evt.StopPropagation();
        }

        private void OnResizeHandlePointerUp(PointerUpEvent evt)
        {
            if (!_isResizingMarkup || !_resizeHandle.HasPointerCapture(evt.pointerId))
            {
                return;
            }

            _isResizingMarkup = false;
            _resizeHandle.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void ApplyMarkupHeight(float height)
        {
            _currentMarkupHeight = Mathf.Max(MinimumMarkupHeight, height);

            _markupField.style.height = _currentMarkupHeight;
            _markupField.style.minHeight = MinimumMarkupHeight;

            if (_markupTextInputElement != null)
            {
                _markupTextInputElement.style.height = _currentMarkupHeight;
                _markupTextInputElement.style.minHeight = MinimumMarkupHeight;
            }
        }

        private static string? GetPrimaryIdentifier(TagHandlerAsset handler)
        {
            var identifiers = handler.TagIdentifiers;
            if (identifiers == null)
            {
                return null;
            }

            for (var i = 0; i < identifiers.Count; i++)
            {
                var candidate = identifiers[i];
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                return candidate.Trim();
            }

            return null;
        }
    }
}
