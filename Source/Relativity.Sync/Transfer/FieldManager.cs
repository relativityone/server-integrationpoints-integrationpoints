using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
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
        private readonly IFieldConfiguration _configuration;
        private readonly IObjectFieldTypeRepository _objectFieldTypeRepository;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IList<INativeSpecialFieldBuilder> _nativeSpecialFieldBuilders;
        private readonly IList<IImageSpecialFieldBuilder> _imageSpecialFieldBuilders;
        private readonly IAPILog _logger;

        private List<FieldInfoDto> _mappedFieldsCache;
        private IReadOnlyList<FieldInfoDto> _imageAllFields;
        private IReadOnlyList<FieldInfoDto> _nativeAllFields;

        public FieldManager(
            IFieldConfiguration configuration,
            IObjectFieldTypeRepository objectFieldTypeRepository,
            IEnumerable<INativeSpecialFieldBuilder> nativeSpecialFieldBuilders,
            IEnumerable<IImageSpecialFieldBuilder> imageSpecialFieldBuilders,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin,
            IAPILog logger)
        {
            _configuration = configuration;
            _objectFieldTypeRepository = objectFieldTypeRepository;
            _nativeSpecialFieldBuilders = OmitNativeInfoFieldsBuildersIfNotNeeded(configuration, nativeSpecialFieldBuilders).OrderBy(b => b.GetType().FullName).ToList();
            _imageSpecialFieldBuilders = imageSpecialFieldBuilders.ToList();
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
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

            List<FieldInfoDto> result = fieldInfos.Where(f => !namesOfFieldsOfTheSameType.Any(n => n == f.SourceFieldName)).ToList();
            return EnrichFieldsWithIndex(result);
        }

        public async Task<IReadOnlyList<FieldInfoDto>> GetMappedFieldsNonDocumentForLinksAsync(CancellationToken token)
        {
            IList<FieldInfoDto> fieldInfos = await GetMappedFieldsAsync(token).ConfigureAwait(false);

            string[] namesOfFieldsOfTheSameType = await GetSameTypeFieldNamesAsync(_configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);

            List<FieldInfoDto> result = fieldInfos.Where(f => f.IsIdentifier || namesOfFieldsOfTheSameType.Any(n => n == f.SourceFieldName)).ToList();
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

                QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue)
                    .ConfigureAwait(false);
                return result.Objects.Select(x => x.Name).ToArray();
            }
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
                    var fieldInfoDto = new FieldInfoDto(
                        matchingSpecialField.SpecialFieldType,
                        mappedDocumentField.SourceFieldName,
                        mappedDocumentField.DestinationFieldName,
                        mappedDocumentField.IsIdentifier,
                        mappedDocumentField.IsDocumentField);
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
                    && (!field.SpecialField.IsDocumentField || !field.SpecialField.SourceFieldName.Equals(field.DocumentField.SourceFieldName, StringComparison.InvariantCultureIgnoreCase)))?.SpecialField;

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
            foreach (FieldInfoDto field in fields)
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
    }
}
