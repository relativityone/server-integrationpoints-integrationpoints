using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class ObjectTypeRepository : IObjectTypeRepository
    {
        private const string _NAME = "Name";
        private const string _DESCRIPTOR_ARTIFACT_TYPE_ID = "DescriptorArtifactTypeID";
        private const string _PARENT_ARTIFACT_TYPE_ID = "ParentArtifactTypeID";
        private readonly IAPILog _logger;
        private readonly int _workspaceArtifactId;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IServicesMgr _servicesMgr;

        public ObjectTypeRepository(int workspaceArtifactId, IServicesMgr servicesMgr, IHelper helper, IRelativityObjectManager objectManager)
        {
            _workspaceArtifactId = workspaceArtifactId;
            _servicesMgr = servicesMgr;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ObjectTypeRepository>();
            _objectManager = objectManager;
        }

        public int CreateObjectType(Guid objectTypeGuid, string objectTypeName, int parentArtifactTypeId)
        {
            var objectTypeRequest = new ObjectTypeRequest()
            {
                Name = objectTypeName,
                ParentObjectType = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier()
                {
                    ArtifactTypeID = parentArtifactTypeId
                }),
                CopyInstancesOnParentCopy = false,
                CopyInstancesOnCaseCreation = false,
                EnableSnapshotAuditingOnDelete = false,
                PivotEnabled = true,
                SamplingEnabled = false,
                PersistentListsEnabled = false
            };

            try
            {
                using (IObjectTypeManager objectTypeManager = _servicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
                using (IArtifactGuidManager artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
                {
                    int objectTypeArtifactId = objectTypeManager.CreateAsync(_workspaceArtifactId, objectTypeRequest).GetAwaiter().GetResult();
                    artifactGuidManager.CreateSingleAsync(_workspaceArtifactId, objectTypeArtifactId, new List<Guid>()
                    {
                        objectTypeGuid
                    }).GetAwaiter().GetResult();

                    return objectTypeArtifactId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ObjectType {name} with {guid}.", objectTypeName, objectTypeGuid);
                throw;
            }
        }

        public int RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid)
        {
            try
            {
                using (IObjectTypeManager objectTypeManager = _servicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
                using (IArtifactGuidManager artifactGuidManager = _servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
                {
                    int objectTypeArtifactId = artifactGuidManager.ReadSingleArtifactIdAsync(_workspaceArtifactId, objectTypeGuid).GetAwaiter().GetResult();
                    ObjectTypeResponse objectTypeResponse = objectTypeManager.ReadAsync(_workspaceArtifactId, objectTypeArtifactId).GetAwaiter().GetResult();

                    if (objectTypeResponse == null)
                    {
                        throw new NotFoundException($"Cannot find object type GUID: {objectTypeGuid}");
                    }

                    return objectTypeResponse.ArtifactTypeID;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve object type with guid {guid}.", objectTypeGuid);
                throw new TypeLoadException(string.Format(ObjectTypeErrors.OBJECT_TYPE_NO_ARTIFACT_TYPE_FOUND, objectTypeGuid), ex);
            }
        }

        public int? RetrieveObjectTypeArtifactId(string objectTypeName)
        {
            string condition = CreateNameCondition(objectTypeName);
            var queryRequest = new QueryRequest
            {
                ObjectType = GetObjectTypeRef(),
                Fields = new FieldRef[0],
                Condition = condition
            };

            List<RelativityObject> result = _objectManager.Query(queryRequest);
            return result?.FirstOrDefault()?.ArtifactID;
        }

        public ObjectTypeDTO GetObjectType(int typeID)
        {
            return GetAllRdo(new List<int> { typeID }).First();
        }

        public int GetObjectTypeID(string objectTypeName)
        {
            FieldRef[] fields =
            {
                new FieldRef {Name = _DESCRIPTOR_ARTIFACT_TYPE_ID },
                new FieldRef {Name = _NAME}
            };
            string condition = CreateNameCondition(objectTypeName);

            var queryRequest = new QueryRequest
            {
                ObjectType = GetObjectTypeRef(),
                Fields = fields,
                Condition = condition,
            };

            RelativityObject firstResult = _objectManager.Query(queryRequest).FirstOrDefault();
            FieldValuePair descriptorField = firstResult?.FieldValues?.FirstOrDefault(x => x.Field.Name == _DESCRIPTOR_ARTIFACT_TYPE_ID);
            if (descriptorField?.Value == null)
            {
                string exceptionMessage = $"Object type with name {objectTypeName} was not found in workspace {_workspaceArtifactId}";
                throw new IntegrationPointsException(exceptionMessage)
                {
                    ExceptionSource = IntegrationPointsExceptionSource.KEPLER
                };
            }

            try
            {
                return Convert.ToInt32(descriptorField.Value);
            }
            catch (Exception ex)
            {
                string exceptionMessage = $"Error while converting 'Descriptor Artifact Id' to int. Actual value: {descriptorField.Value}";
                throw new IntegrationPointsException(exceptionMessage, ex);
            }
        }

        public Dictionary<Guid, int> GetRdoGuidToArtifactIdMap()
        {
            var results = new Dictionary<Guid, int>();
            List<ObjectTypeDTO> types = GetAllRdo();

            foreach (ObjectTypeDTO type in types)
            {
                if (type.DescriptorArtifactTypeId.HasValue)
                {
                    foreach (Guid guid in type.Guids)
                    {
                        results[guid] = type.DescriptorArtifactTypeId.Value;
                    }
                }
            }
            return results;
        }

        private List<ObjectTypeDTO> GetAllRdo(List<int> typeIds = null)
        {
            FieldRef[] fields =
            {
                new FieldRef
                {
                    Name = _DESCRIPTOR_ARTIFACT_TYPE_ID
                },
                new FieldRef
                {
                    Name = _NAME
                },
                new FieldRef
                {
                    Name = _PARENT_ARTIFACT_TYPE_ID
                }
            };

            var sort = new Sort
            {
                Direction = SortEnum.Ascending,
                FieldIdentifier = new FieldRef { Name = _NAME }
            };

            string condition;
            if (typeIds != null)
            {
                string ids = string.Join(",", typeIds);
                condition = $"'{_DESCRIPTOR_ARTIFACT_TYPE_ID}' IN [{ids}]";
            }
            else
            {
                string condition1 = $"'{_DESCRIPTOR_ARTIFACT_TYPE_ID}' > {Constants.NON_SYSTEM_FIELD_START_ID}";
                string condition2 = $"'{_DESCRIPTOR_ARTIFACT_TYPE_ID}' == {(int)ArtifactType.Document}";
                condition = $"({condition1}) OR ({condition2})";
            }

            var queryRequest = new QueryRequest
            {
                ObjectType = GetObjectTypeRef(),
                Fields = fields,
                Condition = condition,
                Sorts = new[] { sort }
            };

            List<RelativityObject> result = _objectManager.Query(queryRequest);

            return result.Select(ConvertRelativityObjectToObjectType).ToList();
        }

        private static string CreateNameCondition(string objectTypeName)
        {
            return $"'{_NAME}' == '{objectTypeName}'";
        }

        private static ObjectTypeRef GetObjectTypeRef()
        {
            return new ObjectTypeRef
            {
                ArtifactTypeID = (int)ArtifactType.ObjectType
            };
        }

        private ObjectTypeDTO ConvertRelativityObjectToObjectType(RelativityObject relativityObject)
        {
            if (relativityObject == null)
            {
                return null;
            }

            string name = relativityObject.FieldValues?.FirstOrDefault(x => x?.Field?.Name == _NAME)?.Value as string;
            int parentArtifactTypeId = GetIntValueFromField(relativityObject, _PARENT_ARTIFACT_TYPE_ID);
            int descriptorArtifactTypeId = GetIntValueFromField(relativityObject, _DESCRIPTOR_ARTIFACT_TYPE_ID);

            return new ObjectTypeDTO
            {
                ArtifactId = relativityObject.ArtifactID,
                ParentArtifactId = relativityObject.ParentObject.ArtifactID,
                ParentArtifactTypeId = parentArtifactTypeId,
                DescriptorArtifactTypeId = descriptorArtifactTypeId,
                Guids = relativityObject.Guids,
                Name = name
            };
        }

        private int GetIntValueFromField(RelativityObject relativityObject, string fieldName)
        {
            int valueToReturn = 0;

            object valueAsObject = relativityObject.FieldValues?.FirstOrDefault(x => x?.Field?.Name == fieldName)?.Value;
            if (valueAsObject == null)
            {
                _logger.LogWarning("{fieldName} is not present in relativity object fields collection.", fieldName);
                return valueToReturn;
            }
            try
            {
                valueToReturn = Convert.ToInt32(valueAsObject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{fieldName} is not an integer. Actual value: {value}", fieldName, valueAsObject);
            }
            return valueToReturn;
        }
    }
}
