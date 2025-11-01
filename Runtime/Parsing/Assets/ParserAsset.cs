using System;
using FLFloppa.TextAnimator.Parsing;
using UnityEngine;

namespace FLFloppa.TextAnimator.Parsing.Assets
{
    /// <summary>
    /// Base ScriptableObject responsible for producing runtime parser instances.
    /// </summary>
    public abstract class ParserAsset : ScriptableObject
    {
        /// <summary>
        /// Builds a runtime parser instance.
        /// </summary>
        public abstract ITagParser BuildParser();

        protected static void ValidateParser(ITagParser parser)
        {
            if (parser == null)
            {
                throw new InvalidOperationException("Parser asset produced a null parser instance.");
            }
        }
    }
}
