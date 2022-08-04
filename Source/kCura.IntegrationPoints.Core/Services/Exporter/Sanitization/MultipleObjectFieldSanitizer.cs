using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    /// <summary>
    /// Returns a scalar representation of the selected objects' names given the value of the field,
    /// which is this case is assumed to be an array of <see cref="RelativityObjectValue"/>. Import
    /// API expects a string where each object name is separated by a multi-value delimiter.
    /// </summary>
    internal sealed class MultipleObjectFieldSanitizer : IExportFieldSanitizer
    {
        private readonly ISanitizationDeserializer _sanitizationDeserializer;
        private readonly char _multiValueDelimiter;

        public MultipleObjectFieldSanitizer(ISanitizationDeserializer sanitizationDeserializer)
        {
            _sanitizationDeserializer = sanitizationDeserializer;
            _multiValueDelimiter = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;
        }

        public FieldTypeHelper.FieldType SupportedType => FieldTypeHelper.FieldType.Objects;

        public Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
        {
            if (initialValue == null)
            {
                return Task.FromResult((object) null);
            }

            // We have to re-serialize and deserialize the value from Export API due to REL-250554.
            RelativityObjectValue[] objectValues = _sanitizationDeserializer.DeserializeAndValidateExportFieldValue<RelativityObjectValue[]>(initialValue);

            if (objectValues.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                throw new InvalidExportFieldValueException($"Some values in MultiObject field are null or contain only white-space characters.");
            }

            bool ContainsDelimiter(string x) => x.Contains(_multiValueDelimiter);

            List<string> names = objectValues.Select(x => x.Name).ToList();
            if (names.Any(ContainsDelimiter))
            {
                throw new InvalidExportFieldValueException(
                    $"The identifiers of the objects in Multiple Object field contain the character specified as the multi-value delimiter ('ASCII {(int)_multiValueDelimiter}'). " +
                    $"Rename these objects to not contain delimiter.");
            }

            string multiValueDelimiterString = char.ToString(_multiValueDelimiter);
            string combinedNames = string.Join(multiValueDelimiterString, names);
            return Task.FromResult<object>(combinedNames);
        }
    }
}
