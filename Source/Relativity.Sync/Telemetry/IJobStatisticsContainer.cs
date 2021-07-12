using Relativity.Sync.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Telemetry
{
	internal interface IJobStatisticsContainer
	{
		/// <summary>
		/// Size of the metadata in bytes, that was successfully pushed.
		/// </summary>
		long MetadataBytesTransferred { get; set; }

		/// <summary>
		/// Size of the files in bytes, that was successfully pushed.
		/// </summary>
		long FilesBytesTransferred { get; set; }

		/// <summary>
		/// Size of the job in bytes, including files and metadata, that was successfully pushed.
		/// </summary>
		long TotalBytesTransferred { get; set; }

		/// <summary>
		/// Size of the natives that was requested to push.
		/// </summary>
		Task<long> NativesBytesRequested { get; set; }

		/// <summary>
		/// Collection of long text statistics.
		/// </summary>
		List<LongTextStreamStatistics> LongTextStatistics { get; }

		/// <summary>
		/// Number of long text streams in current job.
		/// </summary>
		int LongTextStreamsCount { get; }

		/// <summary>
		/// Total size (in bytes) of all long text streams in current job.
		/// </summary>
		long LongTextStreamsTotalSizeInBytes { get; }

		/// <summary>
		/// Returns statistics of the largest long text stream.
		/// </summary>
		LongTextStreamStatistics LargestLongTextStreamStatistics { get; }
		
		/// <summary>
		/// Returns statistics of the smallest long text stream.
		/// </summary>
		LongTextStreamStatistics SmallestLongTextStreamStatistics { get; }

		/// <summary>
		/// Calculates median value of long text streams size.
		/// </summary>
		/// <returns></returns>
		long CalculateMedianLongTextStreamSize();

		/// <summary>
		/// Size of the images that was requested to push.
		/// </summary>
		Task<ImagesStatistics> ImagesStatistics { get; set; }

		/// <summary>
		/// Calculates average long text stream size and time.
		/// </summary>
		/// <returns>Tuple of double values, where Item1 is the average size (in megabytes) and Item2 is the average time (in seconds).</returns>
		Tuple<double, double> CalculateAverageLongTextStreamSizeAndTime(Func<long, bool> streamSizePredicate);

		void RestoreJobStatistics(IEnumerable<IBatch> alreadyExecutedBatches);
	}
}