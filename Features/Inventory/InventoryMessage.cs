using CustomOverlayFramework.Messages;
using System.Collections.Generic;

namespace CustomOverlayFramework.Features.Inventory
{
    public sealed class InventoryMessage : OverlayMessage
    {
        public override MessageType Type => MessageType.Inventory;
        public bool IsAlive { get; }
        public ushort HeldSerial { get; }
        public IReadOnlyList<InventoryItem> Items { get; }

        public InventoryMessage(bool isAlive, ushort heldSerial, IReadOnlyList<InventoryItem> items)
        {
            IsAlive = isAlive;
            HeldSerial = heldSerial;
            Items = items;
        }

        public static InventoryMessage Empty()
        {
            return new InventoryMessage(false, 0, new List<InventoryItem>());
        }

        protected override string SerializeContent()
        {
            List<string> parts = new List<string>();

            parts.Add(IsAlive ? "1" : "0");
            parts.Add(HeldSerial.ToString());

            foreach (InventoryItem item in Items)
            {
                parts.Add(item.Serialize());
            }

            return string.Join("|", parts);
        }
    }
}
