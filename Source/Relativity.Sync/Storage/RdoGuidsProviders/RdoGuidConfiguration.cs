using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class RdoGuidConfiguration : IRdoGuidConfiguration
    {

        public RdoGuidConfiguration(IConfiguration cache)
        {
            JobHistory = new JobHistoryRdoGuidsProvider(cache);
            JobHistoryStatus = new JobHistoryStatusGuidsProvider(cache);
            JobHistoryError = new JobHistoryErrorGuidsProvider(cache);
            DestinationWorkspace = new DestinationWorkspaceTagGuidProvider(cache);
        }

        public IJobHistoryRdoGuidsProvider JobHistory { get; }
        public IJobHistoryStatusProvider JobHistoryStatus { get; }
        public IJobHistoryErrorGuidsProvider JobHistoryError { get; }
        public IDestinationWorkspaceTagGuidProvider DestinationWorkspace { get; }
    }
}