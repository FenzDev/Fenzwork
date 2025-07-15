using System.Collections.Generic;
using System;

namespace Fenzwork.Systems.GUI
{
    public static class ListExtensions
    {
        public static void SwapRemoveAt<T>(this List<T> list, int index)
        {
            int last = list.Count - 1;
            if (index < 0 || index > last)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index != last)
                list[index] = list[last];

            list.RemoveAt(last);
        }
    }
}