namespace Relativity.Sync
{
	/// <summary>
	/// Data structure to hold progress for each batch
	/// </summary>
	internal class SyncBatchProgress
	{
		private int _itemsProcessed;
		private int _itemsFailed;
		private bool _completed;
		
		private readonly object _itemProcessedLock = new object();
		private readonly object _itemFailedLock = new object();
		private readonly object _completedLock = new object();
		
		/// <summary>
		/// Creates the instance for batch with given Id.
		/// 
		/// All public properties setters are thread-safe
		/// </summary>
		/// <param name="batchId">Id of monitored batch</param>
		/// <param name="totalItems">Total items count in batch</param>
		/// <param name="failedItemsCount">Count of already failed items in the batch</param>
		/// <param name="transferredItemsCount">Count of already transferred items in batch</param>
		public SyncBatchProgress(int batchId, int runId, int totalItems, int failedItemsCount, int transferredItemsCount)
		{
			BatchId = batchId;
			RunId = runId;
			TotalItems = totalItems;
			ItemsAlreadyFailed = failedItemsCount;
			ItemsAlreadyProcessed = transferredItemsCount;
		}

		public int ItemsAlreadyProcessed { get; }

		public int ItemsProcessed
		{
			get => _itemsProcessed;
			set 
			{
				lock (_itemProcessedLock)
				{
					if (value >= 0)
					{
						_itemsProcessed = value;
					}
				}
			}
		}

		public int ItemsAlreadyFailed { get; }

		public int ItemsFailed
		{
			get => _itemsFailed;
			set
			{
				lock (_itemFailedLock)
				{
					_itemsFailed = value;
				}
			}
		}

		public bool Completed
		{
			get => _completed;
			set
			{
				lock (_completedLock)
				{
					_completed = value;
				}
			}
		}

		public int BatchId { get; }
		public int RunId { get; }

		public int TotalItems { get; }
	}
}