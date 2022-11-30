using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync
{
    internal static class CollectionExtensions
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

        /// <summary>
        /// Iterates over the given sequences of keys and values, adding each key/value pair
        /// to the given dictionary. Iteration stops once either sequence ends.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary's keys.</typeparam>
        /// <typeparam name="TValue">Type of the dictionary's values.</typeparam>
        /// <param name="src">Dictionary to which pairs will be added. A copy will not be returned; this reference will be mutated.</param>
        /// <param name="keys">Sequence of keys to add. If any key in the sequence is null, an <see cref="ArgumentException"/> will be thrown.</param>
        /// <param name="values">Sequence of values to add.</param>
        internal static void AddMany<TKey, TValue>(
            this IDictionary<TKey, TValue> src,
            IEnumerable<TKey> keys,
            IEnumerable<TValue> values)
        {
            using (IEnumerator<TKey> keysEnumerator = keys.GetEnumerator())
            using (IEnumerator<TValue> valuesEnumerator = values.GetEnumerator())
            {
                while (keysEnumerator.MoveNext() && valuesEnumerator.MoveNext())
                {
                    TKey key = keysEnumerator.Current;
                    TValue value = valuesEnumerator.Current;

                    if (key == null)
                    {
                        throw new ArgumentException("Collection of keys should not contain null values", nameof(keys));
                    }

                    src.Add(key, value);
                }
            }
        }

        /// <summary>
        /// Async version of Select.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in input sequence.</typeparam>
        /// <typeparam name="TResult">Selector's return type.</typeparam>
        /// <param name="source">Sequence to select.</param>
        /// <param name="selector">Function that returns a task.</param>
        /// <returns>Task that completes when all selectors complete.</returns>
        internal static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            IEnumerable<Task<TResult>> results = source.Select(selector);
            return await Task.WhenAll(results).ConfigureAwait(false);
        }

        /// <summary>
        /// Yields an infinite sequence composed of the given value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">Value to yield. The same exact object will be yielded each time.</param>
        /// <returns>An infinite sequence, each value of which is <paramref name="value"/>. If you iterate
        /// over this sequence without a guard, it will loop indefinitely.</returns>
        internal static IEnumerable<T> Repeat<T>(this T value)
        {
            while (true)
            {
                yield return value;
            }
        }

        /// <summary>
        /// Executes action on every element of sequence.
        /// It's equivalent of foreach loop.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in sequence.</typeparam>
        /// <param name="source">IEnumerable to iterate over.</param>
        /// <param name="action">Action to invoke on each element.</param>
        internal static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                action(item);
            }
        }
    }
}
