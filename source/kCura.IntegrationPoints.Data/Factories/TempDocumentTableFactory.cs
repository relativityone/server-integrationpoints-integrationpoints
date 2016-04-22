using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories
{
	public class TempDocumentTableFactory : ITempDocumentTableFactory
	{
		private readonly IHelper _helper;
		private readonly IRepositoryFactory _repositoryFactory;

		public TempDocumentTableFactory(IHelper helper, IRepositoryFactory repositoryFactory)
		{
			_helper = helper;
			_repositoryFactory = repositoryFactory;
		}

		public ITempDocTableHelper GetDocTableHelper(string tableSuffix, int sourceWorkspaceId)
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(sourceWorkspaceId);
			IDocumentRepository documentRepository = _repositoryFactory.GetDocumentRepository(sourceWorkspaceId);

			return new TempDocTableHelper(_helper, tableSuffix, sourceWorkspaceId, fieldRepository, documentRepository);
		}
	}
}