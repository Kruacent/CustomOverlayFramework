using CustomOverlayFramework.Core;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomOverlayFramework.Features.Inventory
{
    public class InventoryModule
    {
        private readonly Config _config;
        private readonly ClientSender _clientSender;
        private readonly Dictionary<string, string> _lastSent = new Dictionary<string, string>();
        private CoroutineHandle _coroutine;

        public InventoryModule(Config config, ClientSender clientSender)
        {
            _config = config;
            _clientSender = clientSender;
        }

        public void Start()
        {
            _coroutine = Timing.RunCoroutine(SyncLoop());
        }

        public void Stop()
        {
            Timing.KillCoroutines(_coroutine);
            _lastSent.Clear();
        }

        private IEnumerator<float> SyncLoop()
        {
            while (true)
            {
                try
                {
                    SyncOverlay();
                }
                catch (Exception e)
                {
                    Log.Error($"Inventory sync : {e.Message}");
                }

                yield return Timing.WaitForSeconds(1f / _config.TicksPerSecond);
            }
        }

        private void SyncOverlay()
        {
            foreach (string userId in _clientSender.GetConnectedUserId())
            {
                if (Player.TryGet(userId, out Player player))
                {
                    Sync(userId, player);
                }
            }

            DeleteDisconnectedPlayer();
        }

        private void Sync(string userId, Player player)
        {
            InventoryMessage message = CreateMessage(player);
            string line = message.Serialize();

            if (_lastSent.TryGetValue(userId, out string last) && last == line)
            {
                return;
            }

            if (_clientSender.Send(userId, message))
            {
                _lastSent[userId] = line;
                Log.Debug($"Inventory sent to {userId} : {line}");
            }
        }

        private InventoryMessage CreateMessage(Player player)
        {
            if (!HasInventory(player))
            {
                return InventoryMessage.Empty();
            }

            List<InventoryItem> items = new List<InventoryItem>();

            foreach (Item item in player.Items)
            {
                if (item != null)
                {
                    items.Add(new InventoryItem(item.Serial, item.Type, GetCustomItemName(item)));
                }
            }

            ushort heldSerial = player.CurrentItem?.Serial ?? 0;
            return new InventoryMessage(true, heldSerial, items);
        }

        private static bool HasInventory(Player player)
        {
            return player.IsAlive && player.IsHuman;
        }

        private static string GetCustomItemName(Item item)
        {
            string name = null;
            if(CustomItem.TryGet(item, out CustomItem customItem))
            {
                name = customItem.Name;
            }

            return name;
        }

        private void DeleteDisconnectedPlayer()
        {
            foreach (string userId in _lastSent.Keys.ToList())
            {
                if (!_clientSender.IsConnected(userId))
                {
                    _lastSent.Remove(userId);
                }
            }
        }
    }
}
