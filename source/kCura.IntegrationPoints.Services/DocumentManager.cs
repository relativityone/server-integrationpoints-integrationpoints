using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

		public async Task<DocumentVolumeSummaryModel> GetDocumentVolume(DocumentVolumeRequest request)
		{
			return await Task.Run(() => GetDocumentVolumeInternal(request)).ConfigureAwait(false);
		}

		private async Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewInternal(PercentagePushedToReviewRequest request)
		{
			string destinationWorkspacedisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_GUID);

			IObjectQueryManager objectQueryManager = API.Services.Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
			Query totalDocumentsQuery = new Query();
			Query totalDocumentsPushedToReviewQuery = new Query
			{
				Condition = $"'{destinationWorkspacedisplayName}' ISSET",
				Fields = new[] { destinationWorkspacedisplayName }
			};

			int[] permissions = { 1 };
			ObjectQueryResultSet totalDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, totalDocumentsQuery, 1, 1, permissions, string.Empty);
			ObjectQueryResultSet totalDocumentsPushedToReviewResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, totalDocumentsPushedToReviewQuery, 1, 1, permissions, string.Empty);

			int totalDocuments = 0;
			int totalDocumentsPushedToReview = 0;

			if (totalDocumentsResultSet.Success)
			{
				totalDocuments = totalDocumentsResultSet.Data.TotalResultCount;
			}

			if (totalDocumentsPushedToReviewResultSet.Success)
			{
				totalDocumentsPushedToReview = totalDocumentsPushedToReviewResultSet.Data.TotalResultCount;
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
			Query totalUntaggedDocumentsQuery = new Query
			{
				Condition = $"NOT '{promotedisplayName}' ISSET",
				Fields = new[] { promotedisplayName }
			};
			Query totalIncludedDocumentsQuery = new Query
			{
				Condition = $"'{promotedisplayName}' == CHOICE {_INCLUDE_GUID}",
				Fields = new[] { promotedisplayName }
			};
			Query totalExcludedDocumentsQuery = new Query
			{
				Condition = $"'{promotedisplayName}' == CHOICE {_EXCLUDE_GUID}",
				Fields = new[] { promotedisplayName }
			};
			Query totalPushedToReviewDocumentsQuery = new Query
			{
				Condition = $"'{destinationWorkspacedisplayName}' ISSET",
				Fields = new[] { destinationWorkspacedisplayName }
			};

			int[] permissions = { 1 };
			ObjectQueryResultSet totalUntaggedDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, totalUntaggedDocumentsQuery, 1, 1, permissions, string.Empty);
			ObjectQueryResultSet totalIncludedDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, totalIncludedDocumentsQuery, 1, 1, permissions, string.Empty);
			ObjectQueryResultSet totalExcludedDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, totalExcludedDocumentsQuery, 1, 1, permissions, string.Empty);
			ObjectQueryResultSet totalPushedToReviewDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, 10, totalPushedToReviewDocumentsQuery, 1, 1, permissions, string.Empty);

			int totalUntaggedDocuments = 0;
			int totalIncludedDocuments = 0;
			int totalExcludedDocuments = 0;
			int totalPushedToReviewDocuments = 0;

			if (totalUntaggedDocumentsResultSet.Success)
			{
				totalUntaggedDocuments = totalUntaggedDocumentsResultSet.Data.TotalResultCount;
			}

			if (totalIncludedDocumentsResultSet.Success)
			{
				totalIncludedDocuments = totalIncludedDocumentsResultSet.Data.TotalResultCount;
			}

			if (totalExcludedDocumentsResultSet.Success)
			{
				totalExcludedDocuments = totalExcludedDocumentsResultSet.Data.TotalResultCount;
			}

			if (totalPushedToReviewDocumentsResultSet.Success)
			{
				totalPushedToReviewDocuments = totalPushedToReviewDocumentsResultSet.Data.TotalResultCount;
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

		private async Task<DocumentVolumeSummaryModel> GetDocumentVolumeInternal(DocumentVolumeRequest request)
		{
			DocumentVolumeModel currentDocumentVolume = await GetCurrentDocumentModel(request.WorkspaceArtifactId);
			IEnumerable<DocumentVolumeModel> historicalDocumentVolume = await GetHistoricalDocumentModel(request.WorkspaceArtifactId);

			DocumentVolumeModel[] documentVolume = historicalDocumentVolume.Concat(new[] {currentDocumentVolume}).ToArray();

			DocumentVolumeSummaryModel model = new DocumentVolumeSummaryModel
			{
				DocumentVolume = documentVolume
			};
			return model;
		}

		private async Task<DocumentVolumeModel> GetCurrentDocumentModel(int workspaceId)
		{
			DateTime now = DateTime.UtcNow;

			CurrentSnapshotRequest request = new CurrentSnapshotRequest() {WorkspaceArtifactId = workspaceId};
			CurrentSnapshotModel currentSnapshot = await GetCurrentShapshotInternal(request);

			DocumentVolumeModel model = new DocumentVolumeModel()
			{
				Date = DateTime.UtcNow,
				TotalDocumentsIncluded = currentSnapshot.TotalDocumentsIncluded,
				TotalDocumentsExcluded = currentSnapshot.TotalDocumentsExcluded,
				TotalDocumentsUntagged = currentSnapshot.TotalDocumentsUntagged
			};
			return model;
		}

		private async Task<List<DocumentVolumeModel>> GetHistoricalDocumentModel(int workspaceId)
		{
			IDBContext workspaceContext = API.Services.Helper.GetDBContext(workspaceId);

			List<DocumentVolumeModel> historicalModels = new List<DocumentVolumeModel>();
			using (SqlDataReader reader = workspaceContext.ExecuteSQLStatementAsReader(_DOCUMENT_VOLUME_SQL))
			{
				while (await reader.ReadAsync())
				{
					DocumentVolumeModel historicalModel = new DocumentVolumeModel
					{
						Date = reader.GetDateTime(0),
						TotalDocumentsIncluded = reader.GetInt32(1),
						TotalDocumentsExcluded = reader.GetInt32(2),
						TotalDocumentsUntagged = reader.GetInt32(3)
					};
					historicalModels.Add(historicalModel);
				}
			}
			return historicalModels;
		}

		public void Dispose() { }

		private string GetDisplayName(int workspaceArtifactId, string artifactGuid)
		{
			SqlParameter artifatGuidParameter = new SqlParameter("@artifactGuid", artifactGuid);
			SqlParameter[] sqlParameters = { artifatGuidParameter };

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

		private const string _DOCUMENT_VOLUME_SQL = @"
			SELECT * FROM [DocumentVolume]";

		#endregion
	}
}