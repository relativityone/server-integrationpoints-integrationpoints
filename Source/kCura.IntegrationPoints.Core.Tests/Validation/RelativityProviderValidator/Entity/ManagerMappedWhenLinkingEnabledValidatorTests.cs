using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts.Entity;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator.Entity
{
    [TestFixture, Category("Unit")]
    internal class ManagerMappedWhenLinkingEnabledValidatorTests
    {
        private ManagerMappedWhenLinkingEnabledValidator _sut;

        private IFixture _fxt;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _sut = new ManagerMappedWhenLinkingEnabledValidator(Mock.Of<IAPILog>());
        }

        [Test]
        public void Validate_ShouldFailValidation_WhenManagerLinkIsConfiguredAndManagerNotMapped()
        {
            // Arrange
            IntegrationPointProviderValidationModel validationModel = _fxt.Create<IntegrationPointProviderValidationModel>();

            validationModel.DestinationConfiguration.EntityManagerFieldContainsLink = true;

            // Act
            ValidationResult actual = _sut.Validate(validationModel);

            // Assert
            actual.IsValid.Should().BeFalse();
            actual.MessageTexts.Should().Contain(IntegrationPointProviderValidationMessages.ERROR_MISSING_MANAGER_FIELD_MAP_WHEN_MANAGER_LINKING_CONFIGURED);
        }
    }
}
