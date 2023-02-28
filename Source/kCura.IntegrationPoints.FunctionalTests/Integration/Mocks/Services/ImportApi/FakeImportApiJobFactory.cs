using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.ImportAPI;
using Relativity.API;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi
{
    public class FakeImportApiJobFactory : IImportJobFactory
    {
        private readonly IJobImport _jobImportToCreate;

        public FakeImportApiJobFactory(IJobImport jobImportToCreate)
        {
            _jobImportToCreate = jobImportToCreate;
        }

        public IJobImport Create(IImportAPI importApi, ImportSettings settings, IDataTransferContext context, IHelper helper)
        {
            if (_jobImportToCreate is FakeJobImport fakeJobImport)
            {
                fakeJobImport.Settings = settings;
                fakeJobImport.Context = context;
            }
            return _jobImportToCreate;
        }
    }
}
