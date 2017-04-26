using System;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiObjectTypeRepository : IObjectTypeRepository
	{
		private readonly IAPILog _logger;
		private readonly IServicesMgr _servicesMgr;
		private readonly int _workspaceArtifactId;

		public RsapiObjectTypeRepository(int workspaceArtifactId, IServicesMgr servicesMgr, IHelper helper)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_servicesMgr = servicesMgr;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RsapiObjectTypeRepository>();
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

			using (IRSAPIClient rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
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
				using (var rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
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
			Query<ObjectType> objectTypeQuery = new Query<ObjectType>
			{
				Condition = new TextCondition(ObjectTypeFieldNames.Name, TextConditionEnum.EqualTo, objectTypeName)
			};
			objectTypeQuery.Fields.Add(new FieldValue(ArtifactQueryFieldNames.ArtifactID));

			QueryResultSet<ObjectType> queryResults;

			using (IRSAPIClient rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				queryResults = rsapiClient.Repositories.ObjectType.Query(objectTypeQuery);
			}

			if (queryResults.Success && queryResults.Results.Any())
			{
				Result<ObjectType> result = queryResults.Results.First();
				return result.Artifact.ArtifactID;
			}

			return null;
		}
	}
}