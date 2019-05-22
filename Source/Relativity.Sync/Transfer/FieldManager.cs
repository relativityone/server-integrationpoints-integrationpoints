using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
		private List<FieldInfoDto> _specialFields;
		private List<FieldInfoDto> _mappedDocumentFields;
		private List<FieldInfoDto> _allFields;
		private readonly ISynchronizationConfiguration _configuration;
		private readonly IDocumentFieldRepository _documentFieldRepository;

		private readonly IList<ISpecialFieldBuilder> _specialFieldBuilders;

		public FieldManager(ISynchronizationConfiguration configuration, IDocumentFieldRepository documentFieldRepository, IEnumerable<ISpecialFieldBuilder> specialFieldBuilders)
		{
			_configuration = configuration;
			_documentFieldRepository = documentFieldRepository;
			_specialFieldBuilders = specialFieldBuilders.OrderBy(b => b.GetType().FullName).ToList();
		}

		public IEnumerable<FieldInfoDto> GetSpecialFields()
		{
			return _specialFields ?? (_specialFields = _specialFieldBuilders.SelectMany(b => b.BuildColumns()).ToList());
		}

		public async Task<IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>> CreateSpecialFieldRowValueBuildersAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			IEnumerable<Task<ISpecialFieldRowValuesBuilder>> specialFieldRowValueBuilderTasks =
				_specialFieldBuilders.Select(async b => await b.GetRowValuesBuilderAsync(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false));
			ISpecialFieldRowValuesBuilder[] specialFieldRowValueBuilders = await Task.WhenAll(specialFieldRowValueBuilderTasks).ConfigureAwait(false);

			Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder> specialFieldBuildersDictionary = new Dictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>();
			foreach (var builder in specialFieldRowValueBuilders)
			{
				foreach (var specialFieldType in builder.AllowedSpecialFieldTypes)
				{
					specialFieldBuildersDictionary.Add(specialFieldType, builder);
				}
			}

			return specialFieldBuildersDictionary;
		}

		public async Task<IList<FieldInfoDto>> GetDocumentFieldsAsync(CancellationToken token)
		{
			IList<FieldInfoDto> fields = await GetAllFieldsAsync(token).ConfigureAwait(false);
			List<FieldInfoDto> documentFields = fields.Where(f => f.IsDocumentField).OrderBy(f => f.DocumentFieldIndex).ToList();
			return documentFields;
		}

		public async Task<IList<FieldInfoDto>> GetAllFieldsAsync(CancellationToken token)
		{
			if (_allFields == null)
			{
				IEnumerable<FieldInfoDto> specialFields = GetSpecialFields();
				IEnumerable<FieldInfoDto> mappedDocumentFields = await GetMappedDocumentFieldsAsync(token).ConfigureAwait(false);
				List<FieldInfoDto> allFields = mappedDocumentFields.Concat(specialFields).ToList();
				_allFields = EnrichDocumentFieldsWithIndex(allFields);
			}

			return _allFields;
		}

		private List<FieldInfoDto> EnrichDocumentFieldsWithIndex(List<FieldInfoDto> fields)
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

		private async Task<IEnumerable<FieldInfoDto>> GetMappedDocumentFieldsAsync(CancellationToken token)
		{
			if (_mappedDocumentFields == null)
			{
				List<FieldInfoDto> fieldInfos = _configuration.FieldMappings.Select(CreateFieldInfoFromFieldMap).ToList();
				_mappedDocumentFields = await EnrichDocumentFieldsWithRelativityDataTypesAsync(fieldInfos, token).ConfigureAwait(false);
			}
			return _mappedDocumentFields;
		}

		private async Task<List<FieldInfoDto>> EnrichDocumentFieldsWithRelativityDataTypesAsync(List<FieldInfoDto> fields, CancellationToken token)
		{
			if (fields.Count == 0)
			{
				return fields;
			}

			IDictionary<string, RelativityDataType> fieldNameToFieldType = await GetRelativityDataTypesForFieldsAsync(fields, token).ConfigureAwait(false);
			foreach (var field in fields)
			{
				field.RelativityDataType = fieldNameToFieldType[field.DisplayName];
			}

			return fields;
		}
		
		private async Task<IDictionary<string, RelativityDataType>> GetRelativityDataTypesForFieldsAsync(IEnumerable<FieldInfoDto> fields, CancellationToken token)
		{
			ICollection<string> fieldNames = fields.Select(f => f.DisplayName).ToArray();
			return await _documentFieldRepository.GetRelativityDataTypesForFieldsByFieldNameAsync(_configuration.SourceWorkspaceArtifactId, fieldNames, token)
				.ConfigureAwait(false);
		}

		private FieldInfoDto CreateFieldInfoFromFieldMap(FieldMap fieldMap)
		{
			return FieldInfoDto.DocumentField(fieldMap.SourceField.DisplayName);
		}
	}
}
