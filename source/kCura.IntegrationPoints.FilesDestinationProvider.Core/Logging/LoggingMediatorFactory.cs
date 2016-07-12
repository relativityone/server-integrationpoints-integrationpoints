using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	public class LoggingMediatorFactory
	{
		private readonly IHelper _helper;
		private readonly IJobHistoryErrorService _historyErrorService;

		public LoggingMediatorFactory(JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider, IHelper helper)
		{
			_historyErrorService = jobHistoryErrorServiceProvider.JobHistoryErrorService;
			_helper = helper;
		}

		public ILoggingMediator Create()
		{
			var compositeLoggingMediator = new CompositeLoggingMediator();
			var apiLog = _helper.GetLoggerFactory().GetLogger().ForContext<ExportProcessRunner>();
			var exportLoggingMediator = new ExportLoggingMediator(apiLog);
			var jobErrorLoggingMediator = new JobErrorLoggingMediator(_historyErrorService);

			compositeLoggingMediator.AddLoggingMediator(exportLoggingMediator);
			compositeLoggingMediator.AddLoggingMediator(jobErrorLoggingMediator);

			return compositeLoggingMediator;
		}
	}
}