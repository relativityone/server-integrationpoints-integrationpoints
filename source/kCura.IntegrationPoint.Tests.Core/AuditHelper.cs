using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoint.Tests.Core.Models;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class AuditHelper
	{
		private readonly IHelper _helper;

		public AuditHelper(IHelper helper)
		{
			_helper = helper;
		}

		public Audit RetrieveLastAuditForArtifact(int workspaceArtifactId, string artifactTypeName, string artifactName)
		{
			string query = $@"
				SELECT TOP 1 
					AuditRecord.ArtifactID, 
					AuditRecord.Details, 
					AuditObject.Textidentifier as [Name],
					AuditObjectType.ArtifactType,
					AuditUser.UserID,
					AuditUser.FullName as [UserName],
					AuditAction.[Action]
				FROM eddsdbo.AuditRecord WITH (NOLOCK)
					INNER JOIN eddsdbo.AuditObject WITH(NOLOCK) ON AuditObject.ArtifactID = AuditRecord.ArtifactID
					INNER JOIN eddsdbo.AuditObjectType WITH(NOLOCK) ON AuditObjectType.ArtifactTypeID = AuditObject.ArtifactTypeID
					INNER JOIN eddsdbo.AuditUser WITH(NOLOCK) ON AuditUser.UserID = AuditRecord.UserID
					INNER JOIN eddsdbo.AuditAction WITH(NOLOCK) on AuditAction.AuditActionID = AuditRecord.[Action]
				WHERE
					AuditObjectType.ArtifactType = @ArtifactTypeName
					AND AuditObject.Textidentifier = @ArtifactName
				ORDER BY AuditRecord.[TimeStamp] DESC";

			var artifactTypeNameParameter = new SqlParameter("@ArtifactTypeName", SqlDbType.NVarChar) { Value = artifactTypeName };
			var artifactNameParameter = new SqlParameter("@ArtifactName", SqlDbType.NVarChar) { Value = artifactName };

			DataTable result = _helper
				.GetDBContext(workspaceArtifactId)
				.ExecuteSqlStatementAsDataTable(
					query, 
					new[] { artifactTypeNameParameter, artifactNameParameter });

			if (result.Rows.Count > 0)
			{
				DataRow row = result.Rows[0];
				var audit = new Audit()
				{
					ArtifactId = (int) row["ArtifactID"],
					ArtifactName = (string) row["Name"],
					UserId = (int) row["UserID"],
					UserFullName = (string) row["UserName"],
					AuditAction = (string) row["Action"],
					AuditDetails = (string) row["Details"]
				};

				return audit;
			}

			return null;
		}
	}
}