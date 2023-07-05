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
                .Returns((int workspaceId, Guid importJobID, string applicationName, string correlationId) =>
                {
                    return Task.FromResult(
                        new Response(
                            importJobID: importJobID,
                            isSuccess: true,
                            string.Empty,
                            string.Empty));
                });
            Mock.Setup(x => x.BeginAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .Returns((int workspaceId, Guid importJobID) =>
                {
                    return Task.FromResult(
                        new Response(
                            importJobID: importJobID,
                            isSuccess: true,
                            string.Empty,
                            string.Empty));
                });

            Mock.Setup(x => x.GetProgressAsync(It.IsAny<int>(), It.IsAny<Guid>())).Returns((int workspaceId, Guid importJobId) =>
            {
                return Task.FromResult(
                    new ValueResponse<ImportProgress>(
                        importJobId,
                        true,
                        string.Empty,
                        string.Empty,
                        new ImportProgress(4, 4, 0)));
            });
        }
    }
}