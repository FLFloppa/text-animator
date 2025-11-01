using UnityEngine;

namespace FLFloppa.TextAnimator.Characters.States
{
    /// <summary>
    /// Represents transform-related state for a single character.
    /// </summary>
    public sealed class TransformCharacterState
    {
        public Vector3 PositionOffset { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public Vector3 Scale { get; set; } = Vector3.one;
        public bool UseGroupPivot { get; set; }
        public int GroupStartIndex { get; set; } = -1;
        public int GroupEndIndex { get; set; } = -1;

        /// <summary>
        /// Resets the state to default values.
        /// </summary>
        public void Reset()
        {
            PositionOffset = Vector3.zero;
            Rotation = Quaternion.identity;
            Scale = Vector3.one;
            UseGroupPivot = false;
            GroupStartIndex = -1;
            GroupEndIndex = -1;
        }
    }
}
