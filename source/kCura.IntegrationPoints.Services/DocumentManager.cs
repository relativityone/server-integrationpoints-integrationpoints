using System.Data.SqlClient;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Interfaces.Private.Requests;
using Relativity.API;
using Relativity.Services.ObjectQuery;
using API = Relativity.API;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	/// Get information about the documents in ECA case such as pushed to
	/// review, included, excluded, untagged, etc.
	/// </summary>
	public class DocumentManager : IDocumentManager
	{
		private const string _DESTINATION_WORKSPACE_GUID = "8980C2FA-0D33-4686-9A97-EA9D6F0B4196";

		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}

		public async Task<PercentagePushedToReviewModel> GetPercentagePushedToReview(PercentagePushedToReviewRequest request)
		{
			return await Task.Run(() => GetPercentagePushedToReviewInternal(request)).ConfigureAwait(false);
		}

		private async Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewInternal(PercentagePushedToReviewRequest request)
		{
			string displayName = GetDestinationWorkspaceDisplayName(request.WorkspaceArtifactId);

			IObjectQueryManager objectQueryManager = API.Services.Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
			Query query1 = new Query();
			Query query2 = new Query
			{
				Condition = $"'{displayName}' ISSET",
				Fields = new string[] { displayName }
			};

			int[] permissions = { 1 };
			ObjectQueryResultSet resultSet1 = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, query1, 1, 1, permissions, string.Empty);
			ObjectQueryResultSet resultSet2 = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, query2, 1, 1, permissions, string.Empty);

			int totalDocuments = 0;
			int totalDocumentsPushedToReview = 0;

			if (resultSet1.Success)
			{
				totalDocuments = resultSet1.Data.TotalResultCount;
			}

			if (resultSet2.Success)
			{
				totalDocumentsPushedToReview = resultSet2.Data.TotalResultCount;
			}

			PercentagePushedToReviewModel model = new PercentagePushedToReviewModel
			{
				TotalDocuments = totalDocuments,
				TotalDocumentsPushedToReview = totalDocumentsPushedToReview

			};
			return model;
		}

		public void Dispose() { }

		private string GetDestinationWorkspaceDisplayName(int workspaceArtifactId)
		{
			var workspaceArtifactIdParameter = new SqlParameter("@destinationWorkspaceGuid", _DESTINATION_WORKSPACE_GUID);
			var sqlParameters = new[] { workspaceArtifactIdParameter };

			IDBContext workspaceContext = API.Services.Helper.GetDBContext(workspaceArtifactId);
			string displayName = workspaceContext.ExecuteSqlStatementAsScalar<string>(_DESTINATION_WORKSPACE_DISPLAY_NAME_SQL, sqlParameters);
			return displayName;
		}

		#region SQL Queries

		private const string _DESTINATION_WORKSPACE_DISPLAY_NAME_SQL = @"
			SELECT F.DisplayName FROM [ArtifactGuid] AS AG
			INNER JOIN Artifact AS A
			ON AG.ArtifactID = A.ArtifactID
			INNER JOIN [EDDS1118254].[EDDSDBO].Field AS F
			ON F.ArtifactID = A.ArtifactID
			WHERE ArtifactGuid = @destinationWorkspaceGuid";

		#endregion
	}
}