using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Represents the mapping between different fields and properties in the source and destination
	/// workspaces. This class should be the source of truth for what fields are mapped and how between
	/// the various Relativity APIs.
	/// </summary>
	internal sealed class FieldManager : IFieldManager
	{
		private List<FieldInfo> _specialFields;
		private List<FieldInfo> _mappedDocumentFields;
		private List<FieldInfo> _allFields;
		private readonly ISynchronizationConfiguration _configuration;
		private readonly IDocumentFieldRepository _documentFieldRepository;

		public IList<ISpecialFieldBuilder> SpecialFieldBuilders { get; }

		public FieldManager(ISynchronizationConfiguration configuration, IDocumentFieldRepository documentFieldRepository, IEnumerable<ISpecialFieldBuilder> specialFieldBuilders)
		{
			_configuration = configuration;
			_documentFieldRepository = documentFieldRepository;
			SpecialFieldBuilders = specialFieldBuilders.OrderBy(b => b.GetType().FullName).ToList();
		}

		public IEnumerable<FieldInfo> GetSpecialFields()
		{
			return _specialFields ?? (_specialFields = SpecialFieldBuilders.SelectMany(b => b.BuildColumns()).ToList());
		}

		public async Task<List<FieldInfo>> GetDocumentFields()
		{
			return (await GetAllFields().ConfigureAwait(false)).Where(f => f.IsDocumentField).OrderBy(f => f.DocumentFieldIndex).ToList();
		}

		public async Task<List<FieldInfo>> GetAllFields()
		{
			if (_allFields == null)
			{
				IEnumerable<FieldInfo> specialFields = GetSpecialFields();
				List<FieldInfo> allFields = (await GetMappedDocumentFields().ConfigureAwait(false)).Concat(specialFields).ToList();
				_allFields = EnrichDocumentFieldsWithIndex(allFields);
				return _allFields;
			}

			return _allFields;
		}

		private List<FieldInfo> EnrichDocumentFieldsWithIndex(List<FieldInfo> fields)
		{
			int currentIndex = 0;
			foreach (var field in fields)
			{
				if (field.IsDocumentField)
				{
					field.DocumentFieldIndex = currentIndex;
					currentIndex++;
				}
			}

			return fields;
		}

		private async Task<Dictionary<string, RelativityDataType>> GetRelativityDataTypesForFields(List<FieldInfo> fields)
		{
			return await _documentFieldRepository.GetRelativityDataTypesForFieldsByFieldName(_configuration.SourceWorkspaceArtifactId, fields.Select(f => f.DisplayName).ToArray())
				.ConfigureAwait(false);
		}

		private async Task<IEnumerable<FieldInfo>> GetMappedDocumentFields()
		{
			if (_mappedDocumentFields == null)
			{
				_mappedDocumentFields = await EnrichDocumentFieldsWithRelativityDataTypes(_configuration.FieldMappings.Select(CreateFieldInfoFromFieldMap).ToList()).ConfigureAwait(false);
			}
			return _mappedDocumentFields;
		}

		private async Task<List<FieldInfo>> EnrichDocumentFieldsWithRelativityDataTypes(List<FieldInfo> fields)
		{
			if (fields.Count == 0)
			{
				return fields;
			}

			Dictionary<string, RelativityDataType> parsedResult = await GetRelativityDataTypesForFields(fields).ConfigureAwait(false);
			foreach (var field in fields)
			{
				field.RelativityDataType = parsedResult[field.DisplayName];
			}

			return fields;
		}

		private FieldInfo CreateFieldInfoFromFieldMap(FieldMap fieldMap)
		{
			return new FieldInfo {DisplayName = fieldMap.SourceField.DisplayName, IsDocumentField = true};
		}
	}
}
