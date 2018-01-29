using kCura.IntegrationPoints.Data.SecretStore;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class GenericLibraryFactory : IGenericLibraryFactory
	{
		private readonly ISecretManager _secretManager;
		private readonly ISecretCatalogFactory _secretCatalogFactory;
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public GenericLibraryFactory(IHelper helper, int workspaceArtifactId, ISecretCatalogFactory secretCatalogFactory, ISecretManager secretManager)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
			_secretCatalogFactory = secretCatalogFactory;
			_secretManager = secretManager;
		}

		public IGenericLibrary<T> Create<T>(ExecutionIdentity executionIdentity) where T : BaseRdo, new()
		{
			return new RsapiClientLibrary<T>(_helper, _workspaceArtifactId, executionIdentity);
		}
	}
}