using kCura.IntegrationPoints.Core.Services.JobHistory;

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
        public JobHistoryErrorServiceProvider(IJobHistoryErrorService jobHistoryErrorService)
        {
            JobHistoryErrorService = jobHistoryErrorService;
        }

        public virtual IJobHistoryErrorService JobHistoryErrorService { get; private set; }
    }
}
