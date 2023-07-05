using System;
using System.Threading.Tasks;
using Moq;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class ImportSourceControllerStub : KeplerStubBase<IImportSourceController>
    {
        public void SetupImportSourceController()
        {
            Mock.Setup(x => x.AddSourceAsync(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DataSourceSettings>()))
                .Returns((int workspaceId, Guid importJobID, Guid sourceId, DataSourceSettings dataSourceSettings) =>
                {
                    return Task.FromResult(
                        new Response(
                            importJobID: importJobID,
                            isSuccess: true,
                            string.Empty,
                            string.Empty));
                });
        }
    }
}