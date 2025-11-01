using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Document;

namespace FLFloppa.TextAnimator.Tags
{
    /// <summary>
    /// Provides character modifiers for visual effects.
    /// </summary>
    public interface ICharacterModifierProvider : ITagHandler
    {
        ISubsystemModifier CreateModifier(TagNode node);
    }
}