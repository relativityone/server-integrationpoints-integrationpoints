using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Telemetry
{
	internal sealed class JobStatisticsContainer : IJobStatisticsContainer
	{
		public long MetadataBytesTransferred { get; set; }

		public long FilesBytesTransferred { get; set; }

		public long TotalBytesTransferred { get; set; }

		public Task<long> NativesBytesRequested { get; set; }

		public List<LongTextStreamStatistics> LongTextStatistics { get; } = new List<LongTextStreamStatistics>();

		public int LongTextStreamsCount => LongTextStatistics.Count;

		public long LongTextStreamsTotalSizeInBytes => LongTextStatistics.Sum(x => x.TotalBytesRead);

		public LongTextStreamStatistics LargestLongTextStreamStatistics =>
			LongTextStatistics.OrderByDescending(x => x.TotalBytesRead).FirstOrDefault();

		public LongTextStreamStatistics SmallestLongTextStreamStatistics =>
			LongTextStatistics.OrderBy(x => x.TotalBytesRead).FirstOrDefault();

		public Task<ImagesStatistics> ImagesStatistics { get; set; }

		public long CalculateMedianLongTextStreamSize()
		{
			List<long> orderedStreamSizes = LongTextStatistics
				.Select(x => x.TotalBytesRead)
				.OrderBy(x => x)
				.ToList();

			if (orderedStreamSizes.Count == 0)
			{
				return 0;
			}
			else if (orderedStreamSizes.Count % 2 == 0)
			{
				return (orderedStreamSizes[orderedStreamSizes.Count / 2] + orderedStreamSizes[orderedStreamSizes.Count / 2] - 1) / 2;
			}
			else
			{
				return orderedStreamSizes[orderedStreamSizes.Count / 2];
			}
		}

		public Tuple<double, double> CalculateAverageLongTextStreamSizeAndTime(Func<long, bool> streamSizePredicate)
		{
			List<Tuple<double, double>> sizeAndTimeTuples = LongTextStatistics
				.Where(x => streamSizePredicate(x.TotalBytesRead))
				.Select(x => new Tuple<double, double>(
					UnitsConverter.BytesToMegabytes(x.TotalBytesRead),
					x.TotalReadTime.TotalSeconds))
				.ToList();

			if (!sizeAndTimeTuples.Any())
			{
				return new Tuple<double, double>(0, 0);
			}

			double averageSizeInMB = sizeAndTimeTuples.Select(x => x.Item1).Average();
			double averageTimeInSeconds = sizeAndTimeTuples.Select(x => x.Item2).Average();
			return new Tuple<double, double>(averageSizeInMB, averageTimeInSeconds);
		}

		public void RestoreJobStatistics(IEnumerable<IBatch> alreadyExecutedBatches)
		{
			MetadataBytesTransferred = FilesBytesTransferred = TotalBytesTransferred = 0;
			foreach (IBatch alreadyExecutedBatch in alreadyExecutedBatches)
			{
				MetadataBytesTransferred += alreadyExecutedBatch.MetadataBytesTransferred;
				FilesBytesTransferred += alreadyExecutedBatch.FilesBytesTransferred;
				TotalBytesTransferred += alreadyExecutedBatch.TotalBytesTransferred;
			}
		}
	}
}