namespace kCura.IntegrationPoints.Core.Contracts.BatchReporter
{
	public class NullBatchReporter : IBatchReporter
	{
		public event StatisticsUpdate OnStatisticsUpdate
		{
			add { }
			remove { }
		}

		public event BatchCompleted OnBatchComplete
		{
			add { }
			remove { }
		}

		public event BatchSubmitted OnBatchSubmit
		{
			add { }
			remove { }
		}

		public event BatchCreated OnBatchCreate
		{
			add { }
			remove { }
		}

		public event StatusUpdate OnStatusUpdate
		{
			add { }
			remove { }
		}

		public event JobError OnJobError
		{
			add { }
			remove { }
		}

		public event RowError OnDocumentError
		{
			add { }
			remove { }
		}
	}
}
