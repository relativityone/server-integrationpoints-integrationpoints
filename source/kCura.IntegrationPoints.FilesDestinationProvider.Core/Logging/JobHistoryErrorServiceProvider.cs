using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	/// <summary>
	///     This is temporary solution for issue with resolving JobHistoryErrorService from Container.
	///     More details:
	///     JobHistoryErrorService is marked as LifestyleTransient in ServiceInstaller.
	///     ExportWorker and LoggingMediatorFactory require same instance of it.
	///     I wanted to avoid modifying ServiceInstaller code, so I create Provider, which Lifestyle is set in AgentInstaller.
	/// </summary>
	public class JobHistoryErrorServiceProvider
	{
		public JobHistoryErrorServiceProvider(JobHistoryErrorService jobHistoryErrorService)
		{
			JobHistoryErrorService = jobHistoryErrorService;
		}

		public JobHistoryErrorService JobHistoryErrorService { get; private set; }
	}
}