using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Telemetry
{
	internal sealed class JobStatisticsContainer : IJobStatisticsContainer
	{
		public long TotalBytesTransferred { get; set; }

		public Task<long> NativesBytesRequested { get; set; }

		public List<LongTextStreamStatistics> LongTextStatistics { get; } = new List<LongTextStreamStatistics>();

		public int LongTextStreamsCount => LongTextStatistics.Count;

		public long LongTextStreamsTotalSizeInBytes => LongTextStatistics.Sum(x => x.TotalBytesRead);

		public LongTextStreamStatistics LargestLongTextStreamStatistics =>
			LongTextStatistics.OrderByDescending(x => x.TotalBytesRead).First();

		public LongTextStreamStatistics SmallestLongTextStreamStatistics =>
			LongTextStatistics.OrderBy(x => x.TotalBytesRead).First();

		public long MedianLongTextStreamSizeInBytes => LongTextStreamsTotalSizeInBytes / LongTextStreamsCount;

		public void AppendLongTextStreamStatistics(LongTextStreamStatistics streamStatistics)
		{
			LongTextStatistics.Add(streamStatistics);
		}
	}
}