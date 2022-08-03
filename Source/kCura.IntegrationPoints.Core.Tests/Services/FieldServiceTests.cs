using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    public class FieldServiceTests : TestBase
    {
        private Mock<IRelativityObjectManager> _objectManagerFake;
        private Mock<IRelativityObjectManagerFactory> _objectManagerFactoryFake;

        private FieldService _sut;

        private const int _WORKSPACE_ID = 9999;
        private const int _ARTIFACT_TYPE_ID = 8888;

        private readonly List<RelativityObject> _longTextFields = new List<RelativityObject>()
        {
            new RelativityObject()
            {
                ArtifactID = 111,
                Name = "Long Text 1"
            },
            new RelativityObject()
            {
                ArtifactID = 222,
                Name = "Long Text 2"
            },
        };

        private readonly List<RelativityObject> _fixedLengthTextFields = new List<RelativityObject>()
        {
            new RelativityObject()
            {
                ArtifactID = 333,
                Name = "Fixed-Length Text 1"
            },
            new RelativityObject()
            {
                ArtifactID = 444,
                Name = "Fixed-Length Text 2"
            },
        };

        public override void SetUp()
        {
            _objectManagerFake = new Mock<IRelativityObjectManager>();
            _objectManagerFactoryFake = new Mock<IRelativityObjectManagerFactory>();
            _objectManagerFactoryFake.Setup(x => x.CreateRelativityObjectManager(_WORKSPACE_ID)).Returns(_objectManagerFake.Object);

            _sut = new FieldService(_objectManagerFactoryFake.Object);
        }

        [Test]
        public void GetAllTextFields_ShouldReturnAllTextFields()
        {
            // Arrange
            List<RelativityObject> allFields = new List<RelativityObject>();
            allFields.AddRange(_fixedLengthTextFields);
            allFields.AddRange(_longTextFields);
            _objectManagerFake.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(allFields);

            // Act
            IEnumerable<FieldEntry> actualFields = _sut.GetAllTextFields(_WORKSPACE_ID, _ARTIFACT_TYPE_ID);

            // Assert
            AssertFields(allFields, actualFields);
        }

        [Test]
        public void GetAllTextFields_ShouldThrowException_WhenObjectManagerFails()
        {
            // Arrange
            _objectManagerFake.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .Throws<NotFoundException>();

            // Act
            Action action = () => _sut.GetAllTextFields(_WORKSPACE_ID, _ARTIFACT_TYPE_ID);

            // Assert
            action.ShouldThrow<IntegrationPointsException>()
                .Which.InnerException.Should().BeOfType<NotFoundException>();
        }

        [Test]
        public void GetLongTextFields_ShouldReturnAllLongTextFields()
        {
            // Arrange
            _objectManagerFake.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(_longTextFields);

            // Act
            IEnumerable<FieldEntry> actualFields = _sut.GetAllTextFields(_WORKSPACE_ID, _ARTIFACT_TYPE_ID);

            // Assert
            AssertFields(_longTextFields, actualFields);
        }

        [Test]
        public void GetLongTextFields_ShouldThrowException_WhenObjectManagerFails()
        {
            // Arrange
            _objectManagerFake.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .Throws<NotFoundException>();

            // Act
            Action action = () => _sut.GetLongTextFields(_WORKSPACE_ID, _ARTIFACT_TYPE_ID);

            // Assert
            action.ShouldThrow<IntegrationPointsException>()
                .Which.InnerException.Should().BeOfType<NotFoundException>();
        }

        private void AssertFields(IEnumerable<RelativityObject> expectedFields, IEnumerable<FieldEntry> actualFields)
        {
            foreach (RelativityObject field in expectedFields)
            {
                actualFields.Should().Contain(fieldEntry => fieldEntry.FieldIdentifier == field.ArtifactID.ToString() &&
                                                            fieldEntry.ActualName == field.Name &&
                                                            fieldEntry.DisplayName == field.Name);
            }
        }
    }
}

