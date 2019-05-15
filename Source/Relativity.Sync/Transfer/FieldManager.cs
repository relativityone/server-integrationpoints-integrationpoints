using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
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
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;
		private readonly ISynchronizationConfiguration _configuration;
		private readonly ISourceServiceFactoryForUser _serviceFactory;

		public IList<ISpecialFieldBuilder> SpecialFieldBuilders { get; }

		public FieldManager(ISynchronizationConfiguration configuration, ISourceServiceFactoryForUser serviceFactory, IEnumerable<ISpecialFieldBuilder> specialFieldBuilders)
		{
			_configuration = configuration;
			_serviceFactory = serviceFactory;
			SpecialFieldBuilders = specialFieldBuilders.OrderBy(b => b.GetType().FullName).ToList();
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

		private async Task<Dictionary<string, RelativityDataType>> GetRelativityDataTypesForFields(List<FieldInfo> fields)
		{
			string fieldNames = string.Join(", ", fields.Select(info => $"'{info.DisplayName}'"));
			QueryResultSlim result;
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef {Name = "Field"},
					Condition = $"'Name' IN [{fieldNames}] AND 'Object Type Artifact Type ID' == {_DOCUMENT_ARTIFACT_TYPE_ID}",
					Fields = new[] {new FieldRef {Name = "Field Type"}, new FieldRef {Name = "Name"}}
				};
				result = await objectManager.QuerySlimAsync(_configuration.SourceWorkspaceArtifactId, request, 0, fields.Count).ConfigureAwait(false);
			}

			Dictionary<string, RelativityDataType> parsedResult = result.Objects.ToDictionary(obj => (string) obj.Values[1], obj => ((string) obj.Values[0]).ToRelativityDataType());
			return parsedResult;
		}

		public async Task<List<FieldInfo>> GetDocumentFields()
		{
			return (await GetAllFields().ConfigureAwait(false)).Where(f => f.IsDocumentField).OrderBy(f => f.DocumentFieldIndex).ToList();
		}

		public IEnumerable<FieldInfo> GetSpecialFields()
		{
			return _specialFields ?? (_specialFields = SpecialFieldBuilders.SelectMany(b => b.BuildColumns()).ToList());
		}

		private async Task<IEnumerable<FieldInfo>> GetMappedDocumentFields()
		{
			if (_mappedDocumentFields == null)
			{
				_mappedDocumentFields = await EnrichDocumentFieldsWithRelativityDataTypes(_configuration.FieldMappings.Select(CreateFieldInfoFromFieldMap).ToList()).ConfigureAwait(false);
			}
			return _mappedDocumentFields;

		}

		private FieldInfo CreateFieldInfoFromFieldMap(FieldMap fieldMap)
		{
			return new FieldInfo {DisplayName = fieldMap.SourceField.DisplayName, IsDocumentField = true};
		}
	}
}
