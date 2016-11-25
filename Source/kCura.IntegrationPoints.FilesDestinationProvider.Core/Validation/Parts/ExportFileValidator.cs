using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public class ExportFileValidator : BaseValidator<IntegrationModelValidation>
    {
        private readonly ISerializer _serializer;
        private readonly IExportSettingsBuilder _exportSettingsBuilder;
        private readonly IExportInitProcessService _exportInitProcessService;
        private readonly IExportFileBuilder _exportFileBuilder;

        public ExportFileValidator(ISerializer serializer, IExportSettingsBuilder exportSettingsBuilder, IExportInitProcessService exportInitProcessService, IExportFileBuilder exportFileBuilder)
        {
            _serializer = serializer;
            _exportSettingsBuilder = exportSettingsBuilder;
            _exportInitProcessService = exportInitProcessService;
            _exportFileBuilder = exportFileBuilder;
        }

        public override ValidationResult Validate(IntegrationModelValidation value)
        {
            var result = new ValidationResult();

            var exportSettingsEx = _serializer.Deserialize<ExportUsingSavedSearchSettings>(value.SourceConfiguration);
            var totalDocCount = _exportInitProcessService.CalculateDocumentCountToTransfer(exportSettingsEx);

            var fileCountValidator = new FileCountValidator();
            result.Add(fileCountValidator.Validate(totalDocCount));

            var fieldMap = _serializer.Deserialize<IEnumerable<FieldMap>>(value.FieldsMap);

            var exportSettings = _exportSettingsBuilder.Create(exportSettingsEx, fieldMap, value.ArtifactTypeId);
            var exportFile = _exportFileBuilder.Create(exportSettings);

            var paddingValidator = new PaddingValidator();
            result.Add(paddingValidator.Validate(exportFile, totalDocCount));

            return result;
        }
    }
}