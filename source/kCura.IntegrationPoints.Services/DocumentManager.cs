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
		private const string _PROMOTE_GUID = "A7F4F47E-EA97-4CC6-9B6D-CBEE6F81C26C"; // TODO: Add this field to tt-img-app-9-3 and replace guid here
		private const string _INCLUDE_GUID = "523284A7-121A-481B-8C03-EE0C36A46EF4"; // TODO: Add this choice to tt-img-app-9-3 and replace guid here
		private const string _EXCLUDE_GUID = "227DB189-9A4F-427E-A72E-5ED069BE84C4"; // TODO: Add this choice to tt-img-app-9-3 and replace guid here

		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}

		public async Task<PercentagePushedToReviewModel> GetPercentagePushedToReview(PercentagePushedToReviewRequest request)
		{
			return await Task.Run(() => GetPercentagePushedToReviewInternal(request)).ConfigureAwait(false);
		}

		public async Task<CurrentSnapshotModel> GetCurrentSnapshot(CurrentSnapshotRequest request)
		{
			return await Task.Run(() => GetCurrentShapshotInternal(request)).ConfigureAwait(false);
		}

		private async Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewInternal(PercentagePushedToReviewRequest request)
		{
			string destinationWorkspacedisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_GUID);

			IObjectQueryManager objectQueryManager = API.Services.Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
			Query query1 = new Query();
			Query query2 = new Query
			{
				Condition = $"'{destinationWorkspacedisplayName}' ISSET",
				Fields = new string[] { destinationWorkspacedisplayName }
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

		private async Task<CurrentSnapshotModel> GetCurrentShapshotInternal(CurrentSnapshotRequest request)
		{
			string promotedisplayName = GetDisplayName(request.WorkspaceArtifactId, _PROMOTE_GUID);
			string destinationWorkspacedisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_GUID);

			IObjectQueryManager objectQueryManager = API.Services.Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
			Query query1 = new Query
			{
				Condition = $"NOT '{promotedisplayName}' ISSET",
				Fields = new[] { promotedisplayName }
			};
			Query query2 = new Query
			{
				Condition = $"'{promotedisplayName}' == CHOICE {_INCLUDE_GUID}",
				Fields = new[] { promotedisplayName }
			};
			Query query3 = new Query
			{
				Condition = $"'{promotedisplayName}' == CHOICE {_EXCLUDE_GUID}",
				Fields = new[] { promotedisplayName }
			};
			Query query4 = new Query
			{
				Condition = $"'{destinationWorkspacedisplayName}' ISSET",
				Fields = new string[] { destinationWorkspacedisplayName }
			};

			int[] permissions = { 1 };
			ObjectQueryResultSet resultSet1 = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, query1, 1, 1, permissions, string.Empty);
			ObjectQueryResultSet resultSet2 = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, query2, 1, 1, permissions, string.Empty);
			ObjectQueryResultSet resultSet3 = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, query3, 1, 1, permissions, string.Empty);
			ObjectQueryResultSet resultSet4 = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, query4, 1, 1, permissions, string.Empty);

			int totalUntaggedDocuments = 0;
			int totalIncludedDocuments = 0;
			int totalExcludedDocuments = 0;
			int totalPushedToReviewDocuments = 0;

			if (resultSet1.Success)
			{
				totalUntaggedDocuments = resultSet1.Data.TotalResultCount;
			}

			if (resultSet2.Success)
			{
				totalIncludedDocuments = resultSet2.Data.TotalResultCount;
			}

			if (resultSet3.Success)
			{
				totalExcludedDocuments = resultSet3.Data.TotalResultCount;
			}

			if (resultSet4.Success)
			{
				totalPushedToReviewDocuments = resultSet4.Data.TotalResultCount;
			}

			CurrentSnapshotModel model = new CurrentSnapshotModel
			{
				TotalDocumentsUntagged = totalUntaggedDocuments,
				TotalDocumentsIncluded = totalIncludedDocuments,
				TotalDocumentsExcluded = totalExcludedDocuments,
				TotalDocumentsPushedToReview = totalPushedToReviewDocuments

			};
			return model;
		}

		public void Dispose() { }

		private string GetDisplayName(int workspaceArtifactId, string artifactGuid)
		{
			var workspaceArtifactIdParameter = new SqlParameter("@artifactGuid", artifactGuid);
			var sqlParameters = new[] { workspaceArtifactIdParameter };

			IDBContext workspaceContext = API.Services.Helper.GetDBContext(workspaceArtifactId);
			string displayName = workspaceContext.ExecuteSqlStatementAsScalar<string>(_DISPLAY_NAME_SQL, sqlParameters);
			return displayName;
		}

		#region SQL Queries

		private const string _DISPLAY_NAME_SQL = @"
			SELECT F.DisplayName FROM [ArtifactGuid] AS AG
			INNER JOIN Artifact AS A
			ON AG.ArtifactID = A.ArtifactID
			INNER JOIN Field AS F
			ON F.ArtifactID = A.ArtifactID
			WHERE ArtifactGuid = @artifactGuid";

		#endregion
	}
}