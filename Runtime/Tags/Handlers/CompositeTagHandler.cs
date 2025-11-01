using System;
using System.Collections.Generic;
using FLFloppa.TextAnimator.Characters;
using FLFloppa.TextAnimator.Document;

namespace FLFloppa.TextAnimator.Tags.Handlers
{
    /// <summary>
    /// Combines multiple character modifier providers into a single composite tag handler that expands into multiple independent modifiers.
    /// </summary>
    public sealed class CompositeTagHandler : ICompositeModifierProvider
    {
        private readonly ICharacterModifierProvider[] _childHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTagHandler"/> class.
        /// </summary>
        /// <param name="childHandlers">Collection of child tag handlers to combine.</param>
        public CompositeTagHandler(IEnumerable<ICharacterModifierProvider> childHandlers)
        {
            if (childHandlers == null)
            {
                throw new ArgumentNullException(nameof(childHandlers));
            }

            var handlersList = new List<ICharacterModifierProvider>(childHandlers);
            if (handlersList.Count == 0)
            {
                throw new ArgumentException("Composite tag handler must contain at least one child handler.", nameof(childHandlers));
            }

            _childHandlers = handlersList.ToArray();
        }

        /// <inheritdoc />
        public ISubsystemModifier CreateModifier(TagNode node)
        {
            // Return first modifier for compatibility, but CreateModifiers should be used instead
            if (_childHandlers.Length > 0)
            {
                return _childHandlers[0].CreateModifier(node);
            }
            return null;
        }

        /// <inheritdoc />
        public IEnumerable<ISubsystemModifier> CreateModifiers(TagNode node)
        {
            var modifiers = new List<ISubsystemModifier>(_childHandlers.Length);

            for (var i = 0; i < _childHandlers.Length; i++)
            {
                var modifier = _childHandlers[i].CreateModifier(node);
                if (modifier != null)
                {
                    modifiers.Add(modifier);
                }
            }

            return modifiers;
        }
    }
}
