using System.Collections;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
    public abstract class JobImport<TJob> : IJobImport where TJob : class, IImportNotifier, IImportBulkArtifactJob, new()
    {
        private TJob _job;
        private readonly IAPILog _logger;

        protected JobImport(ImportSettings importSettings, IImportAPI importApi, IAPILog logger)
        {
            ImportSettings = importSettings;
            ImportApi = importApi;
            _logger = logger;
        }

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

        public event IImportNotifier.OnProcessProgressEventHandler OnProcessProgress
        {
            add { ImportJob.OnProcessProgress += value; }
            remove { ImportJob.OnProcessProgress -= value; }
        }

        public virtual event OnErrorEventHandler OnError;
        public virtual event OnMessageEventHandler OnMessage;

        protected TJob ImportJob
        {
            get
            {
                return _job ?? (_job = CreateJob());
            }
        }

        protected IImportAPI ImportApi { get; }

        protected ImportSettings ImportSettings { get; }

        public abstract void RegisterEventHandlers();

        protected internal abstract TJob CreateJob();

        public abstract void Execute();

        protected void ExportErrorFile()
        {
            if (!string.IsNullOrEmpty(ImportSettings.ErrorFilePath))
            {
                ImportJob.ExportErrorFile(ImportSettings.ErrorFilePath);
            }
        }

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
