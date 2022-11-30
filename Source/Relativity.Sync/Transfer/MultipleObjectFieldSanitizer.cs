using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Returns a scalar representation of the selected objects' names given the value of the field,
    /// which is this case is assumed to be an array of <see cref="RelativityObjectValue"/>. Import
    /// API expects a string where each object name is separated by a multi-value delimiter.
    /// </summary>
    internal sealed class MultipleObjectFieldSanitizer : IExportFieldSanitizer
    {
        private readonly IDocumentSynchronizationConfiguration _configuration;
        private readonly JSONSerializer _serializer = new JSONSerializer();

        public MultipleObjectFieldSanitizer(IDocumentSynchronizationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public RelativityDataType SupportedType => RelativityDataType.MultipleObject;

        public Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
        {
            if (initialValue == null)
            {
                return Task.FromResult(initialValue);
            }

            // We have to re-serialize and deserialize the value from Export API due to REL-250554.
            RelativityObjectValue[] objectValues;
            try
            {
                objectValues = _serializer.Deserialize<RelativityObjectValue[]>(initialValue.ToString());
            }
            catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
            {
                throw new InvalidExportFieldValueException(
                    $"Expected value to be deserializable to {typeof(RelativityObjectValue[])}, but instead type was {initialValue.GetType()}.",
                    ex);
            }

            if (objectValues.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                throw new InvalidExportFieldValueException($"Some values in MultiObject field are null or contain only white-space characters.");
            }

            char multiValueDelimiter = _configuration.MultiValueDelimiter;
            bool ContainsDelimiter(string x) => x.Contains(multiValueDelimiter);

            List<string> names = objectValues.Select(x => x.Name).ToList();
            if (names.Any(ContainsDelimiter))
            {
                throw new InvalidExportFieldValueException(
                    $"The identifiers of the objects in Multiple Object field contain the character specified as the multi-value delimiter ('ASCII {(int)multiValueDelimiter}'). " +
                    $"Rename these objects to not contain delimiter.");
            }

            string multiValueDelimiterString = char.ToString(multiValueDelimiter);
            string combinedNames = string.Join(multiValueDelimiterString, names);
            return Task.FromResult<object>(combinedNames);
        }
    }
}
