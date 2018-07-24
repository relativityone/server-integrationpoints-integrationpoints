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
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IIntegrationPointProviderTypeService _integrationPointProviderTypeService;

		public LoggingMediatorFactory(JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider, IHelper helper,
			IMessageService messageService, ICaseServiceContext caseServiceContext,
			IIntegrationPointProviderTypeService integrationPointProviderTypeService)
		{
			_historyErrorService = jobHistoryErrorServiceProvider.JobHistoryErrorService;
			_helper = helper;
			_messageService = messageService;
			_caseServiceContext = caseServiceContext;
			_integrationPointProviderTypeService = integrationPointProviderTypeService;
		}

		public ICompositeLoggingMediator Create()
		{
			var compositeLoggingMediator = new CompositeLoggingMediator();
			var apiLog = _helper.GetLoggerFactory().GetLogger().ForContext<ExportProcessRunner>();
			var exportLoggingMediator = new ExportLoggingMediator(apiLog);
			var jobErrorLoggingMediator = new JobErrorLoggingMediator(_historyErrorService);
			
			compositeLoggingMediator.AddLoggingMediator(exportLoggingMediator);
			compositeLoggingMediator.AddLoggingMediator(jobErrorLoggingMediator);
			compositeLoggingMediator.AddLoggingMediator(new StatisticsLoggingMediator(_messageService, _historyErrorService, _caseServiceContext, _integrationPointProviderTypeService));

			return compositeLoggingMediator;
		}
	}
}