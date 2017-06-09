using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class LoggingMediatorForTestsFactory
	{
		private readonly IAPILog _apiLog;
		private readonly IJobHistoryErrorService _historyErrorService;

		public LoggingMediatorForTestsFactory(IAPILog apiLog, IJobHistoryErrorService historyErrorService)
		{
			_apiLog = apiLog;
			_historyErrorService = historyErrorService;
		}

		public ICompositeLoggingMediator Create()
		{
			var compositeLoggingMediator = new CompositeLoggingMediator();
			var exportLoggingMediator = new ExportLoggingMediator(_apiLog);
			var jobErrorLoggingMediator = new JobErrorLoggingMediator(_historyErrorService);

			compositeLoggingMediator.AddLoggingMediator(exportLoggingMediator);
			compositeLoggingMediator.AddLoggingMediator(jobErrorLoggingMediator);
			compositeLoggingMediator.AddLoggingMediator(new TestLoggingMediator());

			return compositeLoggingMediator;
		}
	}
}