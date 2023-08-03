public class CF_TileEntity
{
    public static string GetPosTele(TileEntity _te)
    {
        if (_te.EntityId == -1)
        {
            Vector3i pos = _te.ToWorldPos();
            return $"{pos.x} {pos.y} {pos.z}";
        }

        Entity entity = Entities.GetEntity(_te.EntityId);
        if (entity == null)
            return "entity not found";

        return $"{entity.position.x} {entity.position.y} {entity.position.z}";
    }
}