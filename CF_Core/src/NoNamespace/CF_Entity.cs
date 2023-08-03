public class Entities
{
    public static Entity GetEntity(int _entityId) => GameManager.Instance.World.Entities.dict.ContainsKey(_entityId)? GameManager.Instance.World.Entities.dict[_entityId] : null;
}
