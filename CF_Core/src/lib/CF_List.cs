using System.Collections.Generic;

public class CF_List
{
    public static void Shuffle<T>(ref List<T> list)
    {
        int count = list.Count;
        GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
        while (count > 1)
        {
            --count;
            int index = gameRandom.RandomRange(0, count) % count;
            T obj = list[index];
            list[index] = list[count];
            list[count] = obj;
        }
    }
}
