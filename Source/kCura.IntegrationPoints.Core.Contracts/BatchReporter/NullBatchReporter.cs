namespace kCura.IntegrationPoints.Core.Contracts.BatchReporter
{
	public class NullBatchReporter : IBatchReporter
	{
		private StatisticsUpdate _onStatisticsUpdate;
		private BatchCompleted _onBatchComplete;
		private BatchSubmitted _onBatchSubmit;
		private BatchCreated _onBatchCreate;
		private StatusUpdate _onStatusUpdate;
		private JobError _onJobError;
		private RowError _onDocumentError;

		public event StatisticsUpdate OnStatisticsUpdate
		{
			add
			{
				_onStatisticsUpdate += value;
			}
			remove
			{
				_onStatisticsUpdate -= value;
			}
		}

		public event BatchCompleted OnBatchComplete
		{
			add
			{
				_onBatchComplete += value;
			}
			remove
			{
				_onBatchComplete -= value;
			}
		}

		public event BatchSubmitted OnBatchSubmit
		{
			add
			{
				_onBatchSubmit += value;
			}
			remove
			{
				_onBatchSubmit -= value;

			}
		}

		public event BatchCreated OnBatchCreate
		{
			add
			{
				_onBatchCreate += value;
			}
			remove
			{
				_onBatchCreate -= value;

			}
		}

		public event StatusUpdate OnStatusUpdate
		{
			add
			{
				_onStatusUpdate += value;
			}
			remove
			{
				_onStatusUpdate -= value;

			}
		}

		public event JobError OnJobError
		{
			add
			{
				_onJobError += value;
			}
			remove
			{
				_onJobError -= value;

			}
		}

		public event RowError OnDocumentError
		{
			add
			{
				_onDocumentError += value;
			}
			remove
			{
				_onDocumentError -= value;

			}
		}
	}
}
