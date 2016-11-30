using System;
using System.Collections.Generic;
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

			var validator = new ViewValidator(viewServiceMock);

			var exportSettings = new IntegrationPoints.Core.Models.ExportSettings { ViewId = viewId };

			// act
			var actual = validator.Validate(exportSettings);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldFailValidationForUnknownView()
		{
			// arrange
			var viewId = 42;

			var viewServiceMock = Substitute.For<IViewService>();
			viewServiceMock.GetViewsByWorkspaceAndArtifactType(Arg.Any<int>(), Arg.Any<int>())
				.Returns(new List<ViewDTO>());

			var validator = new ViewValidator(viewServiceMock);

			var exportSettings = new IntegrationPoints.Core.Models.ExportSettings { ProductionId = viewId };

			// act
			var actual = validator.Validate(exportSettings);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.First().Contains(FileDestinationProviderValidationMessages.VIEW_NOT_EXIST));
		}
	}
}