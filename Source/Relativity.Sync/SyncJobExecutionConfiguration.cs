using System;

namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync job execution configuration
	/// </summary>
	public sealed class SyncJobExecutionConfiguration
	{
		private int _batchSize = int.MaxValue;

		/// <summary>
		///     Defines size of a single batch
		/// </summary>
		/// <remarks>
		///     Set to <see cref="int.MaxValue" /> to ensure only one batch
		/// </remarks>
		public int BatchSize
		{
			get => _batchSize;
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException("Batch size must be greater than 0.");
				}

				_batchSize = value;
			}
		}

		/// <summary>
		///     Defines how many batches can be run in parallel
		/// </summary>
		public int NumberOfBatchRunInParallel { get; set; } = 4;

		/// <summary>
		///     Defines how many job steps can be run in parallel
		/// </summary>
		public int NumberOfStepsRunInParallel { get; set; } = 3;
	}
}