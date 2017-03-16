namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService : IRSAPIService
	{
		public virtual IGenericLibrary<Document> DocumentLibrary => GetGenericLibrary<Document>();
		public virtual IGenericLibrary<IntegrationPoint> IntegrationPointLibrary => GetGenericLibrary<IntegrationPoint>();
		public virtual IGenericLibrary<SourceProvider> SourceProviderLibrary => GetGenericLibrary<SourceProvider>();
		public virtual IGenericLibrary<DestinationProvider> DestinationProviderLibrary => GetGenericLibrary<DestinationProvider>();
		public virtual IGenericLibrary<JobHistory> JobHistoryLibrary => GetGenericLibrary<JobHistory>();
		public virtual IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary => GetGenericLibrary<JobHistoryError>();
		public virtual IGenericLibrary<DestinationWorkspace> DestinationWorkspaceLibrary => GetGenericLibrary<DestinationWorkspace>();
		public virtual IGenericLibrary<IntegrationPointType> IntegrationPointTypeLibrary => GetGenericLibrary<IntegrationPointType>();
		public virtual IGenericLibrary<IntegrationPointProfile> IntegrationPointProfileLibrary => GetGenericLibrary<IntegrationPointProfile>();
		}
}
