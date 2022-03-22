using kCura.IntegrationPoint.Tests.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.Services;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataMigration.MigrateFileshareAccess;
using Relativity.Services.Credential;
using Relativity.Services.Interfaces.ResourceServer;
using Relativity.Services.Interfaces.ResourceServer.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ResourceServer;
using Relativity.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using IResourceServerManager = Relativity.Services.ResourceServer.IResourceServerManager;

namespace kCura.ScheduleQueue.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    public class FileShareAccesServiceTests : TestBase
    {
        private Mock<IMigrateFileshare> _migrateFileShareMock;

        private Mock<IObjectManager> _objectManagerFake;
        private Mock<IResourceServerManager> _resoruceServerManagerFake;
        private Mock<ISqlPrimaryServerManager> _sqlPrimaryServerManagerFake;
        private Mock<ISqlDistributedServerManager> _sqlDistributedServerManagerFake;
        private Mock<ICredentialManager> _credentialManagerFake;

        private const string _VALID_CREDENTIAL_ACCOUNT_NAME = "RelSVC-T001";
        private const string _VALID_CREDENTIAL_ACCOUNT_PASSWORD = "p******d";
        private const string _VALID_UNC_PATH = "\\\\emttest\\BCPPath";

        [SetUp]
        public override void SetUp()
        {
            _migrateFileShareMock = new Mock<IMigrateFileshare>();
            _objectManagerFake = new Mock<IObjectManager>();
            _resoruceServerManagerFake = new Mock<IResourceServerManager>();
            _sqlPrimaryServerManagerFake = new Mock<ISqlPrimaryServerManager>();
            _sqlDistributedServerManagerFake = new Mock<ISqlDistributedServerManager>();
            _credentialManagerFake = new Mock<ICredentialManager>();
        }

        [Test]
        public async Task MountBcpPathAsync_ShouldMount_GoldFlow()
        {
            // Arrange
            SqlPrimaryServerResponse sqlPrimaryServer = new SqlPrimaryServerResponse
            {
                ArtifactID = 10,
                BcpPath = _VALID_UNC_PATH,
            };

            SetupSqlServers(sqlPrimaryServer);

            List<Credential> credentials = new List<Credential>
            {
                PrepareCredential(_VALID_CREDENTIAL_ACCOUNT_NAME, _VALID_CREDENTIAL_ACCOUNT_PASSWORD)
            };

            SetupCredentials(credentials);

            IFileShareAccessService sut = PrepareSut();

            // Act
            await sut.MountBcpPathAsync().ConfigureAwait(false);

            // Assert
            _migrateFileShareMock.Verify(x => x.MountAsync(_VALID_UNC_PATH, _VALID_CREDENTIAL_ACCOUNT_NAME, _VALID_CREDENTIAL_ACCOUNT_PASSWORD), Times.Once);
        }

        [Test]
        public async Task MountBcpPathAsync_ShouldNotMount_WhenServersNotFound()
        {
            // Arrange
            SetupSqlServers();

            IFileShareAccessService sut = PrepareSut();

            // Act
            await sut.MountBcpPathAsync().ConfigureAwait(false);

            // Assert
            _migrateFileShareMock.Verify(x => x.MountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task MountBcpPathAsync_ShouldNotMount_WhenServersHaveNotBCPPath()
        {
            // Arrange
            SqlPrimaryServerResponse sqlPrimaryServer = new SqlPrimaryServerResponse
            {
                ArtifactID = 10,
            };

            List<SqlDistributedServerResponse> sqlDistributedServers = new List<SqlDistributedServerResponse>
            {
                new SqlDistributedServerResponse { ArtifactID = 20 },
                new SqlDistributedServerResponse { ArtifactID = 30 },
                new SqlDistributedServerResponse { ArtifactID = 40 },
            };

            SetupSqlServers(sqlPrimaryServer, sqlDistributedServers);

            IFileShareAccessService sut = PrepareSut();

            // Act
            await sut.MountBcpPathAsync().ConfigureAwait(false);

            // Assert
            _migrateFileShareMock.Verify(x => x.MountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task MountBcpPathAsync_ShouldMountFirstApplicableBCPPath_WhenManyServersHaveBCPPath()
        {
            // Arrange
            const string overridenUncPath = "<should_not_be_written>";

            SqlPrimaryServerResponse sqlPrimaryServer = new SqlPrimaryServerResponse
            {
                ArtifactID = 10,
            };

            List<SqlDistributedServerResponse> sqlDistributedServers = new List<SqlDistributedServerResponse>
            {
                new SqlDistributedServerResponse { ArtifactID = 20, BcpPath = _VALID_UNC_PATH },
                new SqlDistributedServerResponse { ArtifactID = 30 },
                new SqlDistributedServerResponse { ArtifactID = 40, BcpPath = overridenUncPath },
            };

            SetupSqlServers(sqlPrimaryServer, sqlDistributedServers);

            List<Credential> credentials = new List<Credential>
            {
                PrepareCredential(_VALID_CREDENTIAL_ACCOUNT_NAME, _VALID_CREDENTIAL_ACCOUNT_PASSWORD)
            };

            SetupCredentials(credentials);

            IFileShareAccessService sut = PrepareSut();

            // Act
            await sut.MountBcpPathAsync().ConfigureAwait(false);

            // Assert
            _migrateFileShareMock.Verify(x => x.MountAsync(
                    _VALID_UNC_PATH, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public async Task MountBcpPathAsync_ShouldNotMount_WhenValidCredentialWasNotFound()
        {
            // Arrange
            SqlPrimaryServerResponse sqlPrimaryServer = new SqlPrimaryServerResponse
            {
                ArtifactID = 10,
                BcpPath = _VALID_UNC_PATH
            };

            SetupSqlServers(sqlPrimaryServer);

            List<Credential> credentials = new List<Credential>
            {
                PrepareCredential("<invalid_user>", _VALID_CREDENTIAL_ACCOUNT_PASSWORD)
            };

            SetupCredentials(credentials);

            IFileShareAccessService sut = PrepareSut();

            // Act
            await sut.MountBcpPathAsync().ConfigureAwait(false);

            // Assert
            _migrateFileShareMock.Verify(x => x.MountAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task MountBcpPathAsync_ShouldNotMount_WhenAccountPasswordIsEmpty()
        {
            // Arrange
            SqlPrimaryServerResponse sqlPrimaryServer = new SqlPrimaryServerResponse
            {
                ArtifactID = 10,
                BcpPath = _VALID_UNC_PATH
            };

            SetupSqlServers(sqlPrimaryServer);

            List<Credential> credentials = new List<Credential>
            {
                PrepareCredential(_VALID_CREDENTIAL_ACCOUNT_NAME, null)
            };

            SetupCredentials(credentials);

            IFileShareAccessService sut = PrepareSut();

            // Act
            await sut.MountBcpPathAsync().ConfigureAwait(false);

            // Assert
            _migrateFileShareMock.Verify(x => x.MountAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        private IFileShareAccessService PrepareSut()
        {
            Mock<IHelper> helper = new Mock<IHelper>();

            Mock<IAPILog> log = new Mock<IAPILog>();
            log.Setup(x => x.ForContext<FileShareAccessService>()).Returns(log.Object);

            Mock<ILogFactory> logFactory = new Mock<ILogFactory>();
            logFactory.Setup(x => x.GetLogger()).Returns(log.Object);

            Mock<IServicesMgr> servicesMgr = new Mock<IServicesMgr>();
            servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>())).Returns(_objectManagerFake.Object);
            servicesMgr.Setup(x => x.CreateProxy<IResourceServerManager>(It.IsAny<ExecutionIdentity>())).Returns(_resoruceServerManagerFake.Object);
            servicesMgr.Setup(x => x.CreateProxy<ISqlPrimaryServerManager>(It.IsAny<ExecutionIdentity>())).Returns(_sqlPrimaryServerManagerFake.Object);
            servicesMgr.Setup(x => x.CreateProxy<ISqlDistributedServerManager>(It.IsAny<ExecutionIdentity>())).Returns(_sqlDistributedServerManagerFake.Object);
            servicesMgr.Setup(x => x.CreateProxy<ICredentialManager>(It.IsAny<ExecutionIdentity>())).Returns(_credentialManagerFake.Object);

            helper.Setup(x => x.GetLoggerFactory()).Returns(logFactory.Object);
            helper.Setup(x => x.GetServicesManager()).Returns(servicesMgr.Object);

            Mock<IMigrateFileshareFactory> migrateFileshareFactory = new Mock<IMigrateFileshareFactory>();
            migrateFileshareFactory.Setup(x => x.Create()).Returns(_migrateFileShareMock.Object);

            return new FileShareAccessService(helper.Object, migrateFileshareFactory.Object);
        }

        private void SetupSqlServers(
            SqlPrimaryServerResponse sqlPrimaryServer = null,
            List<SqlDistributedServerResponse> sqlDistributedServers = null)
        {
            sqlDistributedServers = sqlDistributedServers ?? new List<SqlDistributedServerResponse>();
            sqlPrimaryServer = sqlPrimaryServer ?? new SqlPrimaryServerResponse();

            List<int> resourceServers = new List<int>();
            resourceServers.Add(sqlPrimaryServer?.ArtifactID ?? 0);
            resourceServers.AddRange(sqlDistributedServers.Select(x => x.ArtifactID));

            resourceServers = resourceServers.Where(x => x != 0).ToList();

            _objectManagerFake.Setup(x => x.QuerySlimAsync(-1, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResultSlim
                {
                    ResultCount = resourceServers.Count,
                    Objects = resourceServers.Select(x => new RelativityObjectSlim { ArtifactID = x }).ToList()
                });

            _resoruceServerManagerFake.Setup(x => x.ReadSingleAsync(sqlPrimaryServer.ArtifactID))
                .ReturnsAsync(new ResourceServer
                {
                    ArtifactID = sqlPrimaryServer.ArtifactID,
                    ServerType = new ResourceServerTypeRef { Name = "SQL - Primary" }
                });

            _resoruceServerManagerFake.Setup(x => x.ReadSingleAsync(
                    It.Is<int>(y => sqlDistributedServers.Select(s => s.ArtifactID).Contains(y))))
                .Returns((int artifactId) =>
                {
                    return Task.FromResult(new ResourceServer
                    {
                        ArtifactID = artifactId,
                        ServerType = new ResourceServerTypeRef { Name = "SQL - Distributed" }
                    });
                });

            _sqlPrimaryServerManagerFake.Setup(x => x.ReadAsync(sqlPrimaryServer.ArtifactID))
                .ReturnsAsync(sqlPrimaryServer);

            _sqlDistributedServerManagerFake.Setup(x => x.ReadAsync(
                    It.Is<int>(y => sqlDistributedServers.Select(s => s.ArtifactID).Contains(y))))
                .Returns((int artifactId) => Task.FromResult(sqlDistributedServers.Single(x => x.ArtifactID == artifactId)));
        }

        private void SetupCredentials(List<Credential> credentials, bool isSuccess = true)
        {
            _credentialManagerFake.Setup(x => x.QueryAsync(It.IsAny<Query>()))
                .ReturnsAsync(new CredentialQueryResultSet
                {
                    Success = isSuccess,
                    Results = credentials.Select(x => new Result<Credential> { Artifact = x }).ToList()
                });
        }

        private Credential PrepareCredential(string accountName, string accountPassword)
        {
            return new Credential
            {
                SecretValues = new Dictionary<string, string>
                {
                    { "accountName", accountName },
                    { "accountPassword", accountPassword }
                }
            };
        }
    }
}
