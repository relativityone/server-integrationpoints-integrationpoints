using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.Tab;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

// ReSharper disable once CheckNamespace
namespace Relativity.Sync.RDOs.Framework
{
    internal partial class RdoManager
    {
        public async Task EnsureTypeExistsAsync<TRdo>(int workspaceId) where TRdo : IRdoType
        {
            RdoTypeInfo typeInfo = _rdoGuidProvider.GetValue<TRdo>();
            (int rdoArtifactId, List<RelativityObject> existingFields) =
                await GetTypeIdAsync(typeInfo.Name, workspaceId).ConfigureAwait(false)
                ?? await CreateTypeAsync(typeInfo, workspaceId).ConfigureAwait(false);

            ValidateDuplicatedFields(existingFields);
            await CreateMissingFieldsAsync(typeInfo, workspaceId, existingFields, rdoArtifactId).ConfigureAwait(false);
        }

        private async Task<(int artifactId, List<RelativityObject>)?> GetTypeIdAsync(string typeName, int workspaceId)
        {
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                _logger.LogInformation("Querying RDO type ({{name}}) existence in workspace {workspaceId}", typeName, workspaceId);
                QueryResult queryResult = await objectManager
                    .QueryAsync(workspaceId, GetTypeQueryRequest(typeName), 0, 1).ConfigureAwait(false);

                if (queryResult.Objects.Any())
                {
                    int artifactId = (int)queryResult.Objects.First().FieldValues[0].Value;
                    int artifactTypeId = (int)queryResult.Objects.First().FieldValues[1].Value;

                    List<RelativityObject> existingFields = await GetExistingFieldsAsync(artifactTypeId, workspaceId).ConfigureAwait(false);

                    _logger.LogInformation("RDO type ({{name}}) exists in workspace {workspaceId} with fields: [{fieldsGuids}]", typeName, workspaceId, string.Join(", ", existingFields));

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
                    new FieldRef { Name = "Artifact ID" }, new FieldRef { Name = "Artifact Type ID" }
                },
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.ObjectType }
            };
        }

        private async Task<List<RelativityObject>> GetExistingFieldsAsync(int artifactTypeId, int workspaceId)
        {
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var queryResult = await objectManager.QueryAsync(
                        workspaceId,
                        new QueryRequest
                {
                    Condition = $"'FieldArtifactTypeID' == {artifactTypeId}",
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int)ArtifactType.Field
                    },
                    Fields = new List<FieldRef>
                    {
                        new FieldRef
                        {
                            Name = "Name"
                        }
                    }
                },
                        0,
                        int.MaxValue)
                    .ConfigureAwait(false);

                return queryResult.Objects;
            }
        }

        private async Task<(int artifactId, List<RelativityObject>)> CreateTypeAsync(RdoTypeInfo typeInfo, int workspaceId)
        {
            _logger.LogInformation("Creating type ({name}:{guid}) in workspace {workspaceId}", typeInfo.Name, typeInfo.TypeGuid, workspaceId);
            using (IObjectTypeManager objectTypeManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectTypeManager>().ConfigureAwait(false))
            using (IArtifactGuidManager guidManager = await _serviceFactoryForAdmin.CreateProxyAsync<IArtifactGuidManager>().ConfigureAwait(false))
            {
                ObjectTypeRequest objectTypeRequest = GetObjectTypeDefinition(typeInfo);

                int objectTypeArtifactId = await objectTypeManager.CreateAsync(workspaceId, objectTypeRequest).ConfigureAwait(false);
                await guidManager.CreateSingleAsync(workspaceId, objectTypeArtifactId, new List<Guid>() { typeInfo.TypeGuid }).ConfigureAwait(false);
                await DeleteTabAsync(workspaceId, typeInfo.Name).ConfigureAwait(false);

                _logger.LogInformation("Created type ({name}:{guid}) in workspace {workspaceId}", typeInfo.Name, typeInfo.TypeGuid, workspaceId);
                return (objectTypeArtifactId, new List<RelativityObject>());
            }
        }

        private async Task DeleteTabAsync(int workspaceId, string objectTypeName)
        {
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            using (ITabManager tabManager = await _serviceFactoryForAdmin.CreateProxyAsync<ITabManager>().ConfigureAwait(false))
            {
                QueryResult queryResult = await objectManager.QueryAsync(
                    workspaceId,
                    new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int)ArtifactType.Tab
                    },
                    Condition = $"'Object Type' == '{objectTypeName}'"
                },
                    0,
                    1).ConfigureAwait(false);

                if (queryResult.Objects == null || queryResult.Objects.Count == 0)
                {
                    return;
                }

                int tabArtifactId = queryResult.Objects.First().ArtifactID;
                _logger.LogInformation("Deleting tab Artifact ID: {tabArtifactId} for Object Type: '{objectTypeName}'", tabArtifactId, objectTypeName);
                await tabManager.DeleteAsync(workspaceId, tabArtifactId).ConfigureAwait(false);
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
                UseRelativityForms = null,
            };

            if (typeInfo.ParentTypeGuid != null)
            {
                objectTypeRequest.ParentObjectType.Value.Guids.Add(typeInfo.ParentTypeGuid.Value);
            }
            else
            {
                objectTypeRequest.ParentObjectType.Value.ArtifactTypeID = (int)ArtifactType.Case;
            }

            return objectTypeRequest;
        }

        private void ValidateDuplicatedFields(List<RelativityObject> existingFields)
        {
            List<RelativityObject> duplicatedFields = existingFields
                .GroupBy(x => x.FieldValues.First().Value.ToString())
                .Where(y => y.Count() > 1)
                .SelectMany(x => x)
                .ToList();

            if (duplicatedFields.Any())
            {
                string duplicatedFieldsArtifactIds = string.Join(", ", duplicatedFields.Select(x => x.ArtifactID));
                _logger.LogError("Duplicated Field(s) found. ArtifactIds: {duplicatedFieldsArtifactIds}.",
                    duplicatedFieldsArtifactIds);
                throw new DuplicateNameException($"Duplicated Field(s) found. ArtifactIds: {duplicatedFieldsArtifactIds}.");
            }
        }

        private async Task CreateMissingFieldsAsync(
            RdoTypeInfo typeInfo,
            int workspaceId,
            List<RelativityObject> existingFields,
            int artifactId)
        {
            using (IArtifactGuidManager guidManager = await _serviceFactoryForAdmin.CreateProxyAsync<IArtifactGuidManager>().ConfigureAwait(false))
            using (IFieldManager fieldManager = await _serviceFactoryForAdmin.CreateProxyAsync<IFieldManager>().ConfigureAwait(false))
            {
                foreach (RdoFieldInfo fieldInfo in typeInfo.Fields.Values.Where(f => !existingFields.SelectMany(x => x.Guids).Contains(f.Guid)))
                {
                    List<string> existingFieldsNames = existingFields.Select(x => x.FieldValues.First().Value.ToString()).ToList();
                    if (existingFieldsNames.Contains(fieldInfo.Name))
                    {
                        Guid fieldGuid = Guid.NewGuid();
                        _logger.LogInformation(
                            "Updating Guid ({{guid}}) for field with name: {name} for type [{typeName}:{typeGuid}] in workspace {workspaceId}",
                            fieldGuid,
                            fieldInfo.Name,
                            typeInfo.Name,
                            typeInfo.TypeGuid,
                            workspaceId);

                        using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                        {
                            QueryRequest request = new QueryRequest
                            {
                                ObjectType = new ObjectTypeRef()
                                {
                                    ArtifactTypeID = (int)ArtifactType.Field
                                },
                                Fields = new List<FieldRef>
                                {
                                    new FieldRef
                                    {
                                        Name = fieldInfo.Name
                                    }
                                }
                            };

                            QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

                            if (result.Objects.Count <= 0)
                            {
                                _logger.LogError("Unable to retrieve field with name {fieldName} with Object Manager.", fieldInfo.Name);
                                throw new NotFoundException($"Object Manager is unable to retrieve field with name {fieldInfo.Name}.");
                            }

                            // await UpdateFieldInTypeAsync(fieldInfo, result.Objects.First().ArtifactID, artifactId, workspaceId, fieldManager).ConfigureAwait(false);
                            await guidManager
                                .CreateSingleAsync(workspaceId, result.Objects.First().ArtifactID, new List<Guid> { fieldGuid })
                                .ConfigureAwait(false);

                            _logger.LogInformation(
                                "Updated Guid ({guid}) for field with name: {name} for type [{typeName}:{typeGuid}] in workspace {workspaceId}",
                                fieldGuid,
                                fieldInfo.Name,
                                typeInfo.Name,
                                typeInfo.TypeGuid,
                                workspaceId);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Creating field [{name}:{guid}] for type [{typeName}:{typeGuid}] in workspace {workspaceId}", fieldInfo.Name, fieldInfo.Guid, typeInfo.Name, typeInfo.TypeGuid, workspaceId);

                        int fieldId = await CreateFieldInTypeAsync(fieldInfo, artifactId, workspaceId, fieldManager).ConfigureAwait(false);

                        await guidManager
                            .CreateSingleAsync(workspaceId, fieldId, new List<Guid>() { fieldInfo.Guid })
                            .ConfigureAwait(false);

                        _logger.LogInformation("Created field [{name}:{guid}] for type [{typeName}:{typeGuid}] in workspace {workspaceId}", fieldInfo.Name, fieldInfo.Guid, typeInfo.Name, typeInfo.TypeGuid, workspaceId);
                    }
                }
            }
        }

        private async Task UpdateFieldInTypeAsync(RdoFieldInfo fieldInfo, int artifactId, int objectTypeId, int workspaceId, IFieldManager fieldManager)
        {
            switch (fieldInfo.Type)
            {
                case RdoFieldType.LongText:
                    await fieldManager.UpdateLongTextFieldAsync(
                        workspaceId,
                        artifactId,
                        new LongTextFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            HasUnicode = true,
                            IsRequired = fieldInfo.IsRequired,
                        })
                        .ConfigureAwait(false);
                    return;

                case RdoFieldType.FixedLengthText:
                    await fieldManager.UpdateFixedLengthFieldAsync(
                        workspaceId,
                        artifactId,
                        new FixedLengthFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            HasUnicode = true,
                            Length = fieldInfo.TextLength,
                            IsRequired = fieldInfo.IsRequired
                        });
                    return;

                case RdoFieldType.WholeNumber:
                    await fieldManager.UpdateWholeNumberFieldAsync(
                        workspaceId,
                        artifactId,
                        new WholeNumberFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            IsRequired = fieldInfo.IsRequired
                        });
                    return;

                case RdoFieldType.Decimal:
                    await fieldManager.UpdateDecimalFieldAsync(
                        workspaceId,
                        artifactId,
                        new DecimalFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            IsRequired = fieldInfo.IsRequired
                        });
                    return;

                case RdoFieldType.YesNo:
                    await fieldManager.UpdateYesNoFieldAsync(
                        workspaceId,
                        artifactId,
                        new YesNoFieldRequest
                        {
                            FilterType = FilterType.List,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            IsRequired = fieldInfo.IsRequired
                        });
                    return;

                default:
                    throw new NotSupportedException($"Sync doesn't support updating of field type: {fieldInfo.Type}");
            }
        }

        private Task<int> CreateFieldInTypeAsync(RdoFieldInfo fieldInfo, int objectTypeId, int workspaceId, IFieldManager fieldManager)
        {
            switch (fieldInfo.Type)
            {
                case RdoFieldType.LongText:
                    return fieldManager.CreateLongTextFieldAsync(
                        workspaceId,
                        new LongTextFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            HasUnicode = true,
                            IsRequired = fieldInfo.IsRequired
                        });

                case RdoFieldType.FixedLengthText:
                    return fieldManager.CreateFixedLengthFieldAsync(
                        workspaceId,
                        new FixedLengthFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            HasUnicode = true,
                            Length = fieldInfo.TextLength,
                            IsRequired = fieldInfo.IsRequired
                        });

                case RdoFieldType.WholeNumber:
                    return fieldManager.CreateWholeNumberFieldAsync(
                        workspaceId,
                        new WholeNumberFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            IsRequired = fieldInfo.IsRequired
                        });

                case RdoFieldType.Decimal:
                    return fieldManager.CreateDecimalFieldAsync(
                        workspaceId,
                        new DecimalFieldRequest
                        {
                            FilterType = FilterType.TextBox,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            IsRequired = fieldInfo.IsRequired
                        });

                case RdoFieldType.YesNo:
                    return fieldManager.CreateYesNoFieldAsync(
                        workspaceId,
                        new YesNoFieldRequest
                        {
                            FilterType = FilterType.List,
                            Name = fieldInfo.Name,
                            ObjectType = new ObjectTypeIdentifier() { ArtifactID = objectTypeId },
                            IsRequired = fieldInfo.IsRequired
                        });

                default:
                    throw new NotSupportedException($"Sync doesn't support creation of field type: {fieldInfo.Type}");
            }
        }
    }
}
