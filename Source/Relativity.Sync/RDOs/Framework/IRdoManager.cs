using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.RDOs.Framework
{
    internal interface IRdoManager
    {
        Task EnsureTypeExistsAsync<TRdo>(int workspaceId) where TRdo : IRdoType;

        Task<TRdo> GetAsync<TRdo>(int workspaceId, int artifactId, params Expression<Func<TRdo, object>>[] fields)
            where TRdo : IRdoType, new();

        Task CreateAsync<TRdo>(int workspaceId, TRdo rdo, int? parentObjectId = null) where TRdo : IRdoType;

        Task SetValuesAsync<TRdo>(int workspaceId, TRdo rdo, params Expression<Func<TRdo, object>>[] fields)
            where TRdo : IRdoType;
    }

    internal class RdoManager : IRdoManager
    {
        private readonly ISyncLog _logger;
        private readonly ISyncServiceManager _servicesMgr;
        private readonly IRdoGuidProvider _rdoGuidProvider;

        public RdoManager(ISyncLog logger, ISyncServiceManager servicesMgr, IRdoGuidProvider rdoGuidProvider)
        {
            _logger = logger;
            _servicesMgr = servicesMgr;
            _rdoGuidProvider = rdoGuidProvider;
        }

        public async Task EnsureTypeExistsAsync<TRdo>(int workspaceId) where TRdo : IRdoType
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();
            (int rdoArtifactId, HashSet<Guid> existingFields) =
                await GetTypeIdAsync(typeInfo.Name, workspaceId).ConfigureAwait(false)
                ?? await CreateTypeAsync(typeInfo, workspaceId).ConfigureAwait(false);

            await CreateMissingFieldsAsync(typeInfo, workspaceId, existingFields, rdoArtifactId).ConfigureAwait(false);
        }

        public async Task<TRdo> GetAsync<TRdo>(int workspaceId, int artifactId,
            params Expression<Func<TRdo, object>>[] fields) where TRdo : IRdoType, new()
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();

            var explicitlyRequestedFieldsGuids = GetFieldsGuidsFromExpressions(fields);

            var fieldsToQuery = (explicitlyRequestedFieldsGuids.Any()
                    ? explicitlyRequestedFieldsGuids.Select(g => typeInfo.Fields[g])
                    : typeInfo.Fields.Values)
                .ToArray();

            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = typeInfo.TypeGuid
                },
                Fields = fieldsToQuery.Select(x => new FieldRef
                {
                    Guid = x.Guid
                }),
                Condition = $"'ArtifactId' == {artifactId}"
            };

            using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                QueryResultSlim queryResult =
                    await objectManager.QuerySlimAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

                var resultObject = queryResult.Objects.FirstOrDefault();

                var result = new TRdo();

                if (resultObject != null)
                {
                    result.ArtifactId = resultObject.ArtifactID;

                    foreach (var (fieldInfo, i) in fieldsToQuery.Select((x, i) => (x, i)))
                    {
                        fieldInfo.PropertyInfo.SetValue(result, resultObject.Values[i]);
                    }
                }

                return result;
            }
        }

        private static HashSet<Guid> GetFieldsGuidsFromExpressions<TRdo>(Expression<Func<TRdo, object>>[] fields) where TRdo : IRdoType
        {
            return new HashSet<Guid>(fields.Select(x =>
                    ((x.Body as UnaryExpression)?.Operand as MemberExpression)
                    ?? throw new InvalidExpressionException($"Expression must be a unary member expression: {x}"))
                .Select(x => (x.Member.GetCustomAttribute<RdoFieldAttribute>().FieldGuid)));
        }

        public async Task CreateAsync<TRdo>(int workspaceId, TRdo rdo, int? parentObjectId = null) where TRdo : IRdoType
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();

            CreateRequest request = new CreateRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = typeInfo.TypeGuid
                },
                FieldValues = GetFieldRefValuePairsForSettingValues(rdo, typeInfo.Fields.Values.ToArray())
            };

            if (parentObjectId != null)
            {
                request.ParentObject = new RelativityObjectRef {ArtifactID = parentObjectId.Value};
            }

            using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);

                rdo.ArtifactId = result.Object.ArtifactID;
            }
        }

        public async Task SetValuesAsync<TRdo>(int workspaceId, TRdo rdo, params Expression<Func<TRdo, object>>[] fields)
            where TRdo : IRdoType
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();

            var explicitlyRequestedFieldsGuids = GetFieldsGuidsFromExpressions(fields);

            var fieldsToUpdate = (explicitlyRequestedFieldsGuids.Any()
                    ? explicitlyRequestedFieldsGuids.Select(g => typeInfo.Fields[g])
                    : typeInfo.Fields.Values)
                .ToArray();
            
            UpdateRequest request = new UpdateRequest()
            {
                Object = new RelativityObjectRef()
                {
                    ArtifactID = rdo.ArtifactId
                },
                FieldValues = GetFieldRefValuePairsForSettingValues(rdo, fieldsToUpdate)
            };

            using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                await objectManager.UpdateAsync(workspaceId, request).ConfigureAwait(false);
            }
        }


        private IEnumerable<FieldRefValuePair> GetFieldRefValuePairsForSettingValues<TRdo>(TRdo rdo, RdoFieldInfo[] fields)
            where TRdo : IRdoType
        {
           return fields.Select(fieldInfo => new FieldRefValuePair
            {
                Field = new FieldRef
                {
                    Guid = fieldInfo.Guid
                },
                Value = fieldInfo.PropertyInfo.GetValue(rdo)
            });
        }

        private async Task CreateMissingFieldsAsync(RdoTypeInfo typeInfo, int workspaceId, HashSet<Guid> existingFields,
            int artifactId)
        {
            using (IArtifactGuidManager guidManager =
                _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                using (IFieldManager fieldManager = _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
                {
                    foreach (RdoFieldInfo fieldInfo in typeInfo.Fields.Values.Where(f =>
                        !existingFields.Contains(f.Guid)))
                    {
                        int fieldId = await CreateFieldInTypeAsync(fieldInfo, artifactId, workspaceId, fieldManager)
                            .ConfigureAwait(false);

                        await guidManager
                            .CreateSingleAsync(workspaceId, fieldId, new List<Guid>() {fieldInfo.Guid})
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        private Task<int> CreateFieldInTypeAsync(RdoFieldInfo fieldInfo, int objectTypeId, int workspaceId,
            IFieldManager fieldManager)
        {
            switch (fieldInfo.Type)
            {
                case RdoFieldType.LongText:
                    return fieldManager.CreateLongTextFieldAsync(workspaceId,
                        new LongTextFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() {ArtifactID = objectTypeId},
                            IsRequired = fieldInfo.IsRequired
                        });
                case RdoFieldType.FixedLengthText:
                    return fieldManager.CreateFixedLengthFieldAsync(workspaceId,
                        new FixedLengthFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() {ArtifactID = objectTypeId},
                            Length = fieldInfo.TextLenght,
                            IsRequired = fieldInfo.IsRequired
                        });
                case RdoFieldType.WholeNumber:
                    return fieldManager.CreateWholeNumberFieldAsync(workspaceId,
                        new WholeNumberFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() {ArtifactID = objectTypeId},
                            IsRequired = fieldInfo.IsRequired
                        });

                case RdoFieldType.YesNo:
                    return fieldManager.CreateYesNoFieldAsync(workspaceId,
                        new YesNoFieldRequest
                        {
                            FilterType = FilterType.List,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() {ArtifactID = objectTypeId},
                            IsRequired = fieldInfo.IsRequired
                        });

                default:
                    throw new NotSupportedException($"Sync doesn't support creation of field type: {fieldInfo.Type}");
            }
        }


        private async Task<(int artifactId, HashSet<Guid>)?> GetTypeIdAsync(string typeName,
            int workspaceId)
        {
            using (IObjectManager objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                var queryResult = await objectManager
                    .QueryAsync(workspaceId, GetTypeQueryRequest(typeName), 0, 1).ConfigureAwait(false);

                if (queryResult.Objects.Any())
                {
                    int artifactId = (int) queryResult.Objects.First().FieldValues[0].Value;
                    int artifactTypeId = (int) queryResult.Objects.First().FieldValues[1].Value;

                    HashSet<Guid> existingFields =
                        await GetExistingFieldsAsync(artifactTypeId, workspaceId).ConfigureAwait(false);

                    return (artifactId, existingFields);
                }

                return null;
            }
        }

        private static QueryRequest GetTypeQueryRequest(string name)
        {
            return new QueryRequest
            {
                Condition = $"'Name' == '{name}'",
                Fields = new[]
                {
                    new FieldRef {Name = "Artifact ID"}, new FieldRef {Name = "Artifact Type ID"}
                },
                ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.ObjectType}
            };
        }

        private async Task<HashSet<Guid>> GetExistingFieldsAsync(int artifactTypeId, int workspaceId)
        {
            using (IObjectManager objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                var queryResult = await objectManager.QueryAsync(workspaceId, new QueryRequest()
                {
                    Condition = $"'FieldArtifactTypeID' == {artifactTypeId}",
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int) ArtifactType.Field
                    }
                }, 0, int.MaxValue).ConfigureAwait(false);

                return new HashSet<Guid>(queryResult.Objects.SelectMany(x => x.Guids));
            }
        }

        private async Task<(int artifactId, HashSet<Guid>)> CreateTypeAsync(RdoTypeInfo typeInfo,
            int workspaceId)
        {
            using (IObjectTypeManager objectTypeManager =
                _servicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
            using (IArtifactGuidManager guidManager =
                _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
            {
                ObjectTypeRequest objectTypeRequest = GetObjectTypeDefinition(typeInfo);

                int objectTypeArtifactId = await objectTypeManager.CreateAsync(workspaceId, objectTypeRequest)
                    .ConfigureAwait(false);

                await guidManager.CreateSingleAsync(workspaceId, objectTypeArtifactId,
                        new List<Guid>() {typeInfo.TypeGuid})
                    .ConfigureAwait(false);

                return (objectTypeArtifactId, new HashSet<Guid>());
            }
        }

        private static ObjectTypeRequest GetObjectTypeDefinition(RdoTypeInfo typeInfo)
        {
            var objectTypeRequest = new ObjectTypeRequest
            {
                CopyInstancesOnCaseCreation = false,
                CopyInstancesOnParentCopy = false,
                EnableSnapshotAuditingOnDelete = true,
                Keywords = null,
                Name = typeInfo.Name,
                Notes = null,
                ParentObjectType = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier()),
                PersistentListsEnabled = false,
                PivotEnabled = false,
                RelativityApplications = null,
                SamplingEnabled = false,
                UseRelativityForms = null
            };

            if (typeInfo.ParentTypeGuid != null)
            {
                objectTypeRequest.ParentObjectType.Value.Guids.Add(typeInfo.ParentTypeGuid.Value);
            }
            else
            {
                objectTypeRequest.ParentObjectType.Value.ArtifactTypeID = (int) ArtifactType.Case;
            }

            return objectTypeRequest;
        }
    }
}