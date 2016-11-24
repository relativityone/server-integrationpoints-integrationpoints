namespace kCura.IntegrationPoints.Data
{
	public partial interface IRSAPIService
	{
		IGenericLibrary<Document> DocumentLibrary { get; }
		IGenericLibrary<IntegrationPoint> IntegrationPointLibrary { get; }
		IGenericLibrary<SourceProvider> SourceProviderLibrary { get; }
		IGenericLibrary<DestinationProvider> DestinationProviderLibrary { get; }
		IGenericLibrary<JobHistory> JobHistoryLibrary { get; }
		IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary { get; }
		IGenericLibrary<DestinationWorkspace> DestinationWorkspaceLibrary { get; }
		IGenericLibrary<IntegrationPointType> IntegrationPointTypeLibrary { get; }
		IGenericLibrary<IntegrationPointProfile> IntegrationPointProfileLibrary { get; }
		
		IGenericLibrary<T> GetGenericLibrary<T>() where T : BaseRdo, new();
	}
}
