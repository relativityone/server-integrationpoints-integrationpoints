using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Services
{
	[TestFixture]
	public class ExportSettingsValidationServiceTests : TestBase
	{
		private IExportInitProcessService _exportInitProcessService;
		private ExportSettingsValidationService _exportSettingsValidationService;
		private IFileCountValidator _fileCountValidator;
		private IntegrationModel _integrationModel;
		private IPaddingValidator _paddingValidator;

		[SetUp]
		public override void SetUp()
		{
			_integrationModel = new IntegrationModel
			{
				Destination = "{ArtifactTypeId:1}",
				Map = "[]"
			};

			var exportFileBuilder = Substitute.For<IExportFileBuilder>();
			var exportSettingsBuilder = Substitute.For<IExportSettingsBuilder>();

			_exportInitProcessService = Substitute.For<IExportInitProcessService>();
			_fileCountValidator = Substitute.For<IFileCountValidator>();
			_paddingValidator = Substitute.For<IPaddingValidator>();

			_exportSettingsValidationService = new ExportSettingsValidationService(exportSettingsBuilder, exportFileBuilder, _paddingValidator, _exportInitProcessService,
				_fileCountValidator);
		}

		[Test]
		public void ItShouldReturnFileCountWarning()
		{
			string expectedMessage = "expected_message";

			_exportInitProcessService.CalculateDocumentCountToTransfer(Arg.Any<ExportUsingSavedSearchSettings>()).Returns(0);
			_fileCountValidator.Validate(0).Returns(new ValidationResult
			{
				IsValid = false,
				Message = expectedMessage
			});

			var result = _exportSettingsValidationService.Validate(1, _integrationModel);

			Assert.That(result.IsValid, Is.False);
			Assert.That(result.Message, Is.EqualTo(expectedMessage));
		}

		[Test]
		public void ItShouldNotRunPaddingValidationAfterFileCountValidationFailed()
		{
			_fileCountValidator.Validate(Arg.Any<int>()).Returns(new ValidationResult
			{
				IsValid = false
			});

			_exportSettingsValidationService.Validate(1, _integrationModel);

			_paddingValidator.Received(0).Validate(Arg.Any<int>(), Arg.Any<ExportFile>(), Arg.Any<int>());
		}

		[Test]
		public void ItShouldReturnPaddingValidationWarning()
		{
			string expectedMessage = "expected_message";

			_fileCountValidator.Validate(Arg.Any<int>()).Returns(new ValidationResult
			{
				IsValid = true
			});

			_paddingValidator.Validate(Arg.Any<int>(), Arg.Any<ExportFile>(), Arg.Any<int>()).Returns(new ValidationResult
			{
				IsValid = false,
				Message = expectedMessage
			});

			var result = _exportSettingsValidationService.Validate(1, _integrationModel);

			Assert.That(result.IsValid, Is.False);
			Assert.That(result.Message, Is.EqualTo(expectedMessage));
		}

		[Test]
		public void ItShouldReturnSuccessForValidParameters()
		{
			_fileCountValidator.Validate(Arg.Any<int>()).Returns(new ValidationResult
			{
				IsValid = true
			});

			_paddingValidator.Validate(Arg.Any<int>(), Arg.Any<ExportFile>(), Arg.Any<int>()).Returns(new ValidationResult
			{
				IsValid = true
			});

			var result = _exportSettingsValidationService.Validate(1, _integrationModel);

			Assert.That(result.IsValid, Is.True);
			Assert.That(result.Message, Is.Null.Or.Empty);
		}
	}
}