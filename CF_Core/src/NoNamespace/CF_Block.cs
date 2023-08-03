using System.Collections.Generic;
using UnityEngine;

public class CF_Block
{
    public static bool IsDoorOpen(byte _metadata) => ((uint)_metadata & 1U) > 0U;
    public static string IsDoorOpenX(byte _metadata) => ((uint)_metadata & 1U) > 0U ? "Opened" : "Closed";
    public static bool IsDoorLocked(byte _metadata) => ((uint)_metadata & 4U) > 0U;
    public static string IsDoorLockedX(byte _metadata) => ((uint)_metadata & 4U) > 0U ? "Locked" : "Unlocked";
    public static void RemoveBlock(Vector3i pos)
    {
        if (GameManager.Instance.World.GetBlock(pos).type != BlockValue.Air.type)
            GameManager.Instance.World.SetBlockRPC(pos, BlockValue.Air);
    }
    public static bool GiveBlockBackRPC(PlatformUserIdentifierAbs _UserId, BlockChangeInfo newBlockInfo)
    {
        World world = GameManager.Instance.World;
        ClientInfo cInfo = CF_Player.GetClient(_UserId);
        EntityPlayer player = CF_Player.GetPlayer(cInfo);
        string blockName = newBlockInfo.blockValue.Block.GetBlockName();

        try
        {
            ItemValue _itemValue = new ItemValue(ItemClass.GetItem(blockName).type, 1, 1);
            EntityItem entity = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData()
            {
                entityClass = EntityClass.FromString("item"),
                id = EntityFactory.nextEntityID++,
                itemStack = new ItemStack(_itemValue, 1),
                pos = world.Players.dict[cInfo.entityId].position,
                rot = new Vector3(20f, 0.0f, 20f),
                lifetime = 60f,
                belongsPlayerId = cInfo.entityId
            });
            world.SpawnEntityInWorld(entity);
            if (player.bag.CanTakeItem(new ItemStack(_itemValue, 1)) || player.inventory.CanTakeItem(new ItemStack(_itemValue, 1)))
            {
                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entity.entityId, cInfo.entityId));
                world.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Despawned);
            }
            return true;
        }
        catch { Log.Error($"Error giving {blockName} to player {_UserId}."); }

        return false;
    }
    public static bool RemoveTileEntityAtRPC(BlockChangeInfo newBlockInfo)
    {
        World world = GameManager.Instance.World;
        BlockValue blockValue = world.GetBlock(newBlockInfo.pos);

        if (newBlockInfo.blockValue.Block.HasTileEntity && blockValue.type == BlockValue.Air.type)
        {
            try
            {
                Chunk chunkFromWorldPos = world.ChunkClusters[newBlockInfo.clrIdx].GetChunkFromWorldPos(newBlockInfo.pos) as Chunk;
                chunkFromWorldPos.RemoveTileEntityAt<TileEntity>(world, World.toBlock(newBlockInfo.pos));
                GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(new HashSetLong() { chunkFromWorldPos.Key });
                return true;
            }
            catch { Log.Error($"Error removing TileEntity at {newBlockInfo.pos}"); }

            return false;
        }

        try
        {
            world.SetBlockRPC(newBlockInfo.clrIdx, newBlockInfo.pos, blockValue);
            return true;
        }
        catch { Log.Error($"Error restoring Block at {newBlockInfo.pos}"); }

        return false;
    }
}
