using System.Collections.Generic;
using UnityEngine;

namespace Talespin
{
    public static class ArrayExtensions
    {
        public static void Shuffle<T>(this T[] array)
        {
            int total = array.Length;
            for (int i = 0; i < total; i++)
            {
                T tmp = array[i];
                int j = Random.Range(i, total);
                array[i] = array[j];
                array[j] = tmp;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int total = list.Count;
            for (int i = 0; i < total; i++)
            {
                T tmp = list[i];
                int j = Random.Range(i, total);
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        public static string Join<T>(T[] array, string connector = ", ")
        {
            string output = "";
            int total = array.Length;
            for (int i = 0; i < total; i++)
            {
                output += (i + 1 < total ? connector : "") + array[i].ToString();
            }
            return output;
        }

        public static void Set<T>(this T[] array, T defaultVaue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultVaue;
            }
        }
    }
}