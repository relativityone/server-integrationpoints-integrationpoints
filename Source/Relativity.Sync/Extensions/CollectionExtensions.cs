using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Extensions
{
	internal static class CollectionExtensions
	{
		public static IDictionary<TKey, IEnumerable<TValue>> ToKeySubsequenceDictionary<TKey, TValue>(
			this IEnumerable<TKey> keyCollection, IEnumerable<TValue> valueCollection, Func<TValue, TKey> subsequenceSelector)
		{
			var collectionLookup = valueCollection.ToLookup(x => subsequenceSelector(x), x => x);
			return keyCollection.ToDictionary(x => x, x => collectionLookup[x]);
		}
	}
}
