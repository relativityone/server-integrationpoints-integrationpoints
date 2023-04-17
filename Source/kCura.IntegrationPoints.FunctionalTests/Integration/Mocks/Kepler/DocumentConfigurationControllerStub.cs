using System;
using System.Threading.Tasks;
using Moq;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class DocumentConfigurationControllerStub : KeplerStubBase<IDocumentConfigurationController>
    {
        public void SetupDocumentConfigurationController()
        {
            Mock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<ImportDocumentSettings>()))
                .Returns((int workspaceId, Guid importJobId, ImportDocumentSettings settings) =>
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
