using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class ProductionValidatorTests
    {
        private const int _WORKSPACE_ARTIFACT_ID = 1;
        private const int _PRODUCTION_ARTIFACT_ID = 2;

        [Test]
        public void ItShouldValidateProduction()
        {
            // arrange
            var production = new ProductionDTO();
            var productionManager = Substitute.For<IProductionManager>();
            productionManager.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns(production);

            var validator = new ProductionValidator(_WORKSPACE_ARTIFACT_ID, productionManager);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ItShouldFailForNotFoundProduction()
        {
            // arrange
            var production = new ProductionDTO();
            var productionManager = Substitute.For<IProductionManager>();
            productionManager.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Throws(new Exception());

            var validator = new ProductionValidator(_WORKSPACE_ARTIFACT_ID, productionManager);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.Contains(Constants.IntegrationPoints.PermissionErrors.PRODUCTION_NO_ACCESS));
        }

        [Test]
        public void ItShouldFailForEmptyProduction()
        {
            // arrange
            var productionManager = Substitute.For<IProductionManager>();
            productionManager.RetrieveProduction(_WORKSPACE_ARTIFACT_ID, _PRODUCTION_ARTIFACT_ID).Returns((ProductionDTO)null);

            var validator = new ProductionValidator(_WORKSPACE_ARTIFACT_ID, productionManager);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.Contains(Constants.IntegrationPoints.PermissionErrors.PRODUCTION_NO_ACCESS));
        }
    }
}
