using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.ImportAPI;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi
{
    public class FakeImportApiFactory : IImportApiFactory
    {
        private readonly IImportAPI _importApi;
        private readonly IImportApiFacade _facade;

        public FakeImportApiFactory(IImportApiFacade facade)
        {
            _importApi = null;
            _facade = facade;
        }

        public FakeImportApiFactory(IImportAPI importApi, IImportApiFacade facade)
        {
            _importApi = importApi;
            _facade = facade;
        }

        public IImportAPI GetImportAPI(ImportSettings settings)
        {
            return _importApi;
        }

        public IImportApiFacade GetImportApiFacade(ImportSettings settings)
        {
            return _facade;
        }
    }
}
