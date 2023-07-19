using System.Collections.Generic;

public class Utilz
{
    /*
    public static bool ContainsAny(this string haystack, params string[] needles)
    {
        foreach (string needle in needles)
        {
            if (haystack.Contains(needle))
                return true;
        }

        return false;
    }
    */
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
