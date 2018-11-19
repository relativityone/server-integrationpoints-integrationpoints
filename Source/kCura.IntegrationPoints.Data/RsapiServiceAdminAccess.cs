using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class RSAPIServiceAdminAccess : RSAPIService
	{
		public RSAPIServiceAdminAccess(IHelper helper, int workspaceArtifactId) : base(helper, workspaceArtifactId)
		{
			ExecutionIdentity = ExecutionIdentity.System;
		}
	}
}