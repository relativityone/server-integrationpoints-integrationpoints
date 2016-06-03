using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiObjectTypeRepository : IObjectTypeRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public RsapiObjectTypeRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public int? RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid)
		{
			var objectType = new ObjectType(objectTypeGuid) {Fields = FieldValue.AllFields};

			ResultSet<ObjectType> resultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				resultSet = rsapiClient.Repositories.ObjectType.Read(new[] {objectType});
			}

			int? objectTypeArtifactId = null;
			if (resultSet.Success && resultSet.Results.Any())
			{
				objectTypeArtifactId = resultSet.Results.First().Artifact.DescriptorArtifactTypeID;
			}

			return objectTypeArtifactId;
		}

		public int? RetrieveObjectTypeArtifactId(string objectTypeName)
		{
			SqlParameter nameParameter = new SqlParameter("@objectTypeName", SqlDbType.NVarChar)
			{
				Value = objectTypeName
			};

			string sql = @"	SELECT [ArtifactID]
							FROM [eddsdbo].[ObjectType]
							WHERE [Name] = @objectTypeName";

			IDBContext workspaceContext = _helper.GetDBContext(_workspaceArtifactId);
			int? artifactId = workspaceContext.ExecuteSqlStatementAsScalar<int>(sql, nameParameter);

			return artifactId > 0 ? artifactId : null;
		}

		public void Delete(int artifactId)
		{
			var objectType = new ObjectType(artifactId);

			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				rsapiClient.Repositories.ObjectType.Delete(new[] {objectType});
			}
		}
	}
}