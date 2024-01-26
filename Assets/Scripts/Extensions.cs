using System.Collections.Generic;
using Random = System.Random;

public static class Extensions
{
    public static Random random = new Random();
    
    public static void Shuffle<T>(this IList<T> list, int seed = 0)
    {
        var i = list.Count;
        while (i > 1)
        {
            i--;
            var j = random.Next(i + 1);
            (list[j], list[i]) = (list[i], list[j]);
        }
    }
}