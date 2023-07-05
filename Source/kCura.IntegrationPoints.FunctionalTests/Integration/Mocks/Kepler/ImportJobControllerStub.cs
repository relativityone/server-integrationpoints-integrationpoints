using System;
using System.Threading.Tasks;
using Moq;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Services;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class ImportJobControllerStub : KeplerStubBase<IImportJobController>
    {
        public void SetupImportJobController()
        {
            Mock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((int workspaceId, Guid importJobId, string applicationName, string correlationId) =>
                {
                    return Task.FromResult(
                        new Response(
                            importJobID: importJobId,
                            isSuccess: true,
                            string.Empty,
                            string.Empty));
                });

            Mock.Setup(x => x.BeginAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns((int workspaceId, Guid importJobId) => GetSuccessfulResponse(workspaceId, importJobId));

            Mock.Setup(x => x.EndAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns((int workspaceId, Guid importJobId) => GetSuccessfulResponse(workspaceId, importJobId));

            Mock.Setup(x => x.GetProgressAsync(It.IsAny<int>(), It.IsAny<Guid>())).Returns((int workspaceId, Guid importJobId) => Task.FromResult(
                new ValueResponse<ImportProgress>(
                    importJobId,
                    true,
                    string.Empty,
                    string.Empty,
                    new ImportProgress(4, 4, 0))));

            Mock.Setup(x => x.GetDetailsAsync(It.IsAny<int>(), It.IsAny<Guid>())).Returns((int workspaceId, Guid importJobId) => Task.FromResult(
                new ValueResponse<ImportDetails>(
                    importJobId,
                    true,
                    string.Empty,
                    string.Empty,
                    new ImportDetails(
                        ImportState.Completed,
                        "Rip and SFU",
                        Relativity.TestContext.User.ArtifactId,
                        DateTime.Today,
                        Relativity.TestContext.User.ArtifactId,
                        DateTime.UtcNow))));
        }

        private Task<Response> GetSuccessfulResponse(int workspaceId, Guid importJobId)
        {
            return Task.FromResult(
                new Response(
                    importJobID: importJobId,
                    isSuccess: true,
                    string.Empty,
                    string.Empty));
        }
    }
}