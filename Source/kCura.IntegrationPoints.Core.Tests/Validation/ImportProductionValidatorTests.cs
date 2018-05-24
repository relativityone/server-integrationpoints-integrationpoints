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
    [TestFixture]
    class ImportProductionValidatorTests
    {
        private const int _WORKSPACE_ARTIFACT_ID = 1;
        private const int _PRODUCTION_ARTIFACT_ID = 2;
        private const int _FEDERATED_INSTANCE_ID = 3;
        private const string _CREDENTIALS = "";

        [Test]
        public void ItShouldValidateProduction()
        {
            // arrange
            var production = new ProductionDTO() {ArtifactID = _PRODUCTION_ARTIFACT_ID.ToString()};
            var productionManager = Substitute.For<IProductionManager>();
            productionManager.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID, _FEDERATED_INSTANCE_ID, _CREDENTIALS).Returns(new [] { production });

            var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, productionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

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
            var productionManager = Substitute.For<IProductionManager>();
            productionManager.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID, _FEDERATED_INSTANCE_ID, _CREDENTIALS).Returns(Enumerable.Empty<ProductionDTO>());

            var validator = new ImportProductionValidator(_WORKSPACE_ARTIFACT_ID, productionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

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
            productionManager.GetProductionsForImport(_WORKSPACE_ARTIFACT_ID, _FEDERATED_INSTANCE_ID, _CREDENTIALS).Returns(new [] {(ProductionDTO)null});

            var validator = new ImportProductionValidator(_PRODUCTION_ARTIFACT_ID, productionManager, _FEDERATED_INSTANCE_ID, _CREDENTIALS);

            // act
            ValidationResult actual = validator.Validate(_PRODUCTION_ARTIFACT_ID);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.Contains(Constants.IntegrationPoints.PermissionErrors.PRODUCTION_NO_ACCESS));
        }
    }
}

