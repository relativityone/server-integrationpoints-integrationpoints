using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using kCura.Config;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.DataReaderClient;
using Moq;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi
{
    public class FakeJobImport : IJobImport
    {
        private readonly Action<FakeJobImport> _executeAction;

        public FakeJobImport(Action<FakeJobImport> executeAction)
        {
            _executeAction = executeAction;
        }
        
        public ImportSettings Settings { get; set; }
        public IDataTransferContext Context { get; set; }
        
// disable not used anywhere warning
#pragma warning disable CS0067
        public event IImportNotifier.OnCompleteEventHandler OnComplete;
        public event IImportNotifier.OnFatalExceptionEventHandler OnFatalException;
        public event IImportNotifier.OnProgressEventHandler OnProgress;
        public event IImportNotifier.OnProcessProgressEventHandler OnProcessProgress;
        public event OnErrorEventHandler OnError;
        public event OnMessageEventHandler OnMessage;
#pragma warning restore CS0067

        internal void Complete(int numberOfDocuments, int numberOfItemLevelErrors = 0)
        {
            for (long i = 1; i < numberOfDocuments; i++)
            {
                OnProgress?.Invoke(i);
            }

            for (long i = 0; i < numberOfItemLevelErrors; i++)
            {
	            OnError?.Invoke(Mock.Of<IDictionary>());
            }

            OnProgress?.Invoke(1);
            ConstructorInfo[] constructorInfos = typeof(JobReport).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            JobReport jobReport = (JobReport) constructorInfos.First().Invoke(new object[0]);

            MethodInfo totalRecordsSetter = typeof(JobReport).Properties()
                .First(x => x.Name == nameof(JobReport.TotalRows))
                .SetMethod;

            totalRecordsSetter.Invoke(jobReport, new object[] {numberOfDocuments});
            
            OnComplete?.Invoke(jobReport);
        }

        public void RegisterEventHandlers()
        {
        }

        public void Execute()
        {
            // IAPI always starts with 1 row progress
            OnProgress?.Invoke(1);
            _executeAction?.Invoke(this);
        }
    }
}