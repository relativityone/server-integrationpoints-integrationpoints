using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Transfer
{
	internal static class Extensions
	{
		public static void Extend<TKey, TValue>(
			this IDictionary<TKey, TValue> src,
			IEnumerable<TKey> keys,
			IEnumerable<TValue> values)
		{
			using (IEnumerator<TKey> keysEnumerator = keys.GetEnumerator())
			using (IEnumerator<TValue> valuesEnumerator = values.GetEnumerator())
			{
				while (keysEnumerator.MoveNext() && valuesEnumerator.MoveNext())
				{
					src.Add(keysEnumerator.Current, valuesEnumerator.Current);
				}
			}
		}

		public static Dictionary<TKey, TValue> MapOnto<TKey, TValue>(this IEnumerable<TKey> keys,
			IEnumerable<TValue> values)
		{
			Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();
			using (IEnumerator<TKey> keyEnumerator = keys.GetEnumerator())
			using (IEnumerator<TValue> valueEnumerator = values.GetEnumerator())
			{
				while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
				{
					TKey key = keyEnumerator.Current;
					TValue value = valueEnumerator.Current;

					if (key == null)
					{
						throw new ArgumentException("Collection of keys should not contain null values", nameof(keys));
					}
					d.Add(key, value);
				}
			}

			return d;
		}
	}
}
