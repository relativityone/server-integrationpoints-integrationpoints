using System;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiObjectTypeRepository : IObjectTypeRepository
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly int _workspaceArtifactId;

		public RsapiObjectTypeRepository(int workspaceArtifactId, IServicesMgr servicesMgr)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_servicesMgr = servicesMgr;
		}

		public int RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid)
		{
			var objectType = new ObjectType(objectTypeGuid) {Fields = FieldValue.AllFields};
			int descriptorArtifactTypeId = RetrieveObjectTypeDescriptorArtifactTypeId(objectType);
			return descriptorArtifactTypeId;
		}

		public int RetrieveObjectTypeDescriptorArtifactTypeId(int objectTypeArtifactId)
		{
			var objectType = new ObjectType(objectTypeArtifactId) {Fields = FieldValue.AllFields};
			int descriptorArtifactTypeId = RetrieveObjectTypeDescriptorArtifactTypeId(objectType);
			return descriptorArtifactTypeId;
		}

		private int RetrieveObjectTypeDescriptorArtifactTypeId(ObjectType objectType)
		{
			ResultSet<ObjectType> resultSet;
			using (IRSAPIClient rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				resultSet = rsapiClient.Repositories.ObjectType.Read(objectType);
			}

			int? descriptorArtifactTypeId = null;
			if (resultSet.Success && resultSet.Results.Any())
			{
				descriptorArtifactTypeId = resultSet.Results.First().Artifact.DescriptorArtifactTypeID;
			}

			if (!descriptorArtifactTypeId.HasValue)
			{
				throw new TypeLoadException(string.Format(ObjectTypeErrors.OBJECT_TYPE_NO_ARTIFACT_TYPE_FOUND, objectType.Guids[0]));
			}

			return descriptorArtifactTypeId.Value;
		}

		public int? RetrieveObjectTypeArtifactId(string objectTypeName)
		{
			Query<ObjectType> objectTypeQuery = new Query<ObjectType>();

			TextCondition objectTypeNameCondition = new TextCondition("Name", TextConditionEnum.EqualTo, objectTypeName);

			objectTypeQuery.Fields.Add(new FieldValue("Artifact ID"));
			objectTypeQuery.Condition = objectTypeNameCondition;

			QueryResultSet<ObjectType> queryResults;

			using (IRSAPIClient rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				queryResults = rsapiClient.Repositories.ObjectType.Query(objectTypeQuery);
			}

			int? artifactId = 0;
			if (queryResults != null && queryResults.Success && queryResults.Results.Any())
			{
				Result<ObjectType> result = queryResults.Results.First();
				artifactId = result.Artifact.ArtifactID;
			}

			return artifactId > 0 ? artifactId : null;
		}

		public void Delete(int artifactId)
		{
			var objectType = new ObjectType(artifactId);

			using (IRSAPIClient rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				rsapiClient.Repositories.ObjectType.Delete(objectType);
			}
		}
	}
}