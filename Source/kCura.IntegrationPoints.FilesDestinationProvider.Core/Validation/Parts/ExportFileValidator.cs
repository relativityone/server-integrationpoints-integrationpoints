using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public class ExportFileValidator : ExportFileValidatorBase
    {
        private readonly IExportInitProcessService _exportInitProcessService;

        public ExportFileValidator(ISerializer serializer, IExportSettingsBuilder exportSettingsBuilder, IExportInitProcessService exportInitProcessService, IExportFileBuilder exportFileBuilder) : 
            base(serializer, exportSettingsBuilder, exportFileBuilder)
        {
            _exportInitProcessService = exportInitProcessService;
        }

        protected override ValidationResult PerformValidation(ExportFile exportFile)
        {
            var result = new ValidationResult();

            var totalDocCount = _exportInitProcessService.CalculateDocumentCountToTransfer(ExportSettingsEx, DestinationSettingsEx.ArtifactTypeId);

            var fileCountValidator = new FileCountValidator();
            result.Add(fileCountValidator.Validate(totalDocCount));

            var paddingValidator = new PaddingValidator();
            result.Add(paddingValidator.Validate(exportFile, totalDocCount));

            return result;
        }
    }
}