using System;
using FLFloppa.TextAnimator.Editor.AnimationStudio.Panels;
using FLFloppa.TextAnimator.Tags.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio
{
    /// <summary>
    /// Dockable editor window hosting the markup editor panel for the Animation Studio workspace.
    /// </summary>
    internal sealed class AnimationStudioMarkupWindow : EditorWindow
    {
        private IAnimationStudioWorkspaceCoordinator? _coordinator;
        private AnimationStudioMarkupPanel? _panel;
        private Func<TagHandlerAsset?>? _draggedHandlerResolver;
        private Action? _selectionChangedHandlers;

        /// <summary>
        /// Opens the markup window and assigns the specified coordinator.
        /// </summary>
        /// <param name="coordinator">Coordinator that exposes shared workspace services.</param>
        /// <param name="draggedHandlerResolver">Resolver used to convert drag payloads into handlers.</param>
        /// <param name="selectionChanged">Callback invoked when the text selection changes.</param>
        public static AnimationStudioMarkupWindow Open(
            IAnimationStudioWorkspaceCoordinator coordinator,
            Func<TagHandlerAsset?> draggedHandlerResolver,
            Action selectionChanged)
        {
            if (coordinator == null)
            {
                throw new ArgumentNullException(nameof(coordinator));
            }

            if (draggedHandlerResolver == null)
            {
                throw new ArgumentNullException(nameof(draggedHandlerResolver));
            }

            if (selectionChanged == null)
            {
                throw new ArgumentNullException(nameof(selectionChanged));
            }

            var window = GetWindow<AnimationStudioMarkupWindow>();
            window.Initialize(coordinator, draggedHandlerResolver, selectionChanged);
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Markup Editor");
            minSize = new Vector2(320f, 260f);

            if (_coordinator != null && _panel == null)
            {
                BuildUi();
            }
        }

        /// <summary>
        /// Associates the window with the workspace coordinator and delegates.
        /// </summary>
        public void Initialize(
            IAnimationStudioWorkspaceCoordinator coordinator,
            Func<TagHandlerAsset?> draggedHandlerResolver,
            Action selectionChanged)
        {
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            _draggedHandlerResolver = draggedHandlerResolver ?? throw new ArgumentNullException(nameof(draggedHandlerResolver));
            if (selectionChanged == null)
            {
                throw new ArgumentNullException(nameof(selectionChanged));
            }

            RegisterSelectionChanged(selectionChanged);
            BuildUi();
            UpdateMarkup(coordinator.CurrentMarkup);
        }

        /// <summary>
        /// Updates the markup text displayed in the panel without triggering change events.
        /// </summary>
        /// <param name="markup">Markup string to display.</param>
        public void UpdateMarkup(string markup)
        {
            SetMarkupSilently(markup);
        }

        /// <summary>
        /// Focuses the markup text field.
        /// </summary>
        public void FocusMarkup()
        {
            _panel?.FocusMarkup();
        }

        /// <summary>
        /// Gets the current selection range within the markup editor.
        /// </summary>
        /// <returns>A tuple containing the inclusive start index and exclusive end index of the selection.</returns>
        public (int start, int end) GetSelectionRange()
        {
            return _panel != null ? _panel.GetSelectionRange() : (0, 0);
        }

        /// <summary>
        /// Gets the most recently recorded non-empty selection range.
        /// </summary>
        public (int start, int end) GetLastNonEmptySelectionRange()
        {
            return _panel != null ? _panel.GetLastNonEmptySelectionRange() : (0, 0);
        }

        /// <summary>
        /// Applies the provided selection range to the markup editor.
        /// </summary>
        /// <param name="start">Inclusive selection start.</param>
        /// <param name="end">Exclusive selection end.</param>
        public void ApplySelection(int start, int end)
        {
            _panel?.ApplySelection(start, end);
        }

        /// <summary>
        /// Gets the current selection range without applying fallbacks.
        /// </summary>
        public (int start, int end) GetCurrentSelectionRange()
        {
            return _panel != null ? _panel.GetCurrentSelectionRange() : (0, 0);
        }

        /// <summary>
        /// Registers a handler that is invoked when the selection changes.
        /// </summary>
        /// <param name="handler">Callback to attach.</param>
        public void RegisterSelectionChanged(Action handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _selectionChangedHandlers += handler;
            AttachSelectionHandlers();
        }

        /// <summary>
        /// Unregisters a previously attached selection change handler.
        /// </summary>
        /// <param name="handler">Callback to detach.</param>
        public void UnregisterSelectionChanged(Action handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _selectionChangedHandlers -= handler;
            AttachSelectionHandlers();
        }

        /// <summary>
        /// Applies the specified handler to the current markup selection.
        /// </summary>
        /// <param name="handler">Handler to wrap around the selection.</param>
        public void ApplyHandlerToSelection(TagHandlerAsset handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _panel?.ApplyHandlerToSelection(handler);
        }

        /// <summary>
        /// Applies the specified handler to the provided range within the markup content.
        /// </summary>
        /// <param name="handler">Handler to wrap around the range.</param>
        /// <param name="start">Inclusive start index.</param>
        /// <param name="end">Exclusive end index.</param>
        public void ApplyHandlerToRange(TagHandlerAsset handler, int start, int end)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _panel?.ApplyHandlerToRange(handler, start, end);
        }

        /// <summary>
        /// Sets the markup content without emitting change or selection notifications.
        /// </summary>
        /// <param name="markup">Markup string to assign.</param>
        public void SetMarkupSilently(string markup)
        {
            if (_panel == null)
            {
                return;
            }

            DetachSelectionHandlers();

            _panel.SetMarkup(markup ?? string.Empty);

            AttachSelectionHandlers();
        }

        private void BuildUi()
        {
            rootVisualElement.Clear();

            DetachSelectionHandlers();

            if (_coordinator == null || _draggedHandlerResolver == null)
            {
                rootVisualElement.Add(new HelpBox("Animation Studio coordinator unavailable. Open the Animation Studio window to reinitialize.", HelpBoxMessageType.Warning));
                _panel = null;
                return;
            }

            _panel = new AnimationStudioMarkupPanel(_coordinator, _draggedHandlerResolver);
            AttachSelectionHandlers();
            rootVisualElement.Add(_panel.Root);
        }

        private void HandlePanelSelectionChanged()
        {
            _selectionChangedHandlers?.Invoke();
        }

        private void AttachSelectionHandlers()
        {
            if (_panel == null)
            {
                return;
            }

            _panel.SelectionChanged -= HandlePanelSelectionChanged;
            _panel.SelectionChanged += HandlePanelSelectionChanged;

            if (_selectionChangedHandlers != null)
            {
                _panel.SelectionChanged -= _selectionChangedHandlers;
                _panel.SelectionChanged += _selectionChangedHandlers;
            }
        }

        private void DetachSelectionHandlers()
        {
            if (_panel == null)
            {
                return;
            }

            _panel.SelectionChanged -= HandlePanelSelectionChanged;
            if (_selectionChangedHandlers != null)
            {
                _panel.SelectionChanged -= _selectionChangedHandlers;
            }
        }
    }
}
