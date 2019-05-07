using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync
{
	internal static class CollectionExtensions
	{
		internal static IEnumerable<IList<T>> SplitList<T>(IEnumerable<T> fullCollection, int batchSize)
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