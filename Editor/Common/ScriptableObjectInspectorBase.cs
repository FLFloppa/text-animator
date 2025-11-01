using System;
using System.Collections.Generic;
using FLFloppa.EditorHelpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FLFloppa.TextAnimator.Editor.Common
{
    /// <summary>
    /// Shared base class for ScriptableObject inspectors using the FLFloppa inspector UI helpers.
    /// </summary>
    /// <typeparam name="TAsset">ScriptableObject type implemented by the inspector.</typeparam>
    internal abstract class ScriptableObjectInspectorBase<TAsset> : UnityEditor.Editor
        where TAsset : ScriptableObject
    {
        private readonly List<Action> _cleanupActions = new List<Action>();
        private readonly Dictionary<string, List<Action<SerializedProperty>>> _trackedProperties = new Dictionary<string, List<Action<SerializedProperty>>>();
        private bool _updateRegistered;

        public sealed override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();

            var root = InspectorUi.Layout.CreateRoot();
            BuildInspector(root);

            root.RegisterCallback<DetachFromPanelEvent>(_ => Cleanup());
            return root;
        }

        /// <summary>
        /// Builds the custom inspector UI. Derived classes should add their content to <paramref name="root"/>.
        /// </summary>
        protected abstract void BuildInspector(VisualElement root);

        /// <summary>
        /// Registers a callback that is invoked when Unity performs an undo/redo operation.
        /// </summary>
        protected void RegisterUndoRedoCallback(Action callback)
        {
            if (callback == null)
            {
                return;
            }

            void Handler()
            {
                if (serializedObject?.targetObject == null)
                {
                    return;
                }

                serializedObject.UpdateIfRequiredOrScript();
                callback();
            }

            Undo.undoRedoPerformed += Handler;
            _cleanupActions.Add(() => Undo.undoRedoPerformed -= Handler);
        }

        /// <summary>
        /// Tracks a serialized property and executes <paramref name="onChanged"/> whenever the property value may have changed.
        /// </summary>
        protected void TrackPropertyValue(SerializedProperty property, Action<SerializedProperty> onChanged)
        {
            if (property == null || onChanged == null)
            {
                return;
            }

            if (!_trackedProperties.TryGetValue(property.propertyPath, out var callbacks))
            {
                callbacks = new List<Action<SerializedProperty>>();
                _trackedProperties[property.propertyPath] = callbacks;
            }

            callbacks.Add(onChanged);
            EnsureUpdateHook();
            RegisterUndoRedoCallback(() => InvokeTrackedCallbacks(property.propertyPath));
        }

        /// <summary>
        /// Adds a summary label with the provided text to the parent element.
        /// </summary>
        protected static void AddSummaryLabel(VisualElement parent, string text)
        {
            parent.Add(InspectorUi.Controls.CreateSummaryLabel(text));
        }

        /// <summary>
        /// Formats a reference summary, handling missing assignments gracefully.
        /// </summary>
        protected static string FormatReference(string label, UnityEngine.Object reference)
        {
            return reference == null ? $"{label}: Not assigned" : $"{label}: {reference.name}";
        }

        /// <summary>
        /// Gets the strongly typed asset currently inspected.
        /// </summary>
        protected TAsset TargetAsset => (TAsset)target;

        private void EnsureUpdateHook()
        {
            if (_updateRegistered)
            {
                return;
            }

            EditorApplication.update += OnInspectorUpdate;
            _cleanupActions.Add(() => EditorApplication.update -= OnInspectorUpdate);
            _updateRegistered = true;
        }

        private void OnInspectorUpdate()
        {
            if (serializedObject?.targetObject == null)
            {
                return;
            }

            serializedObject.UpdateIfRequiredOrScript();

            foreach (var kvp in _trackedProperties)
            {
                var property = serializedObject.FindProperty(kvp.Key);
                if (property == null)
                {
                    continue;
                }

                var callbacks = kvp.Value;
                for (var i = 0; i < callbacks.Count; i++)
                {
                    callbacks[i]?.Invoke(property);
                }
            }
        }

        private void InvokeTrackedCallbacks(string propertyPath)
        {
            if (!_trackedProperties.TryGetValue(propertyPath, out var callbacks) || callbacks.Count == 0)
            {
                return;
            }

            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                return;
            }

            for (var i = 0; i < callbacks.Count; i++)
            {
                callbacks[i]?.Invoke(property);
            }
        }

        private void Cleanup()
        {
            if (serializedObject?.targetObject != null)
            {
                serializedObject.ApplyModifiedProperties();
            }

            foreach (var cleanup in _cleanupActions)
            {
                cleanup();
            }

            _cleanupActions.Clear();
            _trackedProperties.Clear();

            if (_updateRegistered)
            {
                EditorApplication.update -= OnInspectorUpdate;
                _updateRegistered = false;
            }
        }
    }
}
