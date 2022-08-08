using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Common.Extensions.DotNet
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();
            bool hasElements = enumerator.MoveNext();
            return !hasElements;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}
