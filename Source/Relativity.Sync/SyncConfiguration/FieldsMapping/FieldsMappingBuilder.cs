using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.SyncConfiguration.FieldsMapping
{
    internal class FieldsMappingBuilder : IFieldsMappingBuilder
    {
        private readonly int _sourceWorkspaceId;
        private readonly int _destinationWorkspaceId;
        private readonly int _rdoArtifactTypeId;
        private readonly int _destinationRdoArtifactTypeId;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;

        public FieldsMappingBuilder(int sourceWorkspaceId, int destinationWorkspaceId, int rdoArtifactTypeId, int destinationRdoArtifactTypeId, ISourceServiceFactoryForAdmin serviceFactoryForAdmin)
        {
            _sourceWorkspaceId = sourceWorkspaceId;
            _destinationWorkspaceId = destinationWorkspaceId;
            _rdoArtifactTypeId = rdoArtifactTypeId;
            _destinationRdoArtifactTypeId = destinationRdoArtifactTypeId;
            _serviceFactoryForAdmin = serviceFactoryForAdmin;

            FieldsMapping = new List<FieldMap>();
        }

        public List<FieldMap> FieldsMapping { get; }

        public IFieldsMappingBuilder WithIdentifier()
        {
            if (FieldsMapping.Exists(x => x.FieldMapType == FieldMapType.Identifier))
            {
                throw InvalidFieldsMappingException.IdentifierMappedTwice();
            }

            using (var objectManager = _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                FieldEntry sourceField = ReadIdentifierFieldAsync(_sourceWorkspaceId, _rdoArtifactTypeId, objectManager).GetAwaiter().GetResult();
                FieldEntry destinationField = ReadIdentifierFieldAsync(_destinationWorkspaceId, _destinationRdoArtifactTypeId, objectManager).GetAwaiter().GetResult();

                FieldsMapping.Add(new FieldMap
                {
                    SourceField = sourceField,
                    DestinationField = destinationField,
                    FieldMapType = FieldMapType.Identifier
                });

                return this;
            }
        }

        public IFieldsMappingBuilder WithField(int sourceFieldId, int destinationFieldId)
        {
            using (var fieldManager = _serviceFactoryForAdmin.CreateProxyAsync<IFieldManager>().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                FieldEntry sourceField = ReadFieldEntryByIdAsync(_sourceWorkspaceId, sourceFieldId, fieldManager).GetAwaiter().GetResult();
                FieldEntry destinationField = ReadFieldEntryByIdAsync(_destinationWorkspaceId, destinationFieldId, fieldManager).GetAwaiter().GetResult();

                FieldsMapping.Add(new FieldMap
                {
                    SourceField = sourceField,
                    DestinationField = destinationField,
                    FieldMapType = FieldMapType.None
                });

                return this;
            }
        }

        public IFieldsMappingBuilder WithField(string sourceFieldName, string destinationFieldName)
        {
            using (var objectManager = _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                FieldEntry sourceField = ReadFieldEntryByNameAsync(_sourceWorkspaceId, _rdoArtifactTypeId, sourceFieldName, objectManager).GetAwaiter().GetResult();
                FieldEntry destinationField = ReadFieldEntryByNameAsync(_destinationWorkspaceId, _destinationRdoArtifactTypeId, destinationFieldName, objectManager).GetAwaiter().GetResult();

                FieldsMapping.Add(new FieldMap
                {
                    SourceField = sourceField,
                    DestinationField = destinationField,
                    FieldMapType = FieldMapType.None
                });

                return this;
            }
        }

        private async Task<FieldEntry> ReadIdentifierFieldAsync(int workspaceId, int artifactTypeId, IObjectManager objectManager)
        {
            QueryRequest request = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = (int)ArtifactType.Field
                },
                Condition = $"'FieldArtifactTypeID' == {artifactTypeId} AND 'Is Identifier' == true",
                IncludeNameInQueryResult = true
            };

            QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

            if (result.ResultCount == 0)
            {
                throw new InvalidFieldsMappingException("Identifier field not found");
            }

            RelativityObject field = result.Objects.Single();

            return new FieldEntry
            {
                DisplayName = field.Name,
                FieldIdentifier = field.ArtifactID,
                IsIdentifier = true
            };
        }

        private async Task<FieldEntry> ReadFieldEntryByIdAsync(int workspaceId, int fieldId, IFieldManager fieldManager)
        {
            var field = await fieldManager.ReadAsync(workspaceId, fieldId).ConfigureAwait(false);

            if (field == null)
            {
                throw InvalidFieldsMappingException.FieldNotFound(fieldId);
            }

            var fieldEntry = new FieldEntry
            {
                DisplayName = field.Name,
                FieldIdentifier = field.ArtifactID,
                IsIdentifier = field.IsIdentifier
            };

            if (fieldEntry.IsIdentifier)
            {
                throw InvalidFieldsMappingException.FieldIsIdentifier(fieldEntry.FieldIdentifier);
            }

            return fieldEntry;
        }

        private static async Task<FieldEntry> ReadFieldEntryByNameAsync(int workspaceId, int rdoArtifactTypeId, string fieldName, IObjectManager objectManager)
        {
            QueryRequest request = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = (int)ArtifactType.Field
                },
                Condition = $"'FieldArtifactTypeID' == {rdoArtifactTypeId} AND 'Name' == '{fieldName}'",
                Fields = new List<FieldRef>
                {
                    new FieldRef { Name = "Is Identifier" }
                },
                IncludeNameInQueryResult = true
            };

            QueryResult result = await objectManager
                .QueryAsync(workspaceId, request, 0, int.MaxValue).ConfigureAwait(false);

            if (result.ResultCount == 0)
            {
                throw InvalidFieldsMappingException.FieldNotFound(fieldName);
            }
            else if (result.ResultCount > 1)
            {
                throw InvalidFieldsMappingException.AmbiguousMatch(fieldName);
            }

            RelativityObject field = result.Objects.Single();

            var fieldEntry = new FieldEntry
            {
                DisplayName = field.Name,
                FieldIdentifier = field.ArtifactID,
                IsIdentifier = (bool)field.FieldValues.Single().Value
            };

            if (fieldEntry.IsIdentifier)
            {
                throw InvalidFieldsMappingException.FieldIsIdentifier(fieldEntry.FieldIdentifier);
            }

            return fieldEntry;
        }
    }
}
