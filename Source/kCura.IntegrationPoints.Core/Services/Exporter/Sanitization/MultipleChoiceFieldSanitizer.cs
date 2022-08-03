using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Relativity;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    /// <summary>
    /// Returns a scalar representation of the selected choices' names given the value of the field,
    /// which is this case is assumed to be an array of <see cref="ChoiceDto"/>. Import API expects a
    /// string where each choice is separated by a multi-value delimiter and each parent/child choice
    /// is separated by a nested-value delimiter.
    /// </summary>
    internal sealed class MultipleChoiceFieldSanitizer : IExportFieldSanitizer
    {
        private readonly IChoiceRepository _choiceCache;
        private readonly IChoiceTreeToStringConverter _choiceTreeConverter;
        private readonly ISanitizationDeserializer _sanitizationDeserializer;
        private readonly char _multiValueDelimiter;
        private readonly char _nestedValueDelimiter;

        public MultipleChoiceFieldSanitizer(IChoiceRepository choiceCache, IChoiceTreeToStringConverter choiceTreeConverter, ISanitizationDeserializer sanitizationDeserializer)
        {
            _choiceCache = choiceCache;
            _choiceTreeConverter = choiceTreeConverter;
            _sanitizationDeserializer = sanitizationDeserializer;
            _multiValueDelimiter = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;
            _nestedValueDelimiter = IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER;
        }

        public FieldTypeHelper.FieldType SupportedType => FieldTypeHelper.FieldType.MultiCode;

        public async Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
        {
            if (initialValue == null)
            {
                return null;
            }

            // We have to re-serialize and deserialize the value from Export API due to REL-250554.
            ChoiceDto[] choices = _sanitizationDeserializer.DeserializeAndValidateExportFieldValue<ChoiceDto[]>(initialValue);

            if (choices.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                throw new InvalidExportFieldValueException($"One or more choices are null or contain only white-space characters.");
            }

            bool ContainsDelimiter(string x) => x.Contains(_multiValueDelimiter) || x.Contains(_nestedValueDelimiter);

            List<string> names = choices.Select(x => x.Name).ToList();
            if (names.Any(ContainsDelimiter))
            {
                throw new InvalidExportFieldValueException(
                    $"The identifiers of the choices contain the character specified as the multi-value delimiter ('ASCII {(int)_multiValueDelimiter}')" +
                    $" or nested value delimiter ('ASCII {(int)_nestedValueDelimiter}'). Rename choices to not contain delimiters.");
            }

            IList<ChoiceWithParentInfoDto> choicesFlatList = await _choiceCache.QueryChoiceWithParentInfoAsync(choices, choices).ConfigureAwait(false);
            IList<ChoiceWithChildInfoDto> choicesTree = BuildChoiceTree(choicesFlatList, null); // start with null to designate the roots of the tree

            string treeString = _choiceTreeConverter.ConvertTreeToString(choicesTree);
            return treeString;
        }

        private IList<ChoiceWithChildInfoDto> BuildChoiceTree(IList<ChoiceWithParentInfoDto> flatList, int? parentArtifactID)
        {
            var tree = new List<ChoiceWithChildInfoDto>();
            foreach (ChoiceWithParentInfoDto choice in flatList.Where(x => x.ParentArtifactID == parentArtifactID))
            {
                IList<ChoiceWithChildInfoDto> choiceChildren = BuildChoiceTree(flatList, choice.ArtifactID);
                var choiceWithChildren = new ChoiceWithChildInfoDto(choice.ArtifactID, choice.Name, choiceChildren);
                tree.Add(choiceWithChildren);
            }
            return tree;
        }
    }
}
