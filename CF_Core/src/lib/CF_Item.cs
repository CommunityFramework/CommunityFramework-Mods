using UnityEngine;

public class CF_Item
{
    public static bool GiveItem(ClientInfo _cInfo, ItemValue _itemValue, bool allow_ground, int _count, out bool on_ground)
    {
        on_ground = false;

        bool cantake = CanTakeItem(_cInfo, _itemValue, _count);

        if (!allow_ground && !cantake)
            return false;

        EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
        {
            entityClass = EntityClass.FromString("item"),
            id = EntityFactory.nextEntityID++,
            itemStack = new ItemStack(_itemValue, _count),
            pos = GameManager.Instance.World.Players.dict[_cInfo.entityId].position,
            rot = new Vector3(20f, 0f, 20f),
            lifetime = 60f,
            belongsPlayerId = _cInfo.entityId
        });

        GameManager.Instance.World.SpawnEntityInWorld(entityItem);

        // Don't use inventory check, it's not sync
        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
        GameManager.Instance.World.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
        return true;
    }
    public static bool CanTakeItem(ClientInfo _cInfo, ItemValue _itemValue, int _count)
    {
        if (_cInfo == null)
            return false;

        return CanTakeItem(CF_Player.GetPlayer(_cInfo), _itemValue, _count);
    }
    public static bool CanTakeItem(EntityPlayer _player, ItemValue _itemValue, int _count)
    {
        if (_player == null)
            return false;

        if (!_player.IsSpawned() || !_player.IsAlive())
            return false;

        if (_player.bag.CanTakeItem(new ItemStack(_itemValue, _count)) || _player.inventory.CanTakeItem(new ItemStack(_itemValue, _count)))
            return true;

        return false;
    }
}