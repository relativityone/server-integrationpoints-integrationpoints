using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService : IRSAPIService
	{
		public virtual IGenericLibrary<IntegrationPoint> IntegrationPointLibrary => GetGenericLibrary<IntegrationPoint>();
		public virtual IGenericLibrary<SourceProvider> SourceProviderLibrary => GetGenericLibrary<SourceProvider>();
		public virtual IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary => GetGenericLibrary<JobHistoryError>();
		public virtual IGenericLibrary<DestinationWorkspace> DestinationWorkspaceLibrary => GetGenericLibrary<DestinationWorkspace>();
		public virtual IRelativityObjectManager RelativityObjectManager => GetRelativityObjectManager();
	}
}
