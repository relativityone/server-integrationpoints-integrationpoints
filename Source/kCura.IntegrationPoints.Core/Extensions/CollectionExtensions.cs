using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Partitions an <see cref="IEnumerable{T}"/> into a series of <see cref="IList{T}"/> of
        /// at most <paramref name="batchSize"/> elements.
        /// </summary>
        /// <typeparam name="T">Type of elements in the source.</typeparam>
        /// <param name="fullCollection">Collection to partition.</param>
        /// <param name="batchSize">Maximum size of each partition.</param>
        /// <returns>A sequence of partitions, each of which will contains <paramref name="batchSize"/>
        /// elements. The last partition may contain less than <paramref name="batchSize"/> elements.</returns>
        internal static IEnumerable<IList<T>> SplitList<T>(this IEnumerable<T> fullCollection, int batchSize)
        {
            int actualBatchSize = batchSize;
            if (actualBatchSize <= 0)
            {
                actualBatchSize = int.MaxValue;
            }
            List<T> fullList = fullCollection.ToList();

            for (int i = 0; i < fullList.Count; i += actualBatchSize)
            {
                int endRange = Math.Min(actualBatchSize, fullList.Count - i);
                yield return fullList.GetRange(i, endRange);
            }
        }
    }
}
