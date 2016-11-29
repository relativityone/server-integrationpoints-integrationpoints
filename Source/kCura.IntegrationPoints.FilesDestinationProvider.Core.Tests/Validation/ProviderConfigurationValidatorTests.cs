using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation
{
	[TestFixture]
	public class ProviderConfigurationValidatorTests
	{
		[Test]
		public void ItShouldPrevalidateModel()
		{
			// arrange
			var serializerMock = Substitute.For<ISerializer>();
			var settingsBuilderMock = Substitute.For<IExportSettingsBuilder>();
			var initProcessMock = Substitute.For<IExportInitProcessService>();
			var fileBuilderMock = Substitute.For<IExportFileBuilder>();

			var validatorMock = Substitute.For<ExportFileValidator>(serializerMock, settingsBuilderMock, initProcessMock, fileBuilderMock);
			validatorMock.Validate(Arg.Any<object>())
				.Returns(new ValidationResult());

			var validatorsFactoryMock = Substitute.For<IFileDestinationProviderValidatorsFactory>();
			validatorsFactoryMock.CreateExportFileValidator()
				.Returns(validatorMock);

			var validator = new FileDestinationProviderConfigurationValidator(serializerMock, validatorsFactoryMock);

			// act
			var actual = validator.Prevalidate(new IntegrationPointModel());

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldValidateModel()
		{
			// arrange
			var serializerMock = Substitute.For<ISerializer>();
			var validatorsFactoryMock = Substitute.For<IFileDestinationProviderValidatorsFactory>();

			var validator = new FileDestinationProviderConfigurationValidator(serializerMock, validatorsFactoryMock);

			// act
			var actual = validator.Validate(new IntegrationPointModel());

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldValidateBoxedModel()
		{
			// arrange
			var serializerMock = Substitute.For<ISerializer>();
			var validatorsFactoryMock = Substitute.For<IFileDestinationProviderValidatorsFactory>();

			var validator = new FileDestinationProviderConfigurationValidator(serializerMock, validatorsFactoryMock);

			object model = new IntegrationPointModel();

			// act
			var actual = validator.Validate(model);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}
	}
}