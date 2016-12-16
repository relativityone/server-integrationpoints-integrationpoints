using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
	[TestFixture]
	public class ProductionValidatorTests
	{
		[Test]
		public void ItShouldValidateProduction()
		{
			// arrange
			var productionId = 42;
			var production = new ProductionDTO { ArtifactID = productionId.ToString() };

			var productionServiceMock = Substitute.For<IProductionService>();
			productionServiceMock.GetProductionsForExport(Arg.Any<int>())
				.Returns(new[] { production });

			var validator = new ProductionValidator(productionServiceMock);

			var exportSettings = new IntegrationPoints.Core.Models.ExportSettings { ProductionId = productionId };

			// act
			var actual = validator.Validate(exportSettings);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldFailValidationForUnknownProduction()
		{
			// arrange
			var productionId = 42;

			var productionServiceMock = Substitute.For<IProductionService>();
			productionServiceMock.GetProductionsForExport(Arg.Any<int>())
				.Returns(Enumerable.Empty<ProductionDTO>());

			var validator = new ProductionValidator(productionServiceMock);

			var exportSettings = new IntegrationPoints.Core.Models.ExportSettings { ProductionId = productionId };

			// act
			var actual = validator.Validate(exportSettings);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.First().Contains(FileDestinationProviderValidationMessages.PRODUCTION_NOT_EXIST));
		}
	}
}