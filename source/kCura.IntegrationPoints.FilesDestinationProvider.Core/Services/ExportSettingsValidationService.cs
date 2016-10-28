using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.WinEDDS;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ExportSettingsValidationService : IExportSettingsValidationService
	{
		private readonly IExportFileBuilder _exportFileBuilder;
		private readonly IExportInitProcessService _exportInitProcessService;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;
		private readonly IFileCountValidator _fileCountValidator;
		private readonly IPaddingValidator _paddingValidator;

		public ExportSettingsValidationService(IExportSettingsBuilder exportSettingsBuilder, IExportFileBuilder exportFileBuilder, IPaddingValidator paddingValidator,
			IExportInitProcessService exportInitProcessService, IFileCountValidator fileCountValidator)
		{
			_exportSettingsBuilder = exportSettingsBuilder;
			_exportFileBuilder = exportFileBuilder;
			_paddingValidator = paddingValidator;
			_exportInitProcessService = exportInitProcessService;
			_fileCountValidator = fileCountValidator;
		}

		public ValidationResult Validate(int workspaceID, IntegrationModel model)
		{
			ExportFile exportFile = BuildExportFile(model);

			int totalDocsCount = _exportInitProcessService.CalculateDocumentCountToTransfer(
				JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(model.SourceConfiguration));

			var fileCountValidationResult = _fileCountValidator.Validate(totalDocsCount);
			if (!fileCountValidationResult.IsValid)
			{
				return fileCountValidationResult;
			}

			return _paddingValidator.Validate(workspaceID, exportFile, totalDocsCount);
		}

		private ExportFile BuildExportFile(IntegrationModel model)
		{
			var sourceSettings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(model.SourceConfiguration);
			var artifactTypeId = JsonConvert.DeserializeAnonymousType(model.Destination, new {ArtifactTypeId = 1}).ArtifactTypeId;
			var fieldMap = JsonConvert.DeserializeObject<IEnumerable<FieldMap>>(model.Map);

			var exportSettings = _exportSettingsBuilder.Create(sourceSettings, fieldMap, artifactTypeId);
			var exportFile = _exportFileBuilder.Create(exportSettings);
			return exportFile;
		}
	}
}