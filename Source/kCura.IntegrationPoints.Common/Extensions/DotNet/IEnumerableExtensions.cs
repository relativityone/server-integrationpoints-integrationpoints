using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Common.Extensions.DotNet
{
	public static class IEnumerableExtensions
	{
		public static bool IsNullOrEmpty(this IEnumerable enumerable) // TODO unit tests
		{
			if (enumerable == null)
			{
				return true;
			}

			IEnumerator enumerator = enumerable.GetEnumerator();
			bool hasElements = enumerator.MoveNext();
			return !hasElements;
		}

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) // TODO unit tests
		{
			return enumerable == null || !enumerable.Any();
		}
	}
}
