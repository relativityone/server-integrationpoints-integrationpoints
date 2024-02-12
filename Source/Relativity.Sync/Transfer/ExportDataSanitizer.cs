using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal sealed class ExportDataSanitizer : IExportDataSanitizer
    {
        private readonly Dictionary<RelativityDataType, IExportFieldSanitizer> _sanitizers;

        public ExportDataSanitizer(IEnumerable<IExportFieldSanitizer> sanitizers)
        {
            List<IExportFieldSanitizer> sanitizersList = sanitizers?.ToList() ?? new List<IExportFieldSanitizer>();
            HashSet<RelativityDataType> uniqueDataTypes = new HashSet<RelativityDataType>(sanitizersList.Select(x => x.SupportedType));
            if (sanitizersList.Count > uniqueDataTypes.Count)
            {
                throw new ArgumentException("Multiple sanitizers for the same data type were found. Ensure there is exactly one sanitizer per data type.", nameof(sanitizers));
            }

            _sanitizers = sanitizersList.ToDictionary(s => s.SupportedType);
        }

        public bool ShouldSanitize(RelativityDataType dataType)
        {
            return _sanitizers.ContainsKey(dataType);
        }

        public Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, FieldInfoDto field, object initialValue)
        {
            if (!_sanitizers.ContainsKey(field.RelativityDataType))
            {
                throw new InvalidOperationException($"No field sanitizer found for given '{field.RelativityDataType}' data type.");
            }

            return _sanitizers[field.RelativityDataType].SanitizeAsync(workspaceArtifactId, itemIdentifierSourceFieldName, itemIdentifier, field.SourceFieldName, initialValue);
        }
    }
}
