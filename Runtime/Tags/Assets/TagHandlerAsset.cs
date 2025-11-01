using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Parameters;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags.Assets
{
    /// <summary>
    /// Base ScriptableObject for configuring runtime tag handlers with identifier aliases.
    /// </summary>
    public abstract class TagHandlerAsset : ScriptableObject
    {
        [SerializeField] private List<string> tagIdentifiers = new List<string>();

        /// <summary>
        /// Enumerates all aliases to register for the handler.
        /// </summary>
        public IReadOnlyList<string> TagIdentifiers => tagIdentifiers;

        /// <summary>
        /// Builds a runtime handler instance.
        /// </summary>
        public abstract ITagHandler BuildHandler();

        protected static TParameter RequireParameter<TParameter>(BaseParameterDefinitionAsset asset)
            where TParameter : class, IParameterDefinition
        {
            if (asset == null)
            {
                throw new InvalidOperationException("Parameter definition asset is not assigned.");
            }

            var runtime = asset.BuildRuntimeDefinition() as TParameter;
            if (runtime == null)
            {
                throw new InvalidOperationException($"Parameter asset '{asset.name}' does not produce definition of type {typeof(TParameter).Name}.");
            }

            return runtime;
        }
    }
}
