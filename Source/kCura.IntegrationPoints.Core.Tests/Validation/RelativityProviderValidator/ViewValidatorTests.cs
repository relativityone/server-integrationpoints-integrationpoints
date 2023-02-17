using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
    [TestFixture, Category("Unit")]
    public class ViewValidatorTests
    {
        private const int _VIEW_ID = 10001;
        private Mock<IAPILog> _loggerFake;
        private Mock<IRelativityObjectManager> _objectManagerMock;
        private ViewValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerFake = new Mock<IAPILog>();
            _objectManagerMock = new Mock<IRelativityObjectManager>();

            _sut = new ViewValidator(_objectManagerMock.Object, _loggerFake.Object);
        }

        [Test]
        public void Validate_ShouldPassValidation()
        {
            // Arrange
            _objectManagerMock
                .Setup(x => x.Query(It.Is<QueryRequest>(request =>
                        request.ObjectType.ArtifactTypeID == (int)ArtifactType.View &&
                        request.Condition == $"'Artifact ID' == {_VIEW_ID}"),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(new List<RelativityObject>()
                {
                    new RelativityObject()
                });

            // Act
            ValidationResult result = _sut.Validate(_VIEW_ID);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_ShouldFailValidation()
        {
            // Arrange
            _objectManagerMock
                .Setup(x => x.Query(It.Is<QueryRequest>(request =>
                        request.ObjectType.ArtifactTypeID == (int)ArtifactType.View &&
                        request.Condition == $"'Artifact ID' == {_VIEW_ID}"),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(new List<RelativityObject>());

            // Act
            ValidationResult result = _sut.Validate(_VIEW_ID);

            // Assert
            result.IsValid.Should().BeFalse();
        }
    }
}
