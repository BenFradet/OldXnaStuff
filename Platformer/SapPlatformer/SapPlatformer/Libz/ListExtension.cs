using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ListExtension
{
    public static void Shuffle<T>(this List<T> list)
    {
        Random rand = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
