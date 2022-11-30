using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Extensions;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Tests.Unit.Extensions
{
    [TestFixture]
    public class SourceServiceFactoryForAdminExtensionsTests
    {
        [Test]
        public async Task PrepareSyncConfigurationForResume_ShouldSetResumingToTrue()
        {
            // Arrange
            const int workspaceId = 1;
            const int configurationId = 2;

            var objectManagerMock = new Mock<IObjectManager>();
            objectManagerMock.Setup(x => x.UpdateAsync(workspaceId, It.IsAny<UpdateRequest>())).Returns(Task.FromResult(new UpdateResult()));

            var syncServicesManagerMock = new Mock<IServicesMgr>();
            syncServicesManagerMock.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System))
                .Returns(objectManagerMock.Object);

            // Act
            await syncServicesManagerMock.Object.PrepareSyncConfigurationForResumeAsync(workspaceId, configurationId, new Mock<IAPILog>().Object)
                .ConfigureAwait(false);

            // Assert
            objectManagerMock.Verify(om => om.UpdateAsync(workspaceId,
                It.Is<UpdateRequest>(r =>
                    r.Object.ArtifactID == configurationId && r.FieldValues.Count() == 1 && r.FieldValues.Any(f =>
                        f.Field.Guid.Value == Guid.Parse(SyncRdoGuids.ResumingGuid) && f.Value.Equals(true)))));
        }
    }
}
