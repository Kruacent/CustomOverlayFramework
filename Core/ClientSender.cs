using CustomOverlayFramework.Messages;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomOverlayFramework.Core
{
    public sealed class ClientSender
    {
        private readonly Dictionary<string, StreamWriter> _authentifiedUser = new Dictionary<string, StreamWriter>();

        public IEnumerable<string> GetConnectedUserId() {
            return _authentifiedUser.Keys;
        }

        public ClientSender() { }

        internal void Register(string userId, StreamWriter writer)
        {
            _authentifiedUser.Add(userId, writer);
        }

        internal void Unregister(string userId)
        {
            _authentifiedUser.Remove(userId);
        }

        public bool IsConnected(string userId)
        {
            return _authentifiedUser.ContainsKey(userId);
        }

        public bool Send(string userId, OverlayMessage message)
        {
            if (!_authentifiedUser.TryGetValue(userId, out StreamWriter writer))
            {
                return false;
            }

            try
            {
                writer.WriteLine(message.Serialize());
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }
}
