using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CF_ItemTracker.API;

class CF_ItemMonitor
{
    public class InventoryChainCheckpoint
    {
        public DateTime timestamp = DateTime.UtcNow;
        public PlatformUserIdentifierAbs user;
        public int playerEntId = -1;
        public int entityId = -1;
        public int craftedItems = 0;
        public ItemStack[] bag = null;
        public ItemStack[] toolbelt = null;
        public Vector3 playerPos = Vector3.zero;
        public Vector3 entityPos = Vector3.zero;
        public Vector3i blockPos = Vector3i.zero;
        public ItemStack[] lootContainer = null;
        public bool lootContainerLocked = false;
        public List<string> messages = new List<string>();

        public InventoryChainCheckpoint(ClientInfo _cInfo)
        {
            user = _cInfo.InternalId;
            playerEntId = _cInfo.entityId;
        }
    }
    private static readonly Dictionary<int, ItemStack[]> bagLast = new Dictionary<int, ItemStack[]>();
    private static readonly Dictionary<int, ItemStack[]> toolbeltLast = new Dictionary<int, ItemStack[]>();
    private static readonly Dictionary<int, ItemStack[]> lootcontainerLast = new Dictionary<int, ItemStack[]>();
    private static readonly Dictionary<int, int> CraftedItems = new Dictionary<int, int>();

    public static bool UpdateLootContainer(ClientInfo _cInfo, TileEntity _te)
    {
        if (_te is TileEntityLootContainer lootContainer)
        {
            log.Out($"Access start loot container at {CF_TileEntity.GetPosTele(_te)}", _cInfo, CF_Log.LogLevel.Info);

            int id = _cInfo.entityId;

            if (lootcontainerLast.ContainsKey(id))
            {
                lootcontainerLast[id] = lootContainer.items;
            }
            else
            {
                lootcontainerLast.Add(id, lootContainer.items);
            }
        }

        return true;
    }
    public static void CheckIlligalItems(EntityPlayer _player, ItemStack[] _toolbelt, ItemStack[] _bag, Equipment _equipment)
    {
        if (!illigalItemsCheck || _player == null || InvalidItems == null || InvalidItems.Count < 1)
        {
            return;
        }

        int passes = 0;
        try
        {
            if (!CF_Player.GetClient(_player, out ClientInfo cInfo))
            {
                return;
            }

            if (CF_Player.GetPermission(cInfo) <= adminIgnore)
            {
                return;
            }

            passes++;

            List<string> items = new List<string>();

            if (_toolbelt != null)
            {
                foreach (ItemStack itemStack in _toolbelt)
                {
                    if (itemStack == null || itemStack.IsEmpty() || itemStack.itemValue == null || itemStack.itemValue == ItemValue.None || itemStack.itemValue.ItemClass == null)
                    {
                        continue;
                    }

                    string[] itemParts = itemStack.itemValue.ItemClass.Name.Split(':');
                    if (itemParts.Length > 0)
                    {
                        items.Add(itemParts[0]);
                    }

                    if (itemStack.itemValue.Modifications != null && itemStack.itemValue.Modifications.Length > 0)
                    {
                        foreach (ItemValue modItemValue in itemStack.itemValue.Modifications)
                        {
                            if (modItemValue != null && modItemValue != ItemValue.None && modItemValue.ItemClass != null)
                            {
                                items.Add(modItemValue.ItemClass.Name);
                            }
                        }
                    }
                }
            }

            passes++;
            if (_bag != null)
            {
                foreach (ItemStack itemStack in _bag)
                {
                    if (itemStack == null || itemStack.IsEmpty() || itemStack.itemValue == null || itemStack.itemValue == ItemValue.None || itemStack.itemValue.ItemClass == null)
                        continue;

                    string[] itemParts = itemStack.itemValue.ItemClass.Name.Split(':');
                    if (itemParts.Length > 0)
                        items.Add(itemParts[0]);

                    if (itemStack.itemValue.Modifications != null && itemStack.itemValue.Modifications.Length > 0)
                    {
                        foreach (ItemValue modItemValue in itemStack.itemValue.Modifications)
                        {
                            if (modItemValue != null && modItemValue != ItemValue.None && modItemValue.ItemClass != null)
                                items.Add(modItemValue.ItemClass.Name);
                        }
                    }
                }
            }

            passes++;
            if (_equipment != null)
            {
                foreach (ItemValue itemValue in _equipment.GetItems())
                {
                    if (itemValue == null || itemValue.ItemClass == null)
                        continue;

                    items.Add(itemValue.ItemClass.Name);

                    if (itemValue.Modifications != null && itemValue.Modifications.Length > 0)
                    {
                        foreach (ItemValue modItemValue in itemValue.Modifications)
                        {
                            if (modItemValue != null && modItemValue != ItemValue.None && modItemValue.ItemClass != null)
                                items.Add(modItemValue.ItemClass.Name);
                        }
                    }
                }
            }

            passes++;
            if (items.Count == 0)
                return;

            passes++;
            foreach (string itemname in InvalidItems)
            {
                if (!items.Contains(itemname))
                    continue;

                // TODO: BanManager.Ban(cInfo, "Autoban 65", $"Found a {itemname} in players inventory.");
            }
        }
        catch (Exception e)
        {
            log.Error($"CheckIlligalItems reported ({passes}, T: {_toolbelt != null}, B: {_bag != null}, E: {_equipment != null}): {e.Message}");
        }
    }
    public static void CheckDupedItems(ClientInfo _cInfo, int _totalCrafted, ItemStack[] _bagCurrent, ItemStack[] _toolbeltCurrent)
    {
        if (!antiDupeCheck)
        {
            return;
        }

        if (_totalCrafted < 0)
        {
            log.Error("Invalid _totalCrafted value. Expected a non-negative integer.");
            return;
        }

        int passes = 0;

        try
        {
            int entityId = _cInfo.entityId;
            EntityPlayer _player = CF_Player.GetPlayer(entityId);
            if (_player == null)
            {
                return;
            }

            if (!CraftedItems.TryGetValue(entityId, out int craftedCountOld))
            {
                CraftedItems.Add(entityId, _totalCrafted);
                bagLast.Add(entityId, _bagCurrent);
                lootcontainerLast.Add(entityId, null);
                toolbeltLast.Add(entityId, _toolbeltCurrent);
                return;
            }

            passes++;

            // Crafted items since last check
            if (_totalCrafted != craftedCountOld)
            {
                LogCrafted(_cInfo, _player, $"Crafted {_totalCrafted - craftedCountOld} items");
                CraftedItems[entityId] = _totalCrafted;
            }

            List<int> bagSlotsChecked = new List<int>();
            List<int> toolbeltSlotsChecked = new List<int>();

            passes++;

            // Loop through all bag item stacks
            for (int i = 0; i < _bagCurrent.Length; i++)
            {
                bool next = true;

                if (_bagCurrent[i].IsEmpty())
                    continue;

                if (bagLast.TryGetValue(entityId, out ItemStack[] lastBag) && lastBag.Length >= _bagCurrent.Length && _bagCurrent[i].Equals(lastBag[i]))
                    continue;

                string itemName = _bagCurrent[i].itemValue.ItemClass.Name;
                if (IgnoreDupeItems.Contains(itemName))
                    continue;

                int totalOld = GetTotalItemCount(lastBag, itemName) + GetTotalItemCount(toolbeltLast[entityId], itemName);
                int totalNew = GetTotalItemCount(_bagCurrent, itemName) + GetTotalItemCount(_toolbeltCurrent, itemName);

                if (totalOld == totalNew)
                    next = false;

                if (next && _bagCurrent[i].count == 1)
                {
                    int bagCount1 = GetTotalItemCountEqual(_bagCurrent, itemName, 1);
                    int toolbeltCount1 = GetTotalItemCountEqual(_toolbeltCurrent, itemName, 1);

                    if (bagCount1 + toolbeltCount1 > 1)
                    {
                        next = HasTotalItemCountEqualPlus(bagLast[entityId], itemName, bagCount1);
                        if (next)
                            next = HasTotalItemCountEqualPlus(_toolbeltCurrent, itemName, bagCount1);
                    }
                }

                if (!next)
                    continue;

                if (_bagCurrent[i].itemValue.HasQuality)
                {
                    for (int j = 0; j < _bagCurrent.Length; j++)
                    {
                        if (i == j || _bagCurrent[j].IsEmpty() || !_bagCurrent[i].Equals(_bagCurrent[j]) || bagSlotsChecked.Contains(j))
                            continue;

                        bagSlotsChecked.Add(i);
                        LogDupe(_cInfo, _player, "bag", "", $"{_bagCurrent[i].count}x {itemName} Q: {_bagCurrent[i].itemValue.Quality}");
                    }
                    for (int j = 0; j < _toolbeltCurrent.Length; j++)
                    {
                        if (_toolbeltCurrent[j].IsEmpty() || !_bagCurrent[i].Equals(_toolbeltCurrent[j]) || toolbeltSlotsChecked.Contains(j))
                            continue;

                        toolbeltSlotsChecked.Add(i);
                        LogDupe(_cInfo, _player, "bag", "", $"{_bagCurrent[i].count}x {itemName} Q: {_bagCurrent[i].itemValue.Quality}");
                    }
                }
                else if (_bagCurrent[i].count > 1)
                {
                    for (int j = 0; j < _bagCurrent.Length; j++)
                    {
                        if (i == j || _bagCurrent[j].IsEmpty() || !_bagCurrent[i].Equals(_bagCurrent[j]) || bagSlotsChecked.Contains(j))
                            continue;

                        bagSlotsChecked.Add(i);
                        LogDupe(_cInfo, _player, "bag", "identical to another stack", $"{_bagCurrent[i].count}x {itemName}");
                    }
                    for (int j = 0; j < _toolbeltCurrent.Length; j++)
                    {
                        if (_toolbeltCurrent[j].IsEmpty() || !_bagCurrent[i].Equals(_toolbeltCurrent[j]) || toolbeltSlotsChecked.Contains(j))
                            continue;

                        toolbeltSlotsChecked.Add(i);
                        LogDupe(_cInfo, _player, "bag", "identical to another stack", $"{_bagCurrent[i].count}x {itemName}");
                    }
                }
            }

            passes++;

            for (int i = 0; i < _toolbeltCurrent.Length; i++)
            {
                bool next = true;

                if (_toolbeltCurrent[i].IsEmpty())
                    continue;

                if (toolbeltLast.TryGetValue(entityId, out ItemStack[] lastToolbelt) && lastToolbelt.Length >= _toolbeltCurrent.Length && _toolbeltCurrent[i].Equals(lastToolbelt[i]))
                    continue;

                string itemName = _toolbeltCurrent[i].itemValue.ItemClass.Name;
                if (IgnoreDupeItems.Contains(itemName))
                    continue;

                int oldTotal = GetTotalItemCount(bagLast[entityId], itemName) + GetTotalItemCount(lastToolbelt, itemName);
                int newTotal = GetTotalItemCount(_bagCurrent, itemName) + GetTotalItemCount(_toolbeltCurrent, itemName);

                if (oldTotal == newTotal)
                    next = false;

                if (next && _toolbeltCurrent[i].count == 1)
                {
                    int bagCount1 = GetTotalItemCountEqual(_bagCurrent, itemName, 1);
                    int toolbeltCount1 = GetTotalItemCountEqual(_toolbeltCurrent, itemName, 1);

                    if (bagCount1 + toolbeltCount1 > 1)
                    {
                        next = HasTotalItemCountEqualPlus(lastToolbelt, itemName, bagCount1);
                        if (next)
                            next = HasTotalItemCountEqualPlus(bagLast[entityId], itemName, bagCount1);
                    }
                }

                if (!next)
                    continue;

                if (_toolbeltCurrent[i].itemValue.HasQuality)
                {
                    for (int j = 0; j < _toolbeltCurrent.Length; j++)
                    {
                        if (i == j || _toolbeltCurrent[j].IsEmpty() || !_toolbeltCurrent[i].Equals(_toolbeltCurrent[j]) || toolbeltSlotsChecked.Contains(j))
                            continue;

                        toolbeltSlotsChecked.Add(i);
                        LogDupe(_cInfo, _player, "toolbelt", "", $"{_toolbeltCurrent[i].count}x {itemName} Q: {_toolbeltCurrent[i].itemValue.Quality}");
                    }
                    for (int j = 0; j < _bagCurrent.Length; j++)
                    {
                        if (_bagCurrent[j].IsEmpty() || !_toolbeltCurrent[i].Equals(_bagCurrent[j]) || toolbeltSlotsChecked.Contains(j))
                            continue;

                        toolbeltSlotsChecked.Add(i);
                        LogDupe(_cInfo, _player, "toolbelt", "", $"{_toolbeltCurrent[i].count}x {itemName} Q: {_toolbeltCurrent[i].itemValue.Quality}");
                    }
                }
                else if (_toolbeltCurrent[i].count > 1)
                {
                    for (int j = 0; j < _toolbeltCurrent.Length; j++)
                    {
                        if (i == j || _toolbeltCurrent[j].IsEmpty() || !_toolbeltCurrent[i].Equals(_toolbeltCurrent[j]) || toolbeltSlotsChecked.Contains(j))
                            continue;

                        toolbeltSlotsChecked.Add(i);
                        LogDupe(_cInfo, _player, "toolbelt", "identical to another stack", $"{_toolbeltCurrent[i].count}x {itemName}");
                    }
                    for (int j = 0; j < _bagCurrent.Length; j++)
                    {
                        if (_bagCurrent[j].IsEmpty() || !_toolbeltCurrent[i].Equals(_bagCurrent[j]) || toolbeltSlotsChecked.Contains(j))
                            continue;

                        toolbeltSlotsChecked.Add(i);
                        LogDupe(_cInfo, _player, "toolbelt", "identical to another stack", $"{_toolbeltCurrent[i].count}x {itemName}");
                    }
                }
            }

            passes++;

            bagLast[entityId] = _bagCurrent;
            toolbeltLast[entityId] = _toolbeltCurrent;
        }
        catch (Exception e)
        {
            log.Error($"CheckDupedItems({passes}) reported: {e}");
        }
    }
    public static int GetTotalItemCount(ItemStack[] itemStackArray, string searchItemName)
    {
        int count = 0;

        try
        {
            foreach (ItemStack itemStack in itemStackArray)
            {
                if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.Name.Equals(searchItemName, StringComparison.OrdinalIgnoreCase))
                    count += itemStack.count;
            }
        }
        catch (Exception e)
        {
            log.Error($"GetTotalItemCount reported: {e}");
        }

        return count;
    }
    public static int GetTotalItemCountEqual(ItemStack[] itemStackArray, string searchItemName, int amount)
    {
        int count = 0;

        try
        {
            foreach (ItemStack itemStack in itemStackArray)
            {
                if (!itemStack.IsEmpty() && itemStack.count == amount && itemStack.itemValue.ItemClass.Name == searchItemName)
                    count += itemStack.count;
            }
        }
        catch (Exception e)
        {
            log.Error($"GetTotalItemCountEqual reported: {e}");
        }

        return count;
    }
    public static bool HasTotalItemCountEqualPlus(ItemStack[] itemStackArray, string searchItemName, int amount)
    {
        foreach (ItemStack itemStack in itemStackArray)
        {
            if (!itemStack.IsEmpty() && itemStack.count >= amount && itemStack.itemValue.ItemClass.Name.Equals(searchItemName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
    public static void LogCrafted(ClientInfo _cInfo, EntityPlayer _player, string msg)
    {
        log.Info($"{msg} Pos: {(int)_player.position.x} {(int)_player.position.y} {(int)_player.position.z}", _cInfo);
    }
    public static void LogDupe(ClientInfo _cInfo, EntityPlayer _player, string _location, string _detectionMethod, string _dupedItem)
    {
        log.Warn($"{_detectionMethod} in {_location} Item: {_dupedItem} Pos: {(int)_player.position.x} {(int)_player.position.y} {(int)_player.position.z}", _cInfo);
    }
}
