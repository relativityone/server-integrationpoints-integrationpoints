using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Split the elements of a sequence into chunks of size at most <paramref name="size"/>.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> whose elements to chunk.</param>
        /// <param name="size">Maximum size of each chunk.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static IEnumerable<List<TSource>> SplitList<TSource>(this IEnumerable<TSource> source, int size)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(source));
            }

            if (!source.Any())
            {
                return Enumerable.Empty<List<TSource>>();
            }

            return ChunkIterator(source, size);
        }

        private static IEnumerable<List<TSource>> ChunkIterator<TSource>(IEnumerable<TSource> source, int size)
        {
            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    var chunk = new List<TSource> { e.Current };
                    for (int i = 1; i < size && e.MoveNext(); i++)
                    {
                        chunk.Add(e.Current);
                    }

                    yield return chunk;
                }
            }

        }
    }
}
