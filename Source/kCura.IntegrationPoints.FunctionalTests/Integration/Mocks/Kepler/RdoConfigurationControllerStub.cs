using System;
using System.Threading.Tasks;
using Moq;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class RdoConfigurationControllerStub : KeplerStubBase<IRDOConfigurationController>
    {
        public void SetupRdoConfigurationController()
        {
            Mock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<ImportRdoSettings>()))
                .Returns((int workspaceId, Guid importJobId, ImportRdoSettings settings) =>
                {
                    return Task.FromResult(
                        new Response(
                            importJobID: importJobId,
                            isSuccess: true,
                            string.Empty,
                            string.Empty));
                });
        }
    }
}