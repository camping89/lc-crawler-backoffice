using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LC.Crawler.BackOffice.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
    {
        T[] array = null;
        var count = 0;
        foreach (var item in source)
        {
            array ??= new T[size];
            array[count] = item;
            count++;
            if (count == size)
            {
                yield return new ReadOnlyCollection<T>(array);
                array = null;
                count = 0;
            }
        }

        if (array != null)
        {
            Array.Resize(ref array, count);
            yield return new ReadOnlyCollection<T>(array);
        }
    }
    
    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> source)
    {
        return source != null && source.Any();
    }
}