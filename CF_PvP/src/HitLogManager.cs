using Epic.OnlineServices.Presence;
using GameSparks.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HitLogManager
{
    public static void Add(EntityPlayer _playerA, EntityPlayer _playerV, int _strength, bool _fatal, ItemValue _attackingItem, float _distance, Utils.EnumHitDirection _hitDirection, EnumBodyPartHit _hitBodyPart)
    {
        int physicalArmorRating = (int)_playerV.equipment.GetTotalPhysicalArmorRating(_playerA, _attackingItem);
        int health = _playerV.Health;
        string itemName = _attackingItem.ItemClass.GetLocalizedItemName();
    }
}
