﻿namespace kCura.IntegrationPoints.Data
{
	public partial interface IRSAPIService
	{
		IGenericLibrary<Document> DocumentLibrary { get; set; }
		IGenericLibrary<IntegrationPoint> IntegrationPointLibrary { get; set; }
		IGenericLibrary<SourceProvider> SourceProviderLibrary { get; set; }
		IGenericLibrary<DestinationProvider> DestinationProviderLibrary { get; set; }
		IGenericLibrary<JobHistory> JobHistoryLibrary { get; set; }
		IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary { get; set; }
		IGenericLibrary<DestinationWorkspace> DestinationWorkspaceLibrary { get; set; }
		
	}
}
