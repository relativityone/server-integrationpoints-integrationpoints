using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public class FieldsMapValidator : BasePartsValidator<IntegrationPointProviderValidationModel>
    {
        private readonly IAPILog _logger;
        private readonly ISerializer _serializer;
        private readonly IExportFieldsService _exportFieldsService;

        public FieldsMapValidator(IAPILog logger, ISerializer serializer, IExportFieldsService exportFieldsService)
        {
            _logger = logger.ForContext<FieldsMapValidator>();
            _serializer = serializer;
            _exportFieldsService = exportFieldsService;
        }

        public override ValidationResult Validate(IntegrationPointProviderValidationModel value)
        {
            var result = new ValidationResult();

            if (value.FieldsMap == null)
            {
                result.Add(FileDestinationProviderValidationMessages.FIELD_MAP_NO_FIELDS);
                return result;
            }

            if (value.FieldsMap.Count() == 0)
            {
                result.Add(FileDestinationProviderValidationMessages.FIELD_MAP_NO_FIELDS);
            }
            else
            {
                var exportSettings = _serializer.Deserialize<ExportUsingSavedSearchSettings>(value.SourceConfiguration);

                var exportableFields = RetrieveExportableFields(value, exportSettings);
                var selectedFields = value.FieldsMap.Select(x => x.SourceField);

                var orphanedFields = selectedFields.Except(exportableFields, new FieldEntryEqualityComparer());

                foreach (var field in orphanedFields)
                {
                    result.Add($"{field.DisplayName ?? "<empty>"} {FileDestinationProviderValidationMessages.FIELD_MAP_UNKNOWN_FIELD}");
                }
            }

            return result;
        }

        private FieldEntry[] RetrieveExportableFields(IntegrationPointProviderValidationModel value,
            ExportUsingSavedSearchSettings exportSettings)
        {
            try
            {
                return _exportFieldsService.GetAllExportableFields(exportSettings.SourceWorkspaceArtifactId, value.ArtifactTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retriving exportable fields in {validator}", nameof(FieldsMapValidator));
                string message =
                    IntegrationPointsExceptionMessages.CreateErrorMessageRetryOrContactAdministrator(
                        "while retrieving exportable fields");
                throw new IntegrationPointsException(message, ex);
            }
        }

        private class FieldEntryEqualityComparer : IEqualityComparer<FieldEntry>
        {
            public bool Equals(FieldEntry x, FieldEntry y)
            {
                if ((x == null) && (y == null))
                {
                    return true;
                }

                if ((x == null) || (y == null))
                {
                    return false;
                }

                return (x.FieldIdentifier == y.FieldIdentifier);
            }

            public int GetHashCode(FieldEntry obj)
            {
                return (obj?.FieldIdentifier == null) ? 0 : obj.FieldIdentifier.GetHashCode();
            }
        }
    }
}
