using System;
using System.Threading.Tasks;
using Moq;
using Relativity.Sync.Services.Interfaces.V1;
using Relativity.Sync.Services.Interfaces.V1.DTO;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class SyncServiceStub : KeplerStubBase<ISyncService>
    {
        public void SetupMock()
        {
            Mock.Setup(x => x.SubmitJobAsync(It.IsAny<SubmitJobRequestDTO>()))
                .Returns((SubmitJobRequestDTO request) =>
                {
                    Guid jobId = Guid.NewGuid();

                    Relativity.SyncJobsInQueue.Add(new Models.SyncJobTest
                    {
                        JobId = jobId,
                        WorkspaceId = request.WorkspaceID,
                        SyncConfigurationId = request.SyncConfigurationArtifactID
                    });

                    return Task.FromResult(jobId);
                });
        }
    }
}
