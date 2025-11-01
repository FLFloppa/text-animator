using FLFloppa.TextAnimator.Parsing.Assets;
using UnityEngine;

namespace FLFloppa.TextAnimator.Parsing
{
    /// <summary>
    /// ScriptableObject that builds <see cref="CurlyBraceTagParser"/> instances.
    /// </summary>
    [CreateAssetMenu(fileName = "CurlyBraceTagParser", menuName = "FLFloppa/Text Animator/Parser/Curly Brace", order = 610)]
    public sealed class CurlyBraceTagParserAsset : ParserAsset
    {
        public override ITagParser BuildParser()
        {
            var parser = new CurlyBraceTagParser();
            ValidateParser(parser);
            return parser;
        }
    }
}
