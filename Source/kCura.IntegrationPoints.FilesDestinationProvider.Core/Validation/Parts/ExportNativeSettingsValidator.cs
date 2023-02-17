using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.WinEDDS;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public class ExportNativeSettingsValidator : ExportFileValidatorBase
    {
        private readonly IExportFieldsService _exportFieldsService;

        public ExportNativeSettingsValidator(
            ISerializer serializer,
            IExportSettingsBuilder exportSettingsBuilder,
            IExportFileBuilder exportFileBuilder,
            IExportFieldsService exportFieldsService
        ) : base(serializer, exportSettingsBuilder, exportFileBuilder)
        {
            _exportFieldsService = exportFieldsService;
        }

        protected override ValidationResult PerformValidation(ExportFile exportFile)
        {
            var validationResult = new ValidationResult();

            if (IsRdoExportMode(exportFile) && exportFile.ExportNative && !FileTypeFieldExists(exportFile))
            {
                validationResult.Add(FileDestinationProviderValidationMessages.RDO_INVALID_EXPORT_NATIVE_OPTION);
            }

            return validationResult;
        }

        private static bool IsRdoExportMode(ExportFile exportFile)
        {
            return exportFile.TypeOfExport == ExportFile.ExportType.AncestorSearch && exportFile.ArtifactTypeID != (int)ArtifactType.Document;
        }

        private bool FileTypeFieldExists(ExportFile exportFile)
        {
            FieldEntry[] fieldEntries = _exportFieldsService.GetAllExportableFields(exportFile.CaseArtifactID, exportFile.ArtifactTypeID);

            return fieldEntries.Any(field => field.FieldType == FieldType.File);
        }
    }
}
