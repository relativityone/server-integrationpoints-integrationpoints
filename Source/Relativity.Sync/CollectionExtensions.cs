using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync
{
	internal static class CollectionExtensions
	{
		internal static IEnumerable<IList<T>> SplitList<T>(IEnumerable<T> fullCollection, int batchSize)
		{
			if (batchSize <= 0)
			{
				batchSize = int.MaxValue;
			}
			List<T> fullList = fullCollection.ToList();

			for (int i = 0; i < fullList.Count; i += batchSize)
			{
				int endRange = Math.Min(batchSize, fullList.Count - i);
				yield return fullList.GetRange(i, endRange);
			}
		}
	}
}