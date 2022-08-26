using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Utils.Workarounds;

namespace Relativity.Sync.Tests.Unit.Utils.Workarounds
{
    [TestFixture]
    public class RipWorkaroundsTests
    {
        private readonly Guid _ripJobHistoryTypeGuid = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
        private readonly Guid _hasErrorsFieldGuid = new Guid("a9853e55-0ba0-43d8-a766-747a61471981");
        private readonly Guid _lastRuntimeFieldGuid = new Guid("90d58af1-f79f-40ae-85fc-7e42f84dbcc1");

        private Mock<ISourceServiceFactoryForAdmin> _serviceFactory;
        private Mock<IObjectManager> _objectManager;
        private IRdoGuidConfiguration _rdoGuidConfiguration;

        private RipWorkarounds _sut;

        [SetUp]
        public void SetUp()
        {
            _serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();
            _objectManager = new Mock<IObjectManager>();
            _serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            _rdoGuidConfiguration = new ConfigurationStub();

            _sut = new RipWorkarounds(_serviceFactory.Object, _rdoGuidConfiguration, new EmptyLogger());
        }

        [Test]
        public void TryUpdateIntegrationPointAsync_ShouldNotThrow()
        {
            // Arrange
            _objectManager
                .Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws<InvalidOperationException>();

            // Act
            Func<Task> action = async () => await _sut.TryUpdateIntegrationPointAsync(0, 1, false, DateTime.UtcNow);

            // Assert
            action.Should().NotThrow();
        }

        [Test]
        public async Task TryUpdateIntegrationPointAsync_ShouldNotUpdate_WhenJobHistoryTypeGuidIsDifferentThanRip()
        {
            // Arrange

            // Act
            await _sut.TryUpdateIntegrationPointAsync(1, 2, true, DateTime.Now);

            // Assert
            _objectManager.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()), Times.Never);
        }

        [Test]
        public async Task TryUpdateIntegrationPointAsync_ShouldNotUpdate_WhenHasErrorsIsNull()
        {
            // Arrange
            Mock<IRdoGuidConfiguration> rdoConfig = new Mock<IRdoGuidConfiguration>();
            rdoConfig.SetupGet(x => x.JobHistory.TypeGuid).Returns(_ripJobHistoryTypeGuid);
            RipWorkarounds sut = new RipWorkarounds(_serviceFactory.Object, rdoConfig.Object, new EmptyLogger());

            // Act
            await sut.TryUpdateIntegrationPointAsync(1, 2, null, DateTime.Now);

            // Assert
            _objectManager.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()), Times.Never);
        }

        [Test]
        public async Task TryUpdateIntegrationPointAsync_ShouldUpdate()
        {
            // Arrange
            const int workspaceId = 1;
            const int jobHistoryId = 2;
            const int integrationPointId = 3;
            bool hasErrors = true;

            _objectManager
                .Setup(x => x.QueryAsync(workspaceId, It.Is<QueryRequest>(req => req.ObjectType.Guid == _ripJobHistoryTypeGuid &&
                                                                                 req.Condition == $"'Artifact ID' == {jobHistoryId}" &&
                                                                                 req.Fields.Single().Name == "Integration Point"), 0, 1))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>()
                    {
                        new RelativityObject()
                        {
                            FieldValues = new List<FieldValuePair>()
                            {
                                new FieldValuePair()
                                {
                                    Value = new List<RelativityObjectValue>()
                                    {
                                        new RelativityObjectValue()
                                        {
                                            ArtifactID = integrationPointId
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

            // Act
            DateTime endTime = DateTime.Now;
            await _sut.TryUpdateIntegrationPointAsync(workspaceId, jobHistoryId, hasErrors, endTime);

            // Assert
            _objectManager.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.Is<UpdateRequest>(req => req.Object.ArtifactID == integrationPointId &&
                                                                                                  (bool)req.FieldValues.Single(fieldValuePair => fieldValuePair.Field.Guid == _hasErrorsFieldGuid).Value == hasErrors &&
                                                                                                  ((DateTime)req.FieldValues.Single(fieldValuePair => fieldValuePair.Field.Guid == _lastRuntimeFieldGuid).Value).Ticks == endTime.Ticks)), Times.Once);
        }
    }
}