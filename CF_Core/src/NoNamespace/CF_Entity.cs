public class CF_Entity
{
    public static string GetPosTele(TileEntity _te)
    {
        if (_te.EntityId == -1)
        {
            Vector3i pos = _te.ToWorldPos();
            return CF_Vector3.FormatTeleport(pos);
        }

        Entity entity = GetEntity(_te.EntityId);
        if (entity == null)
            return "entity not found";

        return $"{entity.position.x} {entity.position.y} {entity.position.z}";
    }
    public static Entity GetEntity(int _entityId) => GameManager.Instance.World.Entities.dict.ContainsKey(_entityId)? GameManager.Instance.World.Entities.dict[_entityId] : null;
}
