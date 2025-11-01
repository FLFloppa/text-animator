using System;
using System.Collections.Generic;
using UnityEngine;

namespace FLFloppa.TextAnimator.Tags
{
    /// <summary>
    /// Runtime implementation of <see cref="ITagHandlerFactory"/> backed by a dictionary.
    /// </summary>
    public sealed class RuntimeTagHandlerFactory : ITagHandlerFactory
    {
        private readonly Dictionary<string, ITagHandler> _handlers;

        public RuntimeTagHandlerFactory(IEnumerable<TagHandlerRegistration> registrations)
        {
            if (registrations == null)
            {
                throw new ArgumentNullException(nameof(registrations));
            }

            _handlers = new Dictionary<string, ITagHandler>(StringComparer.OrdinalIgnoreCase);
            foreach (var registration in registrations)
            {
                if (string.IsNullOrWhiteSpace(registration.Identifier) || registration.Handler == null)
                {
                    continue;
                }

                _handlers[registration.Identifier] = registration.Handler;
            }
        }

        public ITagHandler CreateHandler(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            _handlers.TryGetValue(identifier, out var handler);
            return handler;
        }
    }
}