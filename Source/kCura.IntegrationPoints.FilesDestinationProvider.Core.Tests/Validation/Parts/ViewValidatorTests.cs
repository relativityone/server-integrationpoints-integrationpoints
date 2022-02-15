using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
	[TestFixture, Category("Unit")]
	public class ViewValidatorTests
	{
		[Test]
		public void ItShouldValidateView()
		{
			// arrange
			var viewId = 42;
			var view = new ViewDTO { ArtifactId = viewId };

			var viewServiceMock = Substitute.For<IViewService>();
			viewServiceMock.GetViewsByWorkspaceAndArtifactType(Arg.Any<int>(), Arg.Any<int>())
				.Returns(new List<ViewDTO> { view });

			IAPILog logger = Substitute.For<IAPILog>();
			var validator = new ViewExportValidator(logger, viewServiceMock);

			var exportSettings = new ExportSettings { ViewId = viewId };

			// act
			var actual = validator.Validate(exportSettings);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldFailValidationForUnknownView()
		{
			// arrange
			var viewId = 42;

			var viewServiceMock = Substitute.For<IViewService>();
			viewServiceMock.GetViewsByWorkspaceAndArtifactType(Arg.Any<int>(), Arg.Any<int>())
				.Returns(new List<ViewDTO>());

			IAPILog logger = Substitute.For<IAPILog>();
			var validator = new ViewExportValidator(logger, viewServiceMock);

			var exportSettings = new ExportSettings { ProductionId = viewId };

			// act
			var actual = validator.Validate(exportSettings);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.MessageTexts.First().Contains(FileDestinationProviderValidationMessages.VIEW_NOT_EXIST));
		}
	}
}