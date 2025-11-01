using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Output;
using FLFloppa.TextAnimator.Playback.Systems;
using UnityEngine;

namespace FLFloppa.TextAnimator.Animator.Subsystems
{
    /// <summary>
    /// Base class for ScriptableObject-driven text animator subsystems.
    /// </summary>
    public abstract class TextAnimatorSubsystemAsset : ScriptableObject, ITextAnimatorSubsystem
    {
        [SerializeField]
        private TextOutputApplicatorAsset[] applicators = Array.Empty<TextOutputApplicatorAsset>();

        private static readonly TextOutputApplicatorAsset[] EmptyApplicators = Array.Empty<TextOutputApplicatorAsset>();

        /// <inheritdoc />
        public abstract TextAnimatorSubsystemKey Key { get; }

        /// <inheritdoc />
        public abstract ICharacterAnimationPipelineSegment CreatePipelineSegment();

        /// <summary>
        /// Gets the configured applicator assets for this subsystem.
        /// </summary>
        protected IReadOnlyList<TextOutputApplicatorAsset> ApplicatorAssets => applicators ?? EmptyApplicators;

        /// <inheritdoc />
        public virtual IEnumerable<ITextOutputApplicator> GetApplicators()
        {
            var hasApplicators = false;

            if (applicators != null)
            {
                for (var i = 0; i < applicators.Length; i++)
                {
                    var applicator = applicators[i];
                    if (applicator == null)
                    {
                        continue;
                    }

                    hasApplicators = true;
                    yield return applicator;
                }
            }

            if (hasApplicators)
            {
                yield break;
            }

            foreach (var fallback in CreateDefaultApplicators())
            {
                if (fallback == null)
                {
                    continue;
                }

                yield return fallback;
            }
        }

        /// <summary>
        /// Creates default applicators used when none are configured on the asset.
        /// </summary>
        /// <returns>The fallback applicators.</returns>
        protected virtual IEnumerable<ITextOutputApplicator> CreateDefaultApplicators()
        {
            yield break;
        }

        /// <summary>
        /// Creates a runtime applicator instance that is not persisted to disk.
        /// </summary>
        /// <typeparam name="TApplicator">The applicator asset type.</typeparam>
        /// <returns>The runtime applicator instance.</returns>
        protected static TApplicator CreateRuntimeApplicator<TApplicator>()
            where TApplicator : TextOutputApplicatorAsset
        {
            var instance = ScriptableObject.CreateInstance<TApplicator>();
            instance.hideFlags = HideFlags.DontSave;
            return instance;
        }
    }
}
