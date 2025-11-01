using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Animator.Subsystems;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Parsing;
using FLFloppa.TextAnimator.Parsing.Assets;
using FLFloppa.TextAnimator.Playback;
using FLFloppa.TextAnimator.Playback.Systems;
using FLFloppa.TextAnimator.Tags;
using FLFloppa.TextAnimator.Tags.Assets;
using TMPro;
using UnityEngine;

namespace FLFloppa.TextAnimator.Animator
{
    /// <summary>
    /// MonoBehaviour facade that composes parsing, timeline building, and playback for TextMeshPro targets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TextAnimator : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TMP_Text target;
        [SerializeField] private TagHandlerRegistryAsset handlerRegistry;
        [SerializeField] private ParserAsset parserAsset;

        [Header("Playback")]
        [SerializeField] [TextArea] private string initialMarkup;
        [SerializeField] private bool playOnEnable = true;

        [Header("Subsystems")]
        [SerializeField] private TextAnimatorSubsystemBundleAsset subsystemBundle;
        [SerializeField] private TextAnimatorSubsystemAsset[] subsystems = Array.Empty<TextAnimatorSubsystemAsset>();

        private readonly List<ITextAnimatorSubsystem> _resolvedSubsystems = new List<ITextAnimatorSubsystem>();
        private ICharacterAnimationPipelineFactory _pipelineFactory = null!;

        private ITagParser _parser = null!;
        private ITagHandlerFactory _handlerFactory = null!;
        private PlaybackTimelineBuilder _timelineBuilder = null!;
        private TextMeshProOutput _textOutput = null!;
        private PlaybackEngine _engine = null!;
        private PlaybackSession? _activeSession;

        /// <summary>
        /// Indicates whether a playback session is currently active.
        /// </summary>
        public bool IsPlaying => _activeSession != null && !_activeSession.IsComplete;

        private void Awake()
        {
            EnsureTargetAssigned();
            InitializeRuntime();
        }

        private void OnEnable()
        {
            if (playOnEnable && !string.IsNullOrWhiteSpace(initialMarkup))
            {
                Play(initialMarkup);
            }
        }

        private void Update()
        {
            if (_activeSession == null)
            {
                return;
            }

            var completed = _activeSession.Update(Time.deltaTime);
            if (completed)
            {
                _activeSession = null;
            }
        }

        /// <summary>
        /// Rebuilds the runtime components, e.g. after changing registry references at edit time.
        /// </summary>
        public void Reinitialize()
        {
            InitializeRuntime();
        }

        /// <summary>
        /// Begins playback of the specified markup string.
        /// </summary>
        public void Play(string markup)
        {
            if (markup == null)
            {
                markup = string.Empty;
            }

            var root = _parser.Parse(markup);
            var buildResult = _timelineBuilder.Build(root);
            _activeSession = _engine.Start(buildResult);
        }

        /// <summary>
        /// Stops the current playback session immediately.
        /// </summary>
        public void Stop()
        {
            _activeSession = null;
        }

        private void InitializeRuntime()
        {
            if (target == null)
            {
                throw new InvalidOperationException("Text Animator requires a TMP_Text target.");
            }

            ResolveSubsystems();

            _parser = parserAsset != null
                ? BuildParserFromAsset(parserAsset)
                : new CurlyBraceTagParser();
            _handlerFactory = BuildHandlerFactory();
            _timelineBuilder = new PlaybackTimelineBuilder(_handlerFactory);

            _textOutput = new TextMeshProOutput(target);
            var subsystemSnapshot = SnapshotSubsystems();
            _pipelineFactory = new CompositeCharacterAnimationPipelineFactory(subsystemSnapshot);
            _engine = new PlaybackEngine(_textOutput, _pipelineFactory);
            _activeSession = null;
        }

        private static ITagParser BuildParserFromAsset(ParserAsset asset)
        {
            var parser = asset.BuildParser();
            if (parser == null)
            {
                throw new InvalidOperationException($"Parser asset '{asset.name}' returned null parser instance.");
            }

            return parser;
        }

        private ITagHandlerFactory BuildHandlerFactory()
        {
            if (handlerRegistry == null)
            {
                return new RuntimeTagHandlerFactory(Array.Empty<TagHandlerRegistration>());
            }

            var factory = handlerRegistry.BuildFactory();
            return factory;
        }

        private void ResolveSubsystems()
        {
            _resolvedSubsystems.Clear();

            if (subsystemBundle != null && subsystemBundle.Subsystems != null)
            {
                var bundleItems = subsystemBundle.Subsystems;
                for (var i = 0; i < bundleItems.Count; i++)
                {
                    var bundleSubsystem = bundleItems[i];
                    if (bundleSubsystem == null)
                    {
                        continue;
                    }

                    _resolvedSubsystems.Add(bundleSubsystem);
                }
            }

            if (subsystems != null)
            {
                for (var i = 0; i < subsystems.Length; i++)
                {
                    var subsystemAsset = subsystems[i];
                    if (subsystemAsset == null)
                    {
                        continue;
                    }

                    _resolvedSubsystems.Add(subsystemAsset);
                }
            }

            if (_resolvedSubsystems.Count == 0)
            {
                throw new InvalidOperationException("TextAnimator requires at least one subsystem. Assign a bundle or individual subsystem assets.");
            }
        }

        private ITextAnimatorSubsystem[] SnapshotSubsystems()
        {
            var snapshot = new ITextAnimatorSubsystem[_resolvedSubsystems.Count];
            _resolvedSubsystems.CopyTo(snapshot);
            return snapshot;
        }

        private void EnsureTargetAssigned()
        {
            if (target == null)
            {
                target = GetComponent<TMP_Text>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureTargetAssigned();
        }
#endif
    }
}
