using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Data.Factories
{
	public class TempDocumentTableFactory : ITempDocumentTableFactory
	{
		private readonly IHelper _helper;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IToggleProvider _provider;

		public TempDocumentTableFactory(IHelper helper, IRepositoryFactory repositoryFactory, IToggleProvider provider)
		{
			_helper = helper;
			_repositoryFactory = repositoryFactory;
			_provider = provider;
		}

		public ITempDocTableHelper GetDocTableHelper(string tableSuffix, int sourceWorkspaceId)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(sourceWorkspaceId);
			IDocumentRepository documentRepository = _repositoryFactory.GetDocumentRepository(sourceWorkspaceId);
			return new TempDocTableHelper(_helper, tableSuffix, sourceWorkspaceId, fieldRepository, documentRepository, _provider);
		}
	}
}