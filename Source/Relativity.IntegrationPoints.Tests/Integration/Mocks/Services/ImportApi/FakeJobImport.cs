using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.DataReaderClient;
using kCura.Utility.Extensions;
using Relativity.IntegrationPoints.Tests.Integration.Models;

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

        internal void Complete(int numberOfDocuments)
        {
            for (long i = 0; i < numberOfDocuments; i++)
            {
                OnProgress?.Invoke(i);
            }
            
            ConstructorInfo[] constructorInfos = typeof(JobReport).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            JobReport jobReport = (JobReport) constructorInfos.First().Invoke(new object[0]);

            MethodInfo totalRecordsSetter = typeof(JobReport).Properties()
                .First(x => x.Name == nameof(JobReport.TotalRows))
                .SetMethod;

            totalRecordsSetter.Invoke(jobReport, new object[] {numberOfDocuments});
            
            // to make sure that all OnProgress are processed
            Task.Delay(500).GetAwaiter().GetResult();
            OnComplete?.Invoke(jobReport);
        }

        public void RegisterEventHandlers()
        {
        }

        public void Execute()
        {
            // IAPI always starts with 1
            OnProgress?.Invoke(1);
            _executeAction?.Invoke(this);
        }
    }
}