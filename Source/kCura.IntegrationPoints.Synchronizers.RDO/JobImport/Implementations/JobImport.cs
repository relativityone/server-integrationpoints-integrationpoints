using System.Collections;
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

		protected internal abstract TJob CreateJob();

		public abstract void Execute();

        protected virtual void OnOnError(IDictionary row)
        {
            OnError?.Invoke(row);
        }

        protected virtual void OnOnMessage(Status status)
        {
            OnMessage?.Invoke(status);
        }
    }
}
