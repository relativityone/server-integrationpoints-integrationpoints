using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
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
		private List<FieldInfoDto> _mappedFieldsCache;
		private IReadOnlyList<FieldInfoDto> _imageAllFields;
		private IReadOnlyList<FieldInfoDto> _nativeAllFields;
		
		private readonly IFieldConfiguration _configuration;
		private readonly IObjectFieldTypeRepository _objectFieldTypeRepository;
		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;

		private readonly IList<INativeSpecialFieldBuilder> _nativeSpecialFieldBuilders;
		private readonly IList<IImageSpecialFieldBuilder> _imageSpecialFieldBuilders;
		private readonly ISyncLog _logger;

		public FieldManager(IFieldConfiguration configuration, IObjectFieldTypeRepository objectFieldTypeRepository,
			IEnumerable<INativeSpecialFieldBuilder> nativeSpecialFieldBuilders, IEnumerable<IImageSpecialFieldBuilder> imageSpecialFieldBuilders,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin, ISourceServiceFactoryForUser serviceFactoryForUser, ISyncLog logger)
		{
			_configuration = configuration;
			_objectFieldTypeRepository = objectFieldTypeRepository;
			_nativeSpecialFieldBuilders = OmitNativeInfoFieldsBuildersIfNotNeeded(configuration, nativeSpecialFieldBuilders).OrderBy(b => b.GetType().FullName).ToList();
			_imageSpecialFieldBuilders = imageSpecialFieldBuilders.ToList();
			_serviceFactoryForAdmin = serviceFactoryForAdmin;
            _serviceFactoryForUser = serviceFactoryForUser;
			_logger = logger;
        }

		public IEnumerable<FieldInfoDto> GetNativeSpecialFields()
			=> _nativeSpecialFieldBuilders.SelectMany(b => b.BuildColumns());

		public IEnumerable<FieldInfoDto> GetImageSpecialFields()
			=> _imageSpecialFieldBuilders.SelectMany(b => b.BuildColumns());

		public async Task<IDictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder>> CreateNativeSpecialFieldRowValueBuildersAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds)
		{
			IEnumerable<INativeSpecialFieldRowValuesBuilder> specialFieldRowValueBuilders = await _nativeSpecialFieldBuilders
				.SelectAsync(specialFieldBuilder => specialFieldBuilder.GetRowValuesBuilderAsync(sourceWorkspaceArtifactId, documentArtifactIds))
				.ConfigureAwait(false);

			var specialFieldBuildersDictionary = new Dictionary<SpecialFieldType, INativeSpecialFieldRowValuesBuilder>();
			foreach (INativeSpecialFieldRowValuesBuilder builder in specialFieldRowValueBuilders)
			{
				foreach (SpecialFieldType specialFieldType in builder.AllowedSpecialFieldTypes)
				{
					specialFieldBuildersDictionary.Add(specialFieldType, builder);
				}
			}

			return specialFieldBuildersDictionary;
		}

		public async Task<IDictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder>> CreateImageSpecialFieldRowValueBuildersAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds)
		{
			IEnumerable<IImageSpecialFieldRowValuesBuilder> specialFieldRowValueBuilders = await _imageSpecialFieldBuilders
				.SelectAsync(specialFieldBuilder => specialFieldBuilder.GetRowValuesBuilderAsync(sourceWorkspaceArtifactId, documentArtifactIds))
				.ConfigureAwait(false);

			var specialFieldBuildersDictionary = new Dictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder>();
			foreach (IImageSpecialFieldRowValuesBuilder builder in specialFieldRowValueBuilders)
			{
				foreach (SpecialFieldType specialFieldType in builder.AllowedSpecialFieldTypes)
				{
					specialFieldBuildersDictionary.Add(specialFieldType, builder);
				}
			}

			return specialFieldBuildersDictionary;
		}

		public async Task<IReadOnlyList<FieldInfoDto>> GetNativeAllFieldsAsync(CancellationToken token)
		{
			if (_nativeAllFields == null)
			{
				_nativeAllFields = await GetAllFieldsInternalAsync(GetNativeSpecialFields, token).ConfigureAwait(false);
			}

			return _nativeAllFields;
		}

		public async Task<IReadOnlyList<FieldInfoDto>> GetImageAllFieldsAsync(CancellationToken token)
		{
			if (_imageAllFields == null)
			{
				_imageAllFields = await GetAllFieldsInternalAsync(GetImageSpecialFields, token).ConfigureAwait(false);
			}

			return _imageAllFields;
		}
		
		public async Task<IList<FieldInfoDto>> GetDocumentTypeFieldsAsync(CancellationToken token)
		{
			IReadOnlyList<FieldInfoDto> fields = await GetNativeAllFieldsAsync(token).ConfigureAwait(false);
			List<FieldInfoDto> documentFields = fields.Where(f => f.IsDocumentField).OrderBy(f => f.DocumentFieldIndex).ToList();
			return documentFields;
		}

		public async Task<FieldInfoDto> GetObjectIdentifierFieldAsync(CancellationToken token)
		{
			IEnumerable<FieldInfoDto> mappedFields = await GetMappedFieldsAsync(token).ConfigureAwait(false);

			FieldInfoDto identifierField = mappedFields.First(f => f.IsIdentifier);
			identifierField.DocumentFieldIndex = 0;

			return identifierField;
		}

		public async Task<IList<FieldInfoDto>> GetMappedFieldsAsync(CancellationToken token)
		{
			if (_mappedFieldsCache == null)
			{
				List<FieldInfoDto> fieldInfos = GetAllAvailableFieldsToMap();
				_mappedFieldsCache = await EnrichFieldsWithRelativityDataTypesAsync(fieldInfos, token).ConfigureAwait(false);
				EnrichFieldsWithIndex(_mappedFieldsCache);
			}
			return _mappedFieldsCache;
		}

        public List<FieldInfoDto> GetAllAvailableFieldsToMap()
        {
            List<FieldInfoDto> fieldInfos = _configuration.GetFieldMappings().Select(CreateFieldInfoFromFieldMap).ToList();
            return fieldInfos;
        }

        public async Task<IReadOnlyList<FieldInfoDto>> GetMappedFieldsNonDocumentWithoutLinksAsync(
			CancellationToken token)
		{
			IList<FieldInfoDto> fieldInfos = await GetMappedFieldsAsync(token).ConfigureAwait(false);

			string[] namesOfFieldsOfTheSameType = await GetSameTypeFieldNamesAsync(_configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);

			List<FieldInfoDto> result = fieldInfos.Where(f => !namesOfFieldsOfTheSameType.Any( n => n == f.SourceFieldName)).ToList();
			return  EnrichFieldsWithIndex(result);
		}

		public async Task<IReadOnlyList<FieldInfoDto>> GetMappedFieldsNonDocumentForLinksAsync(CancellationToken token)
		{
			IList<FieldInfoDto> fieldInfos = await GetMappedFieldsAsync(token).ConfigureAwait(false);

			string[] namesOfFieldsOfTheSameType = await GetSameTypeFieldNamesAsync(_configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);

			List<FieldInfoDto> result = fieldInfos.Where(f => f.IsIdentifier || namesOfFieldsOfTheSameType.Any( n => n == f.SourceFieldName)).ToList();
			return EnrichFieldsWithIndex(result);
		}

		public async Task<string[]> GetSameTypeFieldNamesAsync(int workspaceId)
		{
			string rdoTypeName = await GetRdoTypeNameAsync(_configuration.SourceWorkspaceArtifactId, _configuration.RdoArtifactTypeId);
			
			using (var objectManager =
				await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
					Condition =
						$"('Associative Object Type' == '{rdoTypeName}') AND ('Object Type' == '{rdoTypeName}')" +
						$" AND (NOT ('Name' LIKE ['::']))" +
						$" AND ('Field Type' IN ['Multiple Object', 'Single Object'])",
					IncludeNameInQueryResult = true
				};

				QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, Int32.MaxValue)
					.ConfigureAwait(false);
				return result.Objects.Select(x => x.Name).ToArray();
			}
		}

        public async Task<Dictionary<string, object>> GetFieldsMappingSummaryAsync(CancellationToken token)
        {
			IList<FieldInfoDto> nonDocumentFields = await GetMappedFieldsAsync(token).ConfigureAwait(false);

            Task<Dictionary<string, RelativityObjectSlim>> sourceFieldsDetailsTask = GetFieldsDetailsAsync(_configuration.SourceWorkspaceArtifactId,
                nonDocumentFields.Where(x => x.RelativityDataType == RelativityDataType.LongText)
                    .Select(x => x.SourceFieldName), _configuration.RdoArtifactTypeId, token);

            Task<Dictionary<string, RelativityObjectSlim>> destinationFieldsDetailsTask = GetFieldsDetailsAsync(_configuration.DestinationWorkspaceArtifactId,
                nonDocumentFields.Where(x => x.RelativityDataType == RelativityDataType.LongText)
                    .Select(x => x.DestinationFieldName), _configuration.RdoArtifactTypeId, token);

            await Task.WhenAll(sourceFieldsDetailsTask, destinationFieldsDetailsTask).ConfigureAwait(false);
            Dictionary<string, object> fieldsMappingSummary = GetFieldsMappingSummary(nonDocumentFields, sourceFieldsDetailsTask.Result, destinationFieldsDetailsTask.Result);
            return fieldsMappingSummary;
        }

		private async Task<string> GetRdoTypeNameAsync(int workspaceArtifactId, int rdoArtifactTypeId)
		{
			using (var objectManager =
				await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var query = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						ArtifactTypeID = (int)ArtifactType.ObjectType
					},
					Condition = $"'Artifact Type ID' == {rdoArtifactTypeId}",
					IncludeNameInQueryResult = true
				};

				QueryResult result =
					await objectManager.QueryAsync(workspaceArtifactId, query, 0, 1).ConfigureAwait(false);

				if (result.Objects.Count != 1)
				{
					_logger.LogError("Rdo with ArtifactTypeId {artifactTypeId} does not exist", rdoArtifactTypeId);
					throw new SyncException($"Rdo with ArtifactTypeId {rdoArtifactTypeId} does not exist");
				}
                
				return result.Objects.Single().Name;
			}
		}

		private static IEnumerable<INativeSpecialFieldBuilder> OmitNativeInfoFieldsBuildersIfNotNeeded(IFieldConfiguration configuration, IEnumerable<INativeSpecialFieldBuilder> nativeSpecialFieldBuilders)
		{
			if (configuration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.DoNotImportNativeFiles)
			{
				nativeSpecialFieldBuilders = nativeSpecialFieldBuilders.Where(x => !(x is INativeInfoFieldsBuilder));
			}

			return nativeSpecialFieldBuilders;
		}

		private async Task<IReadOnlyList<FieldInfoDto>> GetAllFieldsInternalAsync(Func<IEnumerable<FieldInfoDto>> specialFieldsProvider, CancellationToken token)
		{
			IList<FieldInfoDto> specialFields = specialFieldsProvider().ToList();
			IList<FieldInfoDto> mappedDocumentFields = await GetMappedFieldsAsync(token).ConfigureAwait(false);
			List<FieldInfoDto> allFields = MergeFieldCollections(specialFields, mappedDocumentFields);
			return EnrichFieldsWithIndex(allFields);
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
						mdf.DestinationFieldName.Equals(specialField.DestinationFieldName, StringComparison.InvariantCultureIgnoreCase))
				})
				.FirstOrDefault(field =>
					field.DocumentField != null
					&& (!field.SpecialField.IsDocumentField || !field.SpecialField.SourceFieldName.Equals(field.DocumentField.SourceFieldName, StringComparison.InvariantCultureIgnoreCase))
					)?.SpecialField;

			if (invalidSpecialField != null)
			{
				string specialFieldParams = $"{nameof(invalidSpecialField.SpecialFieldType)}: {invalidSpecialField.SpecialFieldType}; {invalidSpecialField.IsDocumentField}: {invalidSpecialField.IsDocumentField};";
				string message = $"Special field destination name conflicts with mapped field destination name. Special field params: {specialFieldParams}";
				throw new InvalidOperationException(message);
			}
		}

		private static bool FieldInfosHaveSameSourceAndDestination(FieldInfoDto first, FieldInfoDto second)
		{
			return first.SourceFieldName.Equals(second.SourceFieldName, StringComparison.InvariantCultureIgnoreCase)
				   && first.DestinationFieldName.Equals(second.DestinationFieldName, StringComparison.InvariantCultureIgnoreCase);
		}

		private List<FieldInfoDto> EnrichFieldsWithIndex(List<FieldInfoDto> fields)
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

		private async Task<List<FieldInfoDto>> EnrichFieldsWithRelativityDataTypesAsync(List<FieldInfoDto> fields, CancellationToken token)
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
			return _objectFieldTypeRepository.GetRelativityDataTypesForFieldsByFieldNameAsync(_configuration.SourceWorkspaceArtifactId, _configuration.RdoArtifactTypeId, fieldNames, token);
		}

		private FieldInfoDto CreateFieldInfoFromFieldMap(FieldMap fieldMap)
		{
			return FieldInfoDto.DocumentField(fieldMap.SourceField.DisplayName, fieldMap.DestinationField.DisplayName, fieldMap.SourceField.IsIdentifier);
		}

		private async Task<Dictionary<string, RelativityObjectSlim>> GetFieldsDetailsAsync(int workspaceId,
			IEnumerable<string> fieldNames, int rdoArtifactTypeId, CancellationToken token)
		{
			if (fieldNames == null || !fieldNames.Any())
			{
				return new Dictionary<string, RelativityObjectSlim>();
			}

			ICollection<string> requestedFieldNames = new HashSet<string>(fieldNames);

			IEnumerable<string> formattedFieldNames =
				requestedFieldNames.Select(KeplerQueryHelpers.EscapeForSingleQuotes).Select(f => $"'{f}'");
			string concatenatedFieldNames = string.Join(", ", formattedFieldNames);

			QueryRequest request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { Name = "Field" },
				Condition =
					$"'Name' IN [{concatenatedFieldNames}] AND 'Object Type Artifact Type ID' == {rdoArtifactTypeId}",
				Fields = new[]
				{
					new FieldRef {Name = "Name"},
					new FieldRef {Name = "Enable Data Grid"}
				}
			};

			QueryResultSlim result;
			using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				try
				{
					const int start = 0;
					result = await objectManager
						.QuerySlimAsync(workspaceId, request, start, requestedFieldNames.Count, token)
						.ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex,
						"Service call failed while querying non document fields in workspace {workspaceArtifactId} for mapping details",
						workspaceId);
					throw new SyncKeplerException(
						$"Service call failed while querying non document fields in workspace {workspaceId} for mapping details",
						ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex,
						"Failed to query non document fields in workspace {workspaceArtifactId} for mapping details",
						workspaceId);
					throw new SyncKeplerException(
						$"Failed to query non document fields in workspace {workspaceId} for mapping details", ex);
				}
			}

			return result.Objects.ToDictionary(x => x.Values[0].ToString(), x => x);
		}

		private Dictionary<string, object> GetFieldsMappingSummary(IList<FieldInfoDto> mappings,
			IDictionary<string, RelativityObjectSlim> sourceLongTextFieldsDetails,
			IDictionary<string, RelativityObjectSlim> destinationLongTextFieldsDetails)
		{
			const string keyFormat = "[{0}] <--> [{1}]";

			Dictionary<string, int> mappingSummary = mappings
				.GroupBy(x => x.RelativityDataType, x => x)
				.ToDictionary(x => x.Key.ToString(), x => x.Count());

			Dictionary<string, Dictionary<string, Dictionary<string, object>>> longTextFields = mappings
				.Where(x => x.RelativityDataType == RelativityDataType.LongText)
				.ToDictionary(x => string.Format(keyFormat, x.SourceFieldName, x.DestinationFieldName), x =>
					new Dictionary<string, Dictionary<string, object>>
					{
						{
							"Source", new Dictionary<string, object>()
							{
								{"ArtifactId", sourceLongTextFieldsDetails[x.SourceFieldName].ArtifactID},
								{"DataGridEnabled", sourceLongTextFieldsDetails[x.SourceFieldName].Values[1]}
							}
						},
						{
							"Destination", new Dictionary<string, object>()
							{
								{"ArtifactId", destinationLongTextFieldsDetails[x.DestinationFieldName].ArtifactID},
								{"DataGridEnabled", destinationLongTextFieldsDetails[x.DestinationFieldName].Values[1]}
							}
						}
					});

            const string extractedTextFieldName = "Extracted Text";
			string extractedTextKey = string.Format(keyFormat, extractedTextFieldName, extractedTextFieldName);

			Dictionary<string, object> summary = new Dictionary<string, object>()
			{
				{ "FieldMapping", mappingSummary },
                { "ExtractedText", longTextFields.TryGetValue(extractedTextKey, out var v) ? v : null },
                { "LongText", longTextFields.Where(x => x.Key != extractedTextKey).Select(x => x.Value).ToArray() }
			};

			return summary;
		}
	}
}
