namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync configuration
	/// </summary>
	public sealed class SyncConfiguration
	{
		/// <summary>
		///     Defines size of a single batch
		/// </summary>
		public int BatchSize { get; set; } = 1000;

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