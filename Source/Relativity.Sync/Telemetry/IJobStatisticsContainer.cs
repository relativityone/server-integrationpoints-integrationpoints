using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Telemetry
{
	internal interface IJobStatisticsContainer
	{
		/// <summary>
		/// Size of the job in bytes, including files and metadata, that was successfully pushed.
		/// </summary>
		long TotalBytesTransferred { get; set; }

		/// <summary>
		/// Size of the natives that was requested to push.
		/// </summary>
		Task<long> NativesBytesRequested { get; set; }

		List<LongTextStreamStatistics> LongTextStatistics { get; }
		void AppendLongTextStreamStatistics(LongTextStreamStatistics streamStatistics);
		int LongTextStreamsCount { get; }
		long LongTextStreamsTotalSizeInBytes { get; }
		LongTextStreamStatistics LargestLongTextStreamStatistics { get; }
		LongTextStreamStatistics SmallestLongTextStreamStatistics { get; }
		long MedianLongTextStreamSizeInBytes { get; }
	}
}