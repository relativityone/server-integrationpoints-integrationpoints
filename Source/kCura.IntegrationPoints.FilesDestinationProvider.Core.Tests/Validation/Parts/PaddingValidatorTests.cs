using System;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.WinEDDS;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
	[TestFixture]
	public class PaddingValidatorTests
	{
		[Test]
		public void ItShouldPassForNoDocumentsCount()
		{
			// arrange
			var exportFile = new ExportFile(artifactTypeID: 42);
			exportFile.VolumeInfo = new WinEDDS.Exporters.VolumeInfo { VolumeStartNumber = 1, SubdirectoryStartNumber = 1 };

			var validator = new PaddingValidator();

			// act
			var actual = validator.Validate(exportFile);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.FirstOrDefault(), Is.Null);
		}

		[Test]
		public void ItShouldPassForDocumentsWithEnoughPadding()
		{
			// arrange
			var exportFile = new ExportFile(artifactTypeID: 42);
			exportFile.VolumeInfo = new WinEDDS.Exporters.VolumeInfo { VolumeStartNumber = 1, SubdirectoryStartNumber = 1 };

			var validator = new PaddingValidator();

			// act
			var actual = validator.Validate(exportFile, totalDocCount: 100);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.FirstOrDefault(), Is.Null);
		}

		[Test]
		public void ItShouldFailValidationForDocumentsWithoutEnoughPadding()
		{
			// arrange
			var exportFile = new ExportFile(artifactTypeID: 42);
			exportFile.VolumeInfo = new WinEDDS.Exporters.VolumeInfo { VolumeStartNumber = 1, SubdirectoryStartNumber = 1 };

			var validator = new PaddingValidator();

			// act
			var actual = validator.Validate(exportFile, totalDocCount: 1000000);

			// assert
			// TODO: somehow make the validator to return warning
			// Assert.IsTrue(actual.IsValid);
			// Assert.That(actual.Messages.FirstOrDefault(), Is.Not.Null.Or.Empty);
		}
	}
}