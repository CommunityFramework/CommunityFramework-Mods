public class Teams
{
    public static bool IsFriendOf(PersistentPlayerData ppd, int entityId)
    {
        if (ppd.ACL == null || ppd.ACL.Count < 1)
            return false; // No friends

        PersistentPlayerData _ppd2 = Players.GetPersistent(entityId);
        if (_ppd2 == null)
        {
            Log.Error($"IsFriendOf(): Could not get PersistentPlayerData for {entityId}");
            return false;
        }
        if (_ppd2.ACL == null || _ppd2.ACL.Count < 1)
            return false; // Big looser with no friends

        if (ppd.ACL.Contains(_ppd2.UserIdentifier) && _ppd2.ACL.Contains(ppd.UserIdentifier)) // We need to check both, if only one contains the other it's an invite
            return true; // They are friends

        return false;
    }
    public static bool IsFriendOf(PlatformUserIdentifierAbs _UserId1, PlatformUserIdentifierAbs _UserId2)
    {
        PersistentPlayerData ppd1 = Players.GetPersistent(_UserId1);
        if (ppd1 == null)
        {
            Log.Error($"IsFriendOf(): Could not get PersistentPlayerData for {_UserId1}");
            return false;
        }

        if (ppd1.ACL == null || ppd1.ACL.Count < 1)
            return false; // No friends

        PersistentPlayerData _ppd2 = Players.GetPersistent(_UserId2);
        if (_ppd2 == null)
        {
            Log.Error($"IsFriendOf(): Could not get PersistentPlayerData for {_UserId2}");
            return false;
        }

        if (_ppd2.ACL == null || _ppd2.ACL.Count < 1)
            return false;

        if (ppd1.ACL.Contains(_ppd2.UserIdentifier) && _ppd2.ACL.Contains(ppd1.UserIdentifier)) // We need to check both, if only one contains the other it's an invite
            return true; // They are friends

        return false;
    }
    public static bool IsFriendOf(PersistentPlayerData ppd, PlatformUserIdentifierAbs _UserId)
    {
        if (ppd.ACL == null || ppd.ACL.Count < 1)
            return false; // No friends

        PersistentPlayerData _ppd2 = Players.GetPersistent(_UserId);
        if (_ppd2 == null)
        {
            Log.Error($"IsFriendOf(): Could not get PersistentPlayerData for {_UserId}");
            return false;
        }

        if (_ppd2.ACL == null || _ppd2.ACL.Count < 1)
            return false;

        if (ppd.ACL.Contains(_ppd2.UserIdentifier) && _ppd2.ACL.Contains(ppd.UserIdentifier)) // We need to check both, if only one contains the other it's an invite
            return true; // They are friends

        return false;
    }
    public static bool IsFriendOf(PersistentPlayerData ppd, PersistentPlayerData ppd2)
    {
        if (ppd.ACL == null || ppd.ACL.Count < 1)
            return false; // No friends

        if (ppd2.ACL == null || ppd2.ACL.Count < 1)
            return false; // Big looser with no friends

        if (ppd.ACL.Contains(ppd2.UserIdentifier) && ppd2.ACL.Contains(ppd.UserIdentifier)) // We need to check both, if only one contains the other it's an invite
            return true; // They are friends

        return false;
    }
    public static bool HasPartyWith(EntityPlayer player, EntityPlayer player2)
    {
        if (player.Party == null)
            return false;

        if (player2.Party == null)
            return false;

        if (!player.Party.ContainsMember(player) || !player.Party.ContainsMember(player2))
            return false;

        return true;
    }
}
