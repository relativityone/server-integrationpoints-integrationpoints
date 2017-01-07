using kCura.Relativity.DataReaderClient;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public abstract class JobImport<TJob> : IJobImport where TJob : IImportNotifier, new()
	{
		private TJob _job;

		public event IImportNotifier.OnCompleteEventHandler OnComplete
		{
			add {  ImportJob.OnComplete += value; }
			remove { ImportJob.OnComplete -= value; }
		}

		public event IImportNotifier.OnFatalExceptionEventHandler OnFatalException
		{
			add {  ImportJob.OnFatalException += value; }
			remove { ImportJob.OnFatalException -= value; }
		}

		public event IImportNotifier.OnProgressEventHandler OnProgress
		{
			add { ImportJob.OnProgress += value; }
			remove { ImportJob.OnProgress -= value; }
		}

		//TODO: create registration method for those event (you can see some examples in StatisticsLoggingMediator)
		public virtual event OnErrorEventHandler OnError;
		public virtual event OnMessageEventHandler OnMessage;


		protected TJob ImportJob
		{
			get
			{
				if (_job == null)
				{
					_job = CreateJob();
				}
				return _job;
			}
		}

		public abstract void RegisterEventHandlers();

		protected abstract TJob CreateJob();

		public abstract void Execute();
	}
}
