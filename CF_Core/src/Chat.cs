using System.Collections.Generic;

public class Chat
{
    public static void Message(string msg, ClientInfo cInfo = null)
    {
        if (cInfo != null)
            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, msg + "[-]", "", false, null));
        else
        {
            List<ClientInfo> _clientList = Players.GetClients();
            if (_clientList == null || _clientList.Count < 1)
                return;

            for (int i = 0; i < _clientList.Count; i++)
            {
                ClientInfo _cInfo2 = _clientList[i];
                if (_cInfo2 == null)
                    continue;
                Message(msg, _cInfo2);
            }
        }
    }
    public static void MessageFriends(PlatformUserIdentifierAbs _UserId, string msg)
    {
        PersistentPlayerData ppd = GameManager.Instance.persistentPlayers.GetPlayerData(_UserId);

        if (ppd == null)
            return;

        if (ppd.ACL != null)
        {
            foreach (PlatformUserIdentifierAbs friend in ppd.ACL)
            {
                ClientInfo _cInfo = Players.GetClient(friend);
                if (_cInfo == null)
                    continue;
                Message(msg, _cInfo);
            }
        }
    }
}
