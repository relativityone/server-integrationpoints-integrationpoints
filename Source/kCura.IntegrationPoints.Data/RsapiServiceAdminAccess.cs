using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class RSAPIServiceAdminAccess : RSAPIService
	{
		public RSAPIServiceAdminAccess(IHelper helper, int workspaceArtifactId) : base(helper, workspaceArtifactId)
		{
			ExecutionIdentity = ExecutionIdentity.System;
		}

		internal RSAPIServiceAdminAccess(IGenericLibraryFactory genericLibraryFactory) : base(genericLibraryFactory)
		{
			ExecutionIdentity = ExecutionIdentity.System;
		}
	}
}