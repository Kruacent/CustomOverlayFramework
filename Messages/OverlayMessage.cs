namespace CustomOverlayFramework.Messages
{
    public abstract class OverlayMessage
    {
        public abstract MessageType Type { get; }
        protected abstract string SerializeContent();
        public string Serialize()
        {
            return $"{Type.Name}|{SerializeContent()}";
        }
    }
}
