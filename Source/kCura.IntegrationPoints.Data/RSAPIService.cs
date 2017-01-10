namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService : IRSAPIService
	{
		public IGenericLibrary<Document> DocumentLibrary => GetGenericLibrary<Document>();
		public IGenericLibrary<IntegrationPoint> IntegrationPointLibrary => GetGenericLibrary<IntegrationPoint>();
		public IGenericLibrary<SourceProvider> SourceProviderLibrary => GetGenericLibrary<SourceProvider>();
		public IGenericLibrary<DestinationProvider> DestinationProviderLibrary => GetGenericLibrary<DestinationProvider>();
		public IGenericLibrary<JobHistory> JobHistoryLibrary => GetGenericLibrary<JobHistory>();
		public IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary => GetGenericLibrary<JobHistoryError>();
		public IGenericLibrary<DestinationWorkspace> DestinationWorkspaceLibrary => GetGenericLibrary<DestinationWorkspace>();
		public IGenericLibrary<IntegrationPointType> IntegrationPointTypeLibrary => GetGenericLibrary<IntegrationPointType>();
		public IGenericLibrary<IntegrationPointProfile> IntegrationPointProfileLibrary => GetGenericLibrary<IntegrationPointProfile>();
		}
}
