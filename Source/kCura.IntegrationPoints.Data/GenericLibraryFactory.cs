using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class GenericLibraryFactory : IGenericLibraryFactory
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public GenericLibraryFactory(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public IGenericLibrary<T> Create<T>(ExecutionIdentity executionIdentity) where T : BaseRdo, new()
		{
			return new RsapiClientLibrary<T>(_helper, _workspaceArtifactId, executionIdentity);
		}
	}
}