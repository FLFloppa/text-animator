using System;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets.Actions
{
    /// <summary>
    /// Produces runtime actions invoked when characters are revealed.
    /// </summary>
    public abstract class ActionAsset : ScriptableObject
    {
        /// <summary>
        /// Builds a runtime action. Parameter contains the character revealed or <c>null</c> for non-character triggers.
        /// </summary>
        public abstract Action<string> BuildAction();
    }
}
