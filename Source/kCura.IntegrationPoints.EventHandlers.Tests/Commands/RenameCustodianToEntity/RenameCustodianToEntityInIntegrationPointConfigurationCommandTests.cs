using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands.RenameCustodianToEntity
{
    [TestFixture, Category("Unit")]
    public class RenameCustodianToEntityInIntegrationPointConfigurationCommandTests
    {
        private RenameCustodianToEntityInIntegrationPointConfigurationCommand _sut;

        private Mock<IEHHelper> _helperFake;
        private Mock<IRelativityObjectManager> _relativityObjectManagerMock;

        [SetUp]
        public void SetUp()
        {
            IAPILog log = Substitute.For<IAPILog>();

            Mock<ILogFactory> logFactory = new Mock<ILogFactory>();
            logFactory.Setup(x => x.GetLogger()).Returns(log);

            _helperFake = new Mock<IEHHelper>();
            _helperFake.Setup(x => x.GetLoggerFactory()).Returns(logFactory.Object);

            _relativityObjectManagerMock = new Mock<IRelativityObjectManager>();
            _relativityObjectManagerMock.Setup(x => x.Query<SourceProvider>(It.IsAny<QueryRequest>(), ExecutionIdentity.System))
                .Returns(new List<SourceProvider>());

            _sut = new RenameCustodianToEntityInIntegrationPointConfigurationCommand(_helperFake.Object, _relativityObjectManagerMock.Object);
        }

        [Test]
        public void ItShouldProcessIntegrationPointForAllNecessaryProviders()
        {
            // arrange
            string[] expectedSourceProviders =
            {
                Constants.IntegrationPoints.SourceProviders.LDAP,
                Constants.IntegrationPoints.SourceProviders.FTP,
                Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE
            };

            // act
            _sut.Execute();

            // assert
            foreach (string provider in expectedSourceProviders)
            {
                _relativityObjectManagerMock.Verify(
                    x => x.Query<SourceProvider>(It.Is<QueryRequest>(q => q.Condition.Contains(provider)), It.IsAny<ExecutionIdentity>()),
                    Times.Once);
            }
        }
    }
}
