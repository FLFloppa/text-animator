using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Animator.Subsystems
{
    /// <summary>
    /// ScriptableObject that groups multiple <see cref="TextAnimatorSubsystemAsset"/> instances for easy reuse.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TextAnimatorSubsystemBundle",
        menuName = "FLFloppa/Text Animator/Subsystem Bundle",
        order = 500)]
    public sealed class TextAnimatorSubsystemBundleAsset : ScriptableObject
    {
        [SerializeField] private TextAnimatorSubsystemAsset[] subsystems = System.Array.Empty<TextAnimatorSubsystemAsset>();

        /// <summary>
        /// Gets the subsystems contained in this bundle.
        /// </summary>
        public IReadOnlyList<TextAnimatorSubsystemAsset> Subsystems => subsystems;
    }
}
