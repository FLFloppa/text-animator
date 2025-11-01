using System;
using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets.Actions
{
    /// <summary>
    /// Executes multiple child actions sequentially.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionComposite", menuName = "FLFloppa/Text Animator/Actions/Composite", order = 610)]
    public sealed class ActionCompositeAsset : ActionAsset
    {
        [SerializeField] private List<ActionAsset> actions = new List<ActionAsset>();

        public IReadOnlyList<ActionAsset> Actions => actions;

        public override Action<string> BuildAction()
        {
            var builtActions = new List<Action<string>>(actions.Count);
            foreach (var asset in actions)
            {
                if (asset == null)
                {
                    continue;
                }

                var runtimeAction = asset.BuildAction();
                if (runtimeAction != null)
                {
                    builtActions.Add(runtimeAction);
                }
            }

            if (builtActions.Count == 0)
            {
                return _ => { };
            }

            return character =>
            {
                for (var i = 0; i < builtActions.Count; i++)
                {
                    builtActions[i]?.Invoke(character);
                }
            };
        }
    }
}
