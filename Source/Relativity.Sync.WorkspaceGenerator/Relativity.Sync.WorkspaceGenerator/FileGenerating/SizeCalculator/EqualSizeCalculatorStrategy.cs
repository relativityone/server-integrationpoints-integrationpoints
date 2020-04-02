using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerating.SizeCalculator
{
	public class EqualFileSizeCalculatorStrategy : IFileSizeCalculatorStrategy
	{
		public IEnumerable<long> GetSizesInBytes(int desiredFilesCount, long totalSizeInMB)
		{
			long totalSizeInBytes = totalSizeInMB * 1024 * 1024;
			long singleFileSizeInBytes = totalSizeInBytes / desiredFilesCount;

			return Enumerable.Repeat(singleFileSizeInBytes, desiredFilesCount);
		}
	}
}