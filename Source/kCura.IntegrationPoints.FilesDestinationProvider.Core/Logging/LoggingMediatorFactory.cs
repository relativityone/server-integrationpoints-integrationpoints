using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	public class LoggingMediatorFactory
	{
		private readonly IHelper _helper;
		private readonly IMessageService _messageService;
		private readonly IJobHistoryErrorService _historyErrorService;
		private readonly IProviderTypeService _providerTypeService;
		private readonly ICaseServiceContext _caseServiceContext;

		public LoggingMediatorFactory(JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider, IHelper helper, IMessageService messageService, IProviderTypeService providerTypeService, ICaseServiceContext caseServiceContext)
		{
			_historyErrorService = jobHistoryErrorServiceProvider.JobHistoryErrorService;
			_helper = helper;
			_messageService = messageService;
			_providerTypeService = providerTypeService;
			_caseServiceContext = caseServiceContext;
		}

		public ICompositeLoggingMediator Create()
		{
			var compositeLoggingMediator = new CompositeLoggingMediator();
			var apiLog = _helper.GetLoggerFactory().GetLogger().ForContext<ExportProcessRunner>();
			var exportLoggingMediator = new ExportLoggingMediator(apiLog);
			var jobErrorLoggingMediator = new JobErrorLoggingMediator(_historyErrorService);
			
			compositeLoggingMediator.AddLoggingMediator(exportLoggingMediator);
			compositeLoggingMediator.AddLoggingMediator(jobErrorLoggingMediator);
			compositeLoggingMediator.AddLoggingMediator(new StatisticsLoggingMediator(_messageService, _providerTypeService, _historyErrorService, _caseServiceContext));

			return compositeLoggingMediator;
		}
	}
}