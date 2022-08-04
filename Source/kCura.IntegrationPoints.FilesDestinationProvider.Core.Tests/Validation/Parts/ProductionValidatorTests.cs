using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
    [TestFixture, Category("Unit")]
    public class ProductionValidatorTests
    {
        [Test]
        public void ItShouldValidateProduction()
        {
            // arrange
            var productionId = 42;
            var production = new ProductionDTO { ArtifactID = productionId.ToString() };

            var productionManager = Substitute.For<IProductionManager>();
            productionManager.GetProductionsForExport(Arg.Any<int>())
                .Returns(new[] { production });

            var validator = new ExportProductionValidator(productionManager);

            var exportSettings = new ExportSettings { ProductionId = productionId };

            // act
            var actual = validator.Validate(exportSettings);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ItShouldFailValidationForUnknownProduction()
        {
            // arrange
            var productionId = 42;

            var productionManager = Substitute.For<IProductionManager>();
            productionManager.GetProductionsForExport(Arg.Any<int>())
                .Returns(Enumerable.Empty<ProductionDTO>());

            var validator = new ExportProductionValidator(productionManager);

            var exportSettings = new ExportSettings { ProductionId = productionId };

            // act
            var actual = validator.Validate(exportSettings);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.First().Contains(FileDestinationProviderValidationMessages.PRODUCTION_NOT_EXIST));
        }
    }
}