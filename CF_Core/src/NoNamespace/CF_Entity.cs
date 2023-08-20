public class CF_Entity
{
    public static Entity GetEntity(int _entityId) => GameManager.Instance.World.Entities.dict.ContainsKey(_entityId)? GameManager.Instance.World.Entities.dict[_entityId] : null;
}
