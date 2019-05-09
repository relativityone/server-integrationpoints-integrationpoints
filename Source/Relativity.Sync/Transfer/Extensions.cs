using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
	}
}
