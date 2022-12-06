using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture]
    [Category("Unit")]
    public class CalculationCheckerTests
    {
        private readonly int _INTEGRATION_POINT_ID = -10;

        private Mock<IAPILog> _loggerMock;
        private Mock<IRelativityObjectManager> _objectManagerMock;

        private CalculationChecker _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _objectManagerMock = new Mock<IRelativityObjectManager>();

            _sut = new CalculationChecker(_objectManagerMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetCalculationState_ShouldCreateNewState_WhenIntegrationPointFieldValueIsNull()
        {
            // Arrange
            GetDataSetup(null);

            // Act
            CalculationState result = await _sut.GetCalculationState(_INTEGRATION_POINT_ID).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            _objectManagerMock.Verify(
                x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<ExecutionIdentity>()),
                Times.Once);
            result.Status.Should().Be(CalculationStatus.New);
        }

        private void GetDataSetup(CalculationState state)
        {
            IntegrationPoint integrationPoint = new Data.IntegrationPoint
            {
                CalculationState = JsonConvert.SerializeObject(state)
            };

            RelativityObject relativityObject = new RelativityObject
            {
                FieldValues = new List<FieldValuePair>() { new FieldValuePair { Value = JsonConvert.SerializeObject(state) } }
            };

            ResultSet<RelativityObject> resultSet = new ResultSet<RelativityObject>();
            resultSet.Items = new List<RelativityObject>() { relativityObject };

            _objectManagerMock.Setup(x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(resultSet);
        }

        private void UpdateDataSetup(bool updatedSuccessfully = true)
        {
            _objectManagerMock.Setup(x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<IList<FieldRefValuePair>>(),
                It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(updatedSuccessfully);
        }
    }
}
