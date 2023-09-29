using System;
using System.Collections.Generic;

namespace MonitorLib.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> collection, Action<T> action)
        {
            var enumerator = collection.GetEnumerator();
            while(enumerator.MoveNext())
            {
                action?.Invoke(enumerator.Current);
            }
        }
    }
}