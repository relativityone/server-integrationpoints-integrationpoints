using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.ObjectQuery;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public class DocumentRepository : IDocumentRepository
	{
		private const string _DESTINATION_WORKSPACE_FIELD_GUID = "8980C2FA-0D33-4686-9A97-EA9D6F0B4196";
		private const string _PROMOTE_GUID = "4E418A56-90C5-4E59-A1C5-C43C11A3CCFF";
		private const string _INCLUDE_GUID = "6884BAC4-DD8F-4087-9C17-B4BCE99815D5";
		private const string _EXCLUDE_GUID = "DB110A00-AC87-4C40-96E2-827BF9B18909";

		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int) ArtifactType.Document;

		private static readonly int[] _viewPermission = {1};

		public PercentagePushedToReviewModel GetPercentagePushedToReview(PercentagePushedToReviewRequest request)
		{
			string destinationWorkspaceDisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_FIELD_GUID);

			IObjectQueryManager objectQueryManager = global::Relativity.API.Services.Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
			Query totalDocumentsQuery = new Query();
			Query totalDocumentsPushedToReviewQuery = new Query
			{
				Condition = $"'{destinationWorkspaceDisplayName}' ISSET",
				Fields = new[] {destinationWorkspaceDisplayName}
			};

			ObjectQueryResultSet totalDocumentsResultSet =
				objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalDocumentsQuery, 1, 1, _viewPermission, string.Empty).Result;
			ObjectQueryResultSet totalDocumentsPushedToReviewResultSet =
				objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalDocumentsPushedToReviewQuery, 1, 1, _viewPermission, string.Empty).Result;

			if (!totalDocumentsResultSet.Success)
			{
				throw new Exception(totalDocumentsResultSet.Message);
			}
			if (!totalDocumentsPushedToReviewResultSet.Success)
			{
				throw new Exception(totalDocumentsPushedToReviewResultSet.Message);
			}

			PercentagePushedToReviewModel model = new PercentagePushedToReviewModel
			{
				TotalDocuments = totalDocumentsResultSet.Data.TotalResultCount,
				TotalDocumentsPushedToReview = totalDocumentsPushedToReviewResultSet.Data.TotalResultCount
			};
			return model;
		}

		public CurrentPromotionStatusModel GetCurrentPromotionStatus(CurrentPromotionStatusRequest request)
		{
			string promotedDisplayName = GetDisplayName(request.WorkspaceArtifactId, _PROMOTE_GUID);
			string destinationWorkspacedisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_FIELD_GUID);

			IObjectQueryManager objectQueryManager = global::Relativity.API.Services.Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
			Query totalUntaggedDocumentsQuery = new Query
			{
				Condition = $"NOT '{promotedDisplayName}' ISSET",
				Fields = new[] {promotedDisplayName}
			};
			Query totalIncludedDocumentsQuery = new Query
			{
				Condition = $"'{promotedDisplayName}' == CHOICE {_INCLUDE_GUID}",
				Fields = new[] {promotedDisplayName}
			};
			Query totalExcludedDocumentsQuery = new Query
			{
				Condition = $"'{promotedDisplayName}' == CHOICE {_EXCLUDE_GUID}",
				Fields = new[] {promotedDisplayName}
			};
			Query totalPushedToReviewDocumentsQuery = new Query
			{
				Condition = $"'{destinationWorkspacedisplayName}' ISSET",
				Fields = new[] {destinationWorkspacedisplayName}
			};

			ObjectQueryResultSet totalUntaggedDocumentsResultSet =
				objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalUntaggedDocumentsQuery, 1, 1, _viewPermission, string.Empty).Result;
			ObjectQueryResultSet totalIncludedDocumentsResultSet =
				objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalIncludedDocumentsQuery, 1, 1, _viewPermission, string.Empty).Result;
			ObjectQueryResultSet totalExcludedDocumentsResultSet =
				objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalExcludedDocumentsQuery, 1, 1, _viewPermission, string.Empty).Result;
			ObjectQueryResultSet totalPushedToReviewDocumentsResultSet =
				objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalPushedToReviewDocumentsQuery, 1, 1, _viewPermission, string.Empty).Result;

			if (!totalUntaggedDocumentsResultSet.Success)
			{
				throw new Exception(totalUntaggedDocumentsResultSet.Message);
			}

			if (!totalIncludedDocumentsResultSet.Success)
			{
				throw new Exception(totalIncludedDocumentsResultSet.Message);
			}

			if (!totalExcludedDocumentsResultSet.Success)
			{
				throw new Exception(totalExcludedDocumentsResultSet.Message);
			}

			if (!totalPushedToReviewDocumentsResultSet.Success)
			{
				throw new Exception(totalPushedToReviewDocumentsResultSet.Message);
			}

			CurrentPromotionStatusModel model = new CurrentPromotionStatusModel
			{
				TotalDocumentsUntagged = totalUntaggedDocumentsResultSet.Data.TotalResultCount,
				TotalDocumentsIncluded = totalIncludedDocumentsResultSet.Data.TotalResultCount,
				TotalDocumentsExcluded = totalExcludedDocumentsResultSet.Data.TotalResultCount,
				TotalDocumentsPushedToReview = totalPushedToReviewDocumentsResultSet.Data.TotalResultCount
			};
			return model;
		}

		public HistoricalPromotionStatusSummaryModel GetHistoricalPromotionStatus(HistoricalPromotionStatusRequest request)
		{
			HistoricalPromotionStatusModel currentPromotionStatus = GetCurrentDocumentModelAsync(request.WorkspaceArtifactId);
			IList<HistoricalPromotionStatusModel> historicalPromotionStatus = GetHistoricalDocumentModelAsync(request.WorkspaceArtifactId);

			bool currentPromotionStatusUpdated = false;
			DateTime currentDate = currentPromotionStatus.Date.Date;

			for (int i = 0; i < historicalPromotionStatus.Count; i++)
			{
				if (historicalPromotionStatus[i].Date.Date == currentDate)
				{
					historicalPromotionStatus[i] = currentPromotionStatus;
					currentPromotionStatusUpdated = true;
					break;
				}
			}

			if (!currentPromotionStatusUpdated)
			{
				historicalPromotionStatus.Add(currentPromotionStatus);
			}

			HistoricalPromotionStatusSummaryModel model = new HistoricalPromotionStatusSummaryModel
			{
				HistoricalPromotionStatus = historicalPromotionStatus.ToArray()
			};
			return model;
		}

		private HistoricalPromotionStatusModel GetCurrentDocumentModelAsync(int workspaceId)
		{
			CurrentPromotionStatusRequest request = new CurrentPromotionStatusRequest {WorkspaceArtifactId = workspaceId};
			CurrentPromotionStatusModel currentPromotionStatus = GetCurrentPromotionStatus(request);

			HistoricalPromotionStatusModel model = new HistoricalPromotionStatusModel
			{
				Date = DateTime.UtcNow,
				TotalDocumentsIncluded = currentPromotionStatus.TotalDocumentsIncluded,
				TotalDocumentsExcluded = currentPromotionStatus.TotalDocumentsExcluded,
				TotalDocumentsUntagged = currentPromotionStatus.TotalDocumentsUntagged
			};
			return model;
		}

		private IList<HistoricalPromotionStatusModel> GetHistoricalDocumentModelAsync(int workspaceId)
		{
			IDBContext workspaceContext = global::Relativity.API.Services.Helper.GetDBContext(workspaceId);

			List<HistoricalPromotionStatusModel> historicalModels = new List<HistoricalPromotionStatusModel>();
			using (SqlDataReader reader = workspaceContext.ExecuteSQLStatementAsReader(_DOCUMENT_VOLUME_SQL))
			{
				while (reader.Read())
				{
					HistoricalPromotionStatusModel historicalModel = new HistoricalPromotionStatusModel
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

		private string GetDisplayName(int workspaceArtifactId, string artifactGuid)
		{
			SqlParameter artifactGuidParameter = new SqlParameter("@artifactGuid", artifactGuid);
			SqlParameter[] sqlParameters = {artifactGuidParameter};

			IDBContext workspaceContext = global::Relativity.API.Services.Helper.GetDBContext(workspaceArtifactId);
			string displayName = workspaceContext.ExecuteSqlStatementAsScalar<string>(_DISPLAY_NAME_SQL, sqlParameters);
			return displayName;
		}

		#region SQL Queries

		private const string _DISPLAY_NAME_SQL = @"
			SELECT F.DisplayName FROM [ArtifactGuid] AS AG
			INNER JOIN Artifact AS A WITH (NOLOCK)
			ON AG.ArtifactID = A.ArtifactID
			INNER JOIN Field AS F WITH (NOLOCK)
			ON F.ArtifactID = A.ArtifactID
			WHERE ArtifactGuid = @artifactGuid";

		private const string _DOCUMENT_VOLUME_SQL = @"
			SELECT TOP(30) [Date], [DocumentsIncluded], [DocumentsExcluded], [DocumentsUntagged]
			FROM [eddsdbo].[DocumentVolume] WITH (NOLOCK)
			ORDER BY [Date] DESC";

		#endregion
	}
}