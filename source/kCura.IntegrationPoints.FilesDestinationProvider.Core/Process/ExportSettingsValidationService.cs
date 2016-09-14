using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportSettingsValidationService : IExportSettingsValidationService
	{
		private readonly IExportFileBuilder _exportFileBuilder;
		private readonly IExportSettingsBuilder _exportSettingsBuilder;
		private readonly IPaddingValidator _paddingValidator;

		public ExportSettingsValidationService(IExportSettingsBuilder exportSettingsBuilder, IExportFileBuilder exportFileBuilder, IPaddingValidator paddingValidator)
		{
			_exportSettingsBuilder = exportSettingsBuilder;
			_exportFileBuilder = exportFileBuilder;
			_paddingValidator = paddingValidator;
		}

		public ExportSettingsValidationResult Validate(int workspaceID, IntegrationModel model)
		{
			var exportFile = BuildExportFile(model);
			return _paddingValidator.Validate(workspaceID, exportFile);
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