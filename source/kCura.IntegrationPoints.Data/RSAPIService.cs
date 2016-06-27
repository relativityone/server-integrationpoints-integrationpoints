
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public partial class RSAPIService : IRSAPIService
	{
		//public IIdentifierLibrary IDLibrary { get; set; }

		public IGenericLibrary<Document> DocumentLibrary { get; set; }
		public IGenericLibrary<IntegrationPoint> IntegrationPointLibrary { get; set; }
		public IGenericLibrary<SourceProvider> SourceProviderLibrary { get; set; }
		public IGenericLibrary<DestinationProvider> DestinationProviderLibrary { get; set; }
		public IGenericLibrary<JobHistory> JobHistoryLibrary { get; set; }
		public IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary { get; set; }
		public IGenericLibrary<DestinationWorkspace> DestinationWorkspaceLibrary { get; set; }
	

		public RSAPIService(){}

		public RSAPIService(IHelper helper, int workspaceArtifactId)
		{
			this.DocumentLibrary = new RsapiClientLibrary<Document>(helper, workspaceArtifactId);
			this.IntegrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(helper, workspaceArtifactId);
			this.SourceProviderLibrary = new RsapiClientLibrary<SourceProvider>(helper, workspaceArtifactId);
			this.DestinationProviderLibrary = new RsapiClientLibrary<DestinationProvider>(helper, workspaceArtifactId);
			this.JobHistoryLibrary = new RsapiClientLibrary<JobHistory>(helper, workspaceArtifactId);
			this.JobHistoryErrorLibrary = new RsapiClientLibrary<JobHistoryError>(helper, workspaceArtifactId);
			this.DestinationWorkspaceLibrary = new RsapiClientLibrary<DestinationWorkspace>(helper, workspaceArtifactId);
			
		}
	}
}
