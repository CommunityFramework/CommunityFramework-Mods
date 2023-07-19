using System;
using System.Collections.Generic;

public class Claims
{
    public static bool GetClaimInfoAtPos(Vector3i pos, out PersistentPlayerData ppd, out Vector3i claimPos)
    {
        ppd = null;
        claimPos = pos;

        int claimRadius = (GamePrefs.GetInt(EnumGamePrefs.LandClaimSize) - 1) / 2;

        Dictionary<Vector3i, PersistentPlayerData> _LCBs = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;
        foreach (KeyValuePair<Vector3i, PersistentPlayerData> _ppdKV in _LCBs)
        {
            if (Math.Abs(pos.x - _ppdKV.Key.x) > claimRadius)
                continue;
            if (Math.Abs(pos.z - _ppdKV.Key.z) > claimRadius)
                continue;

            ppd = _ppdKV.Value;
            claimPos = _ppdKV.Key;
            break;
        }

        return ppd != null;
    }
}