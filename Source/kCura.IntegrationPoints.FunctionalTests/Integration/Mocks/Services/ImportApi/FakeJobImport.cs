using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.DataReaderClient;
using kCura.Utility.Extensions;
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

        internal void Complete(long maxTransferredItems = Int64.MaxValue, long numberOfItemLevelErrors = 0, bool useDataReader = true)
        {
	        ConstructorInfo[] constructorInfos = typeof(JobReport).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
	        JobReport jobReport = (JobReport)constructorInfos.First().Invoke(new object[0]);

	        
	        long i = 0;

            for (; i < numberOfItemLevelErrors && i < maxTransferredItems; i++)
	        {
		        OnProgress?.Invoke(i);
		        OnError?.Invoke(Mock.Of<IDictionary>());
		        jobReport.ErrorRows.Add(new JobReport.RowError(i, "", i.ToString()));
		        
		        if (useDataReader && !Context.DataReader.Read())
		        {
			        break;
		        }
            }

	        while ((!useDataReader || Context.DataReader.Read()) && i < maxTransferredItems)
	        {
		        OnProgress?.Invoke(i++);
	        }

            MethodInfo totalRecordsSetter = typeof(JobReport).Properties()
                .First(x => x.Name == nameof(JobReport.TotalRows))
                .SetMethod;

            totalRecordsSetter.Invoke(jobReport, new object[] {(int)i});
            
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