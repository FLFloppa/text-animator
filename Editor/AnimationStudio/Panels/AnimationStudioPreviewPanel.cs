using System;
using FLFloppa.EditorHelpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using IMGUIContainer = UnityEngine.UIElements.IMGUIContainer;

namespace FLFloppa.TextAnimator.Editor.AnimationStudio.Panels
{
    internal sealed class AnimationStudioPreviewPanel
    {
        public AnimationStudioPreviewPanel(Action onPreviewGui)
        {
            if (onPreviewGui == null)
            {
                throw new ArgumentNullException(nameof(onPreviewGui));
            }

            Root = new VisualElement
            {
                style =
                {
                    flexGrow = 1f,
                    flexDirection = FlexDirection.Column,
                    minHeight = 0f,
                    paddingTop = 0f,
                    paddingBottom = 0f,
                    paddingLeft = 0f,
                    paddingRight = 0f
                }
            };

            Container = new VisualElement
            {
                style =
                {
                    flexGrow = 1f,
                    backgroundColor = Color.black,
                    marginTop = 0f,
                    marginBottom = 0f,
                    marginLeft = 0f,
                    marginRight = 0f
                }
            };
            Root.Add(Container);

            Host = new IMGUIContainer(onPreviewGui)
            {
                style =
                {
                    flexGrow = 1f,
                    marginTop = 0f,
                    marginBottom = 0f,
                    marginLeft = 0f,
                    marginRight = 0f
                }
            };
            Container.Add(Host);
        }

        public VisualElement Root { get; }

        public VisualElement Container { get; }

        public IMGUIContainer Host { get; }
    }
}
