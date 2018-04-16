using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ObjectTypeRepository : IObjectTypeRepository
	{
		private readonly IAPILog _logger;
		private readonly IServicesMgr _servicesMgr;
		private readonly int _workspaceArtifactId;
		private readonly IRsapiClientFactory _rsapiClientFactory;
		private readonly IRelativityObjectManager _objectManager;
		public ObjectTypeRepository(int workspaceArtifactId, IServicesMgr servicesMgr, IHelper helper, IRelativityObjectManager objectManager)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_servicesMgr = servicesMgr;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ObjectTypeRepository>();
			_rsapiClientFactory = new RsapiClientFactory();
			_objectManager = objectManager;
		}

		public int CreateObjectType(Guid objectTypeGuid, string objectTypeName, int parentArtifactTypeId)
		{
			var objectType = new ObjectType(objectTypeGuid)
			{
				Name = objectTypeName,
				ParentArtifactTypeID = parentArtifactTypeId,
				CopyInstancesOnParentCopy = false,
				CopyInstancesOnWorkspaceCreation = false,
				SnapshotAuditingEnabledOnDelete = false,
				Pivot = true,
				Sampling = false,
				PersistentLists = false
			};

			using (new SerilogContextRestorer())
			using (Relativity.Client.IRSAPIClient rsapiClient = _rsapiClientFactory.CreateUserClient(_servicesMgr, _logger))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					return rsapiClient.Repositories.ObjectType.CreateSingle(objectType);
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Failed to create ObjectType {name} with {guid}.", objectTypeName, objectTypeGuid);
					throw;
				}
			}
		}

		public int RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid)
		{
			int? descriptorArtifactTypeId = null;
			try
			{
				using (new SerilogContextRestorer())
				using (var rsapiClient = _rsapiClientFactory.CreateUserClient(_servicesMgr, _logger))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					descriptorArtifactTypeId = rsapiClient.Repositories.ObjectType.ReadSingle(objectTypeGuid).DescriptorArtifactTypeID;
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to retrieve object type with guid {guid}.", objectTypeGuid);
			}
			if (!descriptorArtifactTypeId.HasValue)
			{
				throw new TypeLoadException(string.Format(ObjectTypeErrors.OBJECT_TYPE_NO_ARTIFACT_TYPE_FOUND, objectTypeGuid));
			}
			return descriptorArtifactTypeId.Value;
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
				new FieldRef {Name = ObjectTypeFieldNames.DESCRIPTOR_ARTIFACT_TYPE_ID},
				new FieldRef {Name = ObjectTypeFieldNames.NAME}
			};
			string condition = CreateNameCondition(objectTypeName);

			var queryRequest = new QueryRequest
			{
				ObjectType = GetObjectTypeRef(),
				Fields = fields,
				Condition = condition,
			};

			RelativityObject firstResult = _objectManager.Query(queryRequest).FirstOrDefault();
			FieldValuePair descriptorField = firstResult?.FieldValues?.FirstOrDefault(x => x.Field.Name == ObjectTypeFieldNames.DESCRIPTOR_ARTIFACT_TYPE_ID);
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

		public Dictionary<Guid, int> GetRdoGuidToArtifactIdMap(int userId)
		{
			var results = new Dictionary<Guid, int>();
			List<ObjectTypeDTO> types = GetAllTypes(userId);

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

		public List<ObjectTypeDTO> GetAllTypes(int userId)
		{
			return GetAllRdo();
		}

		private List<ObjectTypeDTO> GetAllRdo(List<int> typeIds = null)
		{
			FieldRef[] fields =
			{
				new FieldRef
				{
					Name = ObjectTypeFieldNames.DESCRIPTOR_ARTIFACT_TYPE_ID
				},
				new FieldRef
				{
					Name = ObjectTypeFieldNames.NAME
				},
				new FieldRef
				{
					Name = ObjectTypeFieldNames.PARENT_ARTIFACT_TYPE_ID
				}
			};

			var sort = new Sort
			{
				Direction = SortEnum.Ascending,
				FieldIdentifier = new FieldRef { Name = ObjectTypeFieldNames.NAME }
			};

			string condition;
			if (typeIds != null)
			{
				string ids = string.Join(",", typeIds);
				condition = $"'{ObjectTypeFieldNames.DESCRIPTOR_ARTIFACT_TYPE_ID}' IN [{ids}]";
			}
			else
			{
				string condition1 = $"'{ObjectTypeFieldNames.DESCRIPTOR_ARTIFACT_TYPE_ID}' > {Constants.NON_SYSTEM_FIELD_START_ID}";
				string condition2 = $"'{ObjectTypeFieldNames.DESCRIPTOR_ARTIFACT_TYPE_ID}' == {(int)Relativity.Client.ArtifactType.Document}";
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
			return $"'{ObjectTypeFieldNames.NAME}' == '{objectTypeName}'";
		}

		private static ObjectTypeRef GetObjectTypeRef()
		{
			return new ObjectTypeRef
			{
				ArtifactTypeID = (int)Relativity.Client.ArtifactType.ObjectType
			};
		}

		private ObjectTypeDTO ConvertRelativityObjectToObjectType(RelativityObject relativityObject)
		{
			if (relativityObject == null)
			{
				return null;
			}

			string name = relativityObject.FieldValues?.FirstOrDefault(x => x?.Field?.Name == ObjectTypeFieldNames.NAME)?.Value as string;
			int parentArtifactTypeId = GetIntValueFromField(relativityObject, ObjectTypeFieldNames.PARENT_ARTIFACT_TYPE_ID);
			int descriptorArtifactTypeId = GetIntValueFromField(relativityObject, ObjectTypeFieldNames.DESCRIPTOR_ARTIFACT_TYPE_ID);

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

		private static class ObjectTypeFieldNames
		{
			public const string NAME = "Name";
			public const string PARENT_ARTIFACT_TYPE_ID = "ParentArtifactTypeID";
			public const string DESCRIPTOR_ARTIFACT_TYPE_ID = "DescriptorArtifactTypeID";
		}
	}
}