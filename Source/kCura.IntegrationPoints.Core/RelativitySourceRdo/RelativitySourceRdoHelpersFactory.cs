using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public class RelativitySourceRdoHelpersFactory : IRelativitySourceRdoHelpersFactory
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public RelativitySourceRdoHelpersFactory(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public IRelativitySourceRdoDocumentField CreateRelativitySourceRdoDocumentField(IRelativityProviderObjectRepository relativityProviderObjectRepository)
        {
            return new RelativitySourceRdoDocumentField(relativityProviderObjectRepository, _repositoryFactory);
        }

        public IRelativitySourceRdoFields CreateRelativitySourceRdoFields()
        {
            return new RelativitySourceRdoFields(_repositoryFactory);
        }

        public IRelativitySourceRdoObjectType CreateRelativitySourceRdoObjectType(IRelativityProviderObjectRepository relativityProviderObjectRepository)
        {
            return new RelativitySourceRdoObjectType(relativityProviderObjectRepository, _repositoryFactory);
        }
    }
}
