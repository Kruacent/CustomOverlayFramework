using System;
using System.Collections.Generic;

namespace CustomOverlayFramework.Messages
{
    public sealed partial class MessageType
    {
        private static HashSet<string> _names;
        public string Name { get; }

        private MessageType(string name)
        {
            _names ??= new HashSet<string>();

            if (!_names.Add(name))
            {
                throw new InvalidOperationException($"Message type already exists {name}");
            }

            Name = name;
        }
    }
}
