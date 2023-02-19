using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    internal class ExportDataSanitizer : IExportDataSanitizer
    {
        private readonly Dictionary<FieldTypeHelper.FieldType, IExportFieldSanitizer> _sanitizers;

        public ExportDataSanitizer(IEnumerable<IExportFieldSanitizer> sanitizers)
        {
            List<IExportFieldSanitizer> sanitizerList = sanitizers?.ToList() ?? new List<IExportFieldSanitizer>();
            ValidateSanitizerProviders(sanitizerList);

            _sanitizers = sanitizerList.ToDictionary(s => s.SupportedType);
        }

        public bool ShouldSanitize(FieldTypeHelper.FieldType fieldType) => _sanitizers.ContainsKey(fieldType);

        public Task<object> SanitizeAsync(
            int workspaceArtifactID,
            string itemIdentifierSourceFieldName,
            string itemIdentifier,
            string fieldName,
            FieldTypeHelper.FieldType fieldType,
            object initialValue)
        {
            if (!_sanitizers.ContainsKey(fieldType))
            {
                throw new InvalidOperationException($"No field sanitizer found for given '{fieldType}' data type.");
            }

            return _sanitizers[fieldType]
                .SanitizeAsync(workspaceArtifactID, itemIdentifierSourceFieldName, itemIdentifier, fieldName, initialValue);
        }

        private static void ValidateSanitizerProviders(IList<IExportFieldSanitizer> sanitizerList)
        {
            HashSet<FieldTypeHelper.FieldType> uniqueDataTypes =
                new HashSet<FieldTypeHelper.FieldType>(sanitizerList.Select(x => x.SupportedType));
            if (sanitizerList.Count > uniqueDataTypes.Count)
            {
                throw new IntegrationPointsException(
                    "Multiple sanitizers for the same data type were found. Ensure there is exactly one sanitizer per data type.");
            }
        }
    }
}
