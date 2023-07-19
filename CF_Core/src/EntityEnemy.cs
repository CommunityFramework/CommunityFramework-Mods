using System.Collections.Generic;

public class EntityEnemy
{
    public static bool IsHostileMobInRange(Vector3i pos, int radius)
    {
        if (radius < 0)
            return false;

        List<Entity> Entities = GameManager.Instance.World.Entities.list;

        for (int j = 0; j < Entities.Count; j++)
        {
            if (Entities[j] == null)
                continue;

            if (Entities[j].IsClientControlled() || Entities[j].IsDead())
                continue;

            string _tags = Entities[j].EntityClass.Tags.ToString();

            if (_tags.Contains("zombie") || _tags.Contains("hostile"))
            {
                float distance = (pos.x - Entities[j].position.x) *
                    (pos.x - Entities[j].position.x) +
                    (pos.z - Entities[j].position.z) *
                    (pos.z - Entities[j].position.z);

                if (distance > radius * radius)
                    continue;

                return true;
            }
        }

        return false;
    }
    public static int DespawnZombiesRadius(Vector3i pos, int radius)
    {
        if (radius < 0)
            return -1;

        List<Entity> Entities = GameManager.Instance.World.Entities.list;
        List<int> markedforDelete = new List<int>();

        for (int j = 0; j < Entities.Count; j++)
        {
            if (Entities[j] == null)
                continue;

            if (Entities[j].IsClientControlled() || Entities[j].IsDead())
                continue;

            string _tags = Entities[j].EntityClass.Tags.ToString();

            if (_tags.Contains("zombie") || _tags.Contains("hostile"))
            {
                float distance = (pos.x - Entities[j].position.x) *
                    (pos.x - Entities[j].position.x) +
                    (pos.z - Entities[j].position.z) *
                    (pos.z - Entities[j].position.z);

                if (distance > radius * radius)
                    continue;

                markedforDelete.Add(Entities[j].entityId);
            }
        }

        foreach (int entityId in markedforDelete)
        {
            GameManager.Instance.World.RemoveEntity(entityId, EnumRemoveEntityReason.Killed);
        }

        return markedforDelete.Count;
    }
}