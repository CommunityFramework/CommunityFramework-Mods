public class RandomStuff
{
    public static int Rnd(int e) => GameManager.Instance.World.GetGameRandom().RandomRange(0, e);
}
