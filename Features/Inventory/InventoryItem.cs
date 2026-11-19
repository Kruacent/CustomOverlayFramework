using System;

namespace CustomOverlayFramework.Features.Inventory
{
    public sealed class InventoryItem
    {
        public ushort Serial { get; }
        public ItemType ItemType { get; }
        public string CustomItemName { get; }
        public bool IsCustom => CustomItemName != null;

        public InventoryItem(ushort serial, ItemType itemType, string customItemName)
        {
            Serial = serial;
            ItemType = itemType;
            CustomItemName = customItemName;
        }

        public string Serialize()
        {
            return $"{Serial}:{ItemType}:{(IsCustom ? 1 : 0)}:{CustomItemName}";
        }
    }
}
