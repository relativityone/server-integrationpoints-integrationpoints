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

		public IGenericLibrary<T> Create<T>() where T : BaseRdo, new()
		{
			if (typeof(T) == typeof(IntegrationPoint))
			{
				RsapiClientLibrary<IntegrationPoint> integrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(_helper, _workspaceArtifactId);
				return (IGenericLibrary<T>) new EncryptingRsapiClientLibrary(integrationPointLibrary, _secretCatalogFactory.Create(_workspaceArtifactId), _secretManager);
			}
			return new RsapiClientLibrary<T>(_helper, _workspaceArtifactId);
		}
	}
}