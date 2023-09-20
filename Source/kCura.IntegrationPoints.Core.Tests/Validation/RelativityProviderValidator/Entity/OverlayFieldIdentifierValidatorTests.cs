using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts.Entity;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator.Entity
{
    [TestFixture, Category("Unit")]
    internal class OverlayFieldIdentifierValidatorTests
    {
        private OverlayFieldIdentifierValidator _sut;

        private IFixture _fxt;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _sut = new OverlayFieldIdentifierValidator(Mock.Of<IAPILog>());
        }

        [Test]
        public void Validate_ShouldFailValidation_WhenOverlayIdentifierIsNotMapped()
        {
            // Arrange
            IntegrationPointProviderValidationModel validationModel = _fxt.Create<IntegrationPointProviderValidationModel>();

            // Act
            ValidationResult actual = _sut.Validate(validationModel);

            // Assert
            actual.IsValid.Should().BeFalse();
            actual.MessageTexts.Should().Contain(IntegrationPointProviderValidationMessages.ERROR_OVERLAY_IDENTIFIER_FIELD_NOT_FOUND_IN_MAPPING);
        }

        [TestCase(ImportOverwriteModeEnum.AppendOnly)]
        [TestCase(ImportOverwriteModeEnum.AppendOverlay)]
        public void Validate_ShouldPassValidation_WhenOverwriteModeIsNotOverlayOnly(ImportOverwriteModeEnum overwriteMode)
        {
            // Arrange
            IntegrationPointProviderValidationModel validationModel = _fxt.Create<IntegrationPointProviderValidationModel>();

            validationModel.DestinationConfiguration.ImportOverwriteMode = overwriteMode;
            validationModel.DestinationConfiguration.OverlayIdentifier = validationModel.FieldsMap.Last().DestinationField.DisplayName;

            // Act
            ValidationResult actual = _sut.Validate(validationModel);

            // Assert
            actual.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_ShouldPassValidation_WhenOverlayIdentifierIsFullName()
        {
            // Arrange
            IntegrationPointProviderValidationModel validationModel = _fxt.Create<IntegrationPointProviderValidationModel>();

            validationModel.DestinationConfiguration.OverlayIdentifier = EntityFieldNames.FullName;

            // Act
            ValidationResult actual = _sut.Validate(validationModel);

            // Assert
            actual.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_ShouldFailValidation_WhenOverlayIdentifierIsCustomFieldAndFullNameIsMapped()
        {
            // Arrange
            IntegrationPointProviderValidationModel validationModel = _fxt.Create<IntegrationPointProviderValidationModel>();

            validationModel.DestinationConfiguration.ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly;
			validationModel.DestinationConfiguration.OverlayIdentifier = validationModel.FieldsMap.First().DestinationField.DisplayName;

			validationModel.FieldsMap.Last().DestinationField.DisplayName = EntityFieldNames.FullName;

            // Act
            ValidationResult actual = _sut.Validate(validationModel);

            // Assert
            actual.IsValid.Should().BeFalse();
            actual.MessageTexts.Should().Contain(IntegrationPointProviderValidationMessages.ERROR_OTHER_OVERLAY_IDENTIFIER_WITH_FULL_NAME_MAPPED);
        }
    }
}
