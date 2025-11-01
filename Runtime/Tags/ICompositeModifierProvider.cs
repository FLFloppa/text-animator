using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Document;

namespace FLFloppa.TextAnimator.Tags
{
    /// <summary>
    /// Provides multiple character modifiers from a single tag, effectively expanding into multiple independent effects.
    /// </summary>
    public interface ICompositeModifierProvider : ICharacterModifierProvider
    {
        /// <summary>
        /// Creates multiple modifiers for the given tag node.
        /// </summary>
        /// <param name="node">The tag node containing attributes and context.</param>
        /// <returns>Collection of modifiers to apply, in application order.</returns>
        IEnumerable<ISubsystemModifier> CreateModifiers(TagNode node);
    }
}
