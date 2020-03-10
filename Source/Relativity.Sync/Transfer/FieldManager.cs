using System;
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
		private readonly IFieldConfiguration _configuration;
		private readonly IDocumentFieldRepository _documentFieldRepository;

		private readonly IList<ISpecialFieldBuilder> _specialFieldBuilders;

		public FieldManager(IFieldConfiguration configuration, IDocumentFieldRepository documentFieldRepository, IEnumerable<ISpecialFieldBuilder> specialFieldBuilders)
		{
			_configuration = configuration;
			_documentFieldRepository = documentFieldRepository;
			_specialFieldBuilders = specialFieldBuilders.OrderBy(b => b.GetType().FullName).ToList();
		}

		public IList<FieldInfoDto> GetSpecialFields()
		{
			return _specialFields ?? (_specialFields = _specialFieldBuilders.SelectMany(b => b.BuildColumns()).ToList());
		}

		public async Task<IDictionary<SpecialFieldType, ISpecialFieldRowValuesBuilder>> CreateSpecialFieldRowValueBuildersAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			IEnumerable<ISpecialFieldRowValuesBuilder> specialFieldRowValueBuilders = await _specialFieldBuilders
				.SelectAsync(specialFieldBuilder => specialFieldBuilder.GetRowValuesBuilderAsync(sourceWorkspaceArtifactId, documentArtifactIds))
				.ConfigureAwait(false);

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
			IReadOnlyList<FieldInfoDto> fields = await GetAllFieldsAsync(token).ConfigureAwait(false);
			List<FieldInfoDto> documentFields = fields.Where(f => f.IsDocumentField).OrderBy(f => f.DocumentFieldIndex).ToList();
			return documentFields;
		}

		public async Task<FieldInfoDto> GetObjectIdentifierFieldAsync(CancellationToken token)
		{
			IEnumerable<FieldInfoDto> mappedFields = await GetDocumentFieldsAsync(token).ConfigureAwait(false);
			return mappedFields.First(f => f.IsIdentifier);
		}

		public async Task<IReadOnlyList<FieldInfoDto>> GetAllFieldsAsync(CancellationToken token)
		{
			if (_allFields == null)
			{
				IList<FieldInfoDto> specialFields = GetSpecialFields();
				IList<FieldInfoDto> mappedDocumentFields = await GetMappedDocumentFieldsAsync(token).ConfigureAwait(false);
				List<FieldInfoDto> allFields = MergeFieldCollections(specialFields, mappedDocumentFields);
				_allFields = EnrichDocumentFieldsWithIndex(allFields);
			}

			return _allFields;
		}

		private List<FieldInfoDto> MergeFieldCollections(IList<FieldInfoDto> specialFields, IList<FieldInfoDto> mappedDocumentFields)
		{
			ThrowIfSpecialFieldsInvalid(specialFields, mappedDocumentFields);

			List<FieldInfoDto> remainingSpecialFields = new List<FieldInfoDto>(specialFields);
			var result = new List<FieldInfoDto>();

			foreach (FieldInfoDto mappedDocumentField in mappedDocumentFields)
			{
				FieldInfoDto matchingSpecialField = remainingSpecialFields.FirstOrDefault(f => FieldInfosHaveSameSourceAndDestination(f, mappedDocumentField));

				if (matchingSpecialField != null)
				{
					var fieldInfoDto = new FieldInfoDto(matchingSpecialField.SpecialFieldType,
						mappedDocumentField.SourceFieldName, mappedDocumentField.DestinationFieldName,
						mappedDocumentField.IsIdentifier, mappedDocumentField.IsDocumentField);
					result.Add(fieldInfoDto);
					remainingSpecialFields.Remove(matchingSpecialField);
				}
				else
				{
					result.Add(mappedDocumentField);
				}
			}
			result.AddRange(remainingSpecialFields);

			return result;
		}

		private static void ThrowIfSpecialFieldsInvalid(IList<FieldInfoDto> specialFields, IList<FieldInfoDto> mappedDocumentFields)
		{
			FieldInfoDto invalidSpecialField = specialFields
				.Select(specialField => new
				{
					SpecialField = specialField,
					DocumentField = mappedDocumentFields.SingleOrDefault(mdf =>
						mdf.DestinationFieldName == specialField.DestinationFieldName)
				})
				.FirstOrDefault(field =>
					field.DocumentField != null && (!field.SpecialField.IsDocumentField ||
					                                field.SpecialField.SourceFieldName !=
					                                field.DocumentField.SourceFieldName))?.SpecialField;

			if (invalidSpecialField != null)
			{
				string specialFieldParams = $"{nameof(invalidSpecialField.SpecialFieldType)}: {invalidSpecialField.SpecialFieldType}; {invalidSpecialField.IsDocumentField}: {invalidSpecialField.IsDocumentField};";
				string message = $"Special field destination name conflicts with mapped field destination name. Special field params: {specialFieldParams}";
				throw new InvalidOperationException(message);
			}
		}

		private static bool FieldInfosHaveSameSourceAndDestination(FieldInfoDto first, FieldInfoDto second)
		{ 
			return first.SourceFieldName == second.SourceFieldName && first.DestinationFieldName == second.DestinationFieldName;
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

		private async Task<List<FieldInfoDto>> GetMappedDocumentFieldsAsync(CancellationToken token)
		{
			if (_mappedDocumentFields == null)
			{
				List<FieldInfoDto> fieldInfos = _configuration.GetFieldMappings().Select(CreateFieldInfoFromFieldMap).ToList();
				_mappedDocumentFields = await EnrichDocumentFieldsWithRelativityDataTypesAsync(fieldInfos, token).ConfigureAwait(false);
			}
			return _mappedDocumentFields;
		}

		private async Task<List<FieldInfoDto>> EnrichDocumentFieldsWithRelativityDataTypesAsync(List<FieldInfoDto> fields, CancellationToken token)
		{
			if (fields.Count != 0)
			{
				IDictionary<string, RelativityDataType> fieldNameToFieldType = await GetRelativityDataTypesForFieldsAsync(fields, token).ConfigureAwait(false);
				foreach (var field in fields)
				{
					field.RelativityDataType = fieldNameToFieldType[field.SourceFieldName];
				}
			}

			return fields;
		}
		
		private Task<IDictionary<string, RelativityDataType>> GetRelativityDataTypesForFieldsAsync(IEnumerable<FieldInfoDto> fields, CancellationToken token)
		{
			ICollection<string> fieldNames = fields.Select(f => f.SourceFieldName).ToArray();
			return _documentFieldRepository.GetRelativityDataTypesForFieldsByFieldNameAsync(_configuration.SourceWorkspaceArtifactId, fieldNames, token);
		}

		private FieldInfoDto CreateFieldInfoFromFieldMap(FieldMap fieldMap)
		{
			return FieldInfoDto.DocumentField(fieldMap.SourceField.DisplayName, fieldMap.DestinationField.DisplayName, fieldMap.SourceField.IsIdentifier);
		}
	}
}
