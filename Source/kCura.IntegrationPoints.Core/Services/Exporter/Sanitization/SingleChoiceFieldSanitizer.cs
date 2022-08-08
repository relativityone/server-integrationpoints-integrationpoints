using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    /// <summary>
    /// Returns a choice's name given its exported value, which in this case is assumed
    /// to be a <see cref="ChoiceDto"/>. Import API expects the choice name instead of e.g.
    /// its artifact ID.
    /// </summary>
    internal sealed class SingleChoiceFieldSanitizer : IExportFieldSanitizer
    {
        private readonly ISanitizationDeserializer _sanitizationDeserializer;

        public SingleChoiceFieldSanitizer(ISanitizationDeserializer sanitizationDeserializer)
        {
            _sanitizationDeserializer = sanitizationDeserializer;
        }

        public FieldTypeHelper.FieldType SupportedType => FieldTypeHelper.FieldType.Code;

        public Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
        {
            if (initialValue == null)
            {
                return Task.FromResult((object) null);
            }

            // We have to re-serialize and deserialize the value from Export API due to REL-250554.
            ChoiceDto choice = _sanitizationDeserializer.DeserializeAndValidateExportFieldValue<ChoiceDto>(initialValue);

            if (string.IsNullOrWhiteSpace(choice.Name))
            {
                throw new InvalidExportFieldValueException($"Expected input to be deserializable to type {typeof(Choice)} and name to not be null or empty.");
            }

            string value = choice.Name;
            return Task.FromResult<object>(value);
        }
    }
}
