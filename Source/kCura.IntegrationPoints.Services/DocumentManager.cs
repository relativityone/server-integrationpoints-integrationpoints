using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Logging;
using Relativity.Services.ObjectQuery;

using API = Relativity.API;

using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.Services
{
    /// <summary>
    /// Get information about the documents in ECA case such as pushed to
    /// review, included, excluded, untagged, etc.
    /// </summary>
    public class DocumentManager : KeplerServiceBase, IDocumentManager
    {
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

        private const string _DESTINATION_WORKSPACE_FIELD_GUID = "8980C2FA-0D33-4686-9A97-EA9D6F0B4196";
        private const string _PROMOTE_GUID = "4E418A56-90C5-4E59-A1C5-C43C11A3CCFF";
        private const string _INCLUDE_GUID = "6884BAC4-DD8F-4087-9C17-B4BCE99815D5";
        private const string _EXCLUDE_GUID = "DB110A00-AC87-4C40-96E2-827BF9B18909";

        private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

        private static readonly int[] _viewPermission = { 1 };

        public DocumentManager(ILog logger) : base(logger)
        {
        }

        public void Dispose()
        {
        }

        public async Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request)
        {
            return await Task.Run(() => GetPercentagePushedToReviewInternalAsync(request)).ConfigureAwait(false);
        }

        public async Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request)
        {
            return await Task.Run(() => GetCurrentPromotionStatusInternalAsync(request)).ConfigureAwait(false);
        }

        public async Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request)
        {
            return await Task.Run(() => GetHistoricalPromotionStatusInternalAsync(request)).ConfigureAwait(false);
        }

        private async Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewInternalAsync(PercentagePushedToReviewRequest request)
        {
            try
            {
                string destinationWorkspaceDisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_FIELD_GUID);

                IObjectQueryManager objectQueryManager = API.Services.Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
                Query totalDocumentsQuery = new Query();
                Query totalDocumentsPushedToReviewQuery = new Query
                {
                    Condition = $"'{destinationWorkspaceDisplayName}' ISSET",
                    Fields = new[] { destinationWorkspaceDisplayName }
                };

                ObjectQueryResultSet totalDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalDocumentsQuery, 1, 1, _viewPermission, string.Empty);
                ObjectQueryResultSet totalDocumentsPushedToReviewResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalDocumentsPushedToReviewQuery, 1, 1, _viewPermission, string.Empty);

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
            catch (Exception ex)
            {
                Logger.LogError(ex, "{0}.{1}", nameof(DocumentManager), nameof(GetPercentagePushedToReviewInternalAsync));
                throw;
            }
        }

        private async Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusInternalAsync(CurrentPromotionStatusRequest request)
        {
            try
            {
                string promotedDisplayName = GetDisplayName(request.WorkspaceArtifactId, _PROMOTE_GUID);
                string destinationWorkspacedisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_FIELD_GUID);

                IObjectQueryManager objectQueryManager = API.Services.Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.System);
                Query totalUntaggedDocumentsQuery = new Query
                {
                    Condition = $"NOT '{promotedDisplayName}' ISSET",
                    Fields = new[] { promotedDisplayName }
                };
                Query totalIncludedDocumentsQuery = new Query
                {
                    Condition = $"'{promotedDisplayName}' == CHOICE {_INCLUDE_GUID}",
                    Fields = new[] { promotedDisplayName }
                };
                Query totalExcludedDocumentsQuery = new Query
                {
                    Condition = $"'{promotedDisplayName}' == CHOICE {_EXCLUDE_GUID}",
                    Fields = new[] { promotedDisplayName }
                };
                Query totalPushedToReviewDocumentsQuery = new Query
                {
                    Condition = $"'{destinationWorkspacedisplayName}' ISSET",
                    Fields = new[] { destinationWorkspacedisplayName }
                };

                ObjectQueryResultSet totalUntaggedDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalUntaggedDocumentsQuery, 1, 1, _viewPermission, string.Empty);
                ObjectQueryResultSet totalIncludedDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalIncludedDocumentsQuery, 1, 1, _viewPermission, string.Empty);
                ObjectQueryResultSet totalExcludedDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalExcludedDocumentsQuery, 1, 1, _viewPermission, string.Empty);
                ObjectQueryResultSet totalPushedToReviewDocumentsResultSet = await objectQueryManager.QueryAsync(request.WorkspaceArtifactId, _DOCUMENT_ARTIFACT_TYPE_ID, totalPushedToReviewDocumentsQuery, 1, 1, _viewPermission, string.Empty);

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
            catch (Exception ex)
            {
                Logger.LogError(ex, "{0}.{1}", nameof(DocumentManager), nameof(GetCurrentPromotionStatusInternalAsync));
                throw;
            }
        }

        private async Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusInternalAsync(HistoricalPromotionStatusRequest request)
        {
            try
            {
                HistoricalPromotionStatusModel currentPromotionStatus = await GetCurrentDocumentModelAsync(request.WorkspaceArtifactId);
                IList<HistoricalPromotionStatusModel> historicalPromotionStatus = await GetHistoricalDocumentModelAsync(request.WorkspaceArtifactId);

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
            catch (Exception ex)
            {
                Logger.LogError(ex, "{0}.{1}", nameof(DocumentManager), nameof(GetHistoricalPromotionStatusInternalAsync));
                throw;
            }
        }

        private async Task<HistoricalPromotionStatusModel> GetCurrentDocumentModelAsync(int workspaceId)
        {
            try
            {
                CurrentPromotionStatusRequest request = new CurrentPromotionStatusRequest { WorkspaceArtifactId = workspaceId };
                CurrentPromotionStatusModel currentPromotionStatus = await GetCurrentPromotionStatusInternalAsync(request);

                HistoricalPromotionStatusModel model = new HistoricalPromotionStatusModel
                {
                    Date = DateTime.UtcNow,
                    TotalDocumentsIncluded = currentPromotionStatus.TotalDocumentsIncluded,
                    TotalDocumentsExcluded = currentPromotionStatus.TotalDocumentsExcluded,
                    TotalDocumentsUntagged = currentPromotionStatus.TotalDocumentsUntagged
                };
                return model;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{0}.{1}", nameof(DocumentManager), nameof(GetCurrentDocumentModelAsync));
                throw;
            }
        }

        private async Task<IList<HistoricalPromotionStatusModel>> GetHistoricalDocumentModelAsync(int workspaceId)
        {
            try
            {
                IDBContext workspaceContext = API.Services.Helper.GetDBContext(workspaceId);

                List<HistoricalPromotionStatusModel> historicalModels = new List<HistoricalPromotionStatusModel>();
                using (SqlDataReader reader = workspaceContext.ExecuteSQLStatementAsReader(_DOCUMENT_VOLUME_SQL))
                {
                    while (await reader.ReadAsync())
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
            catch (Exception ex)
            {
                Logger.LogError(ex, "{0}.{1}", nameof(DocumentManager), nameof(GetHistoricalDocumentModelAsync));
                throw;
            }
        }

        private string GetDisplayName(int workspaceArtifactId, string artifactGuid)
        {
            SqlParameter artifactGuidParameter = new SqlParameter("@artifactGuid", artifactGuid);
            SqlParameter[] sqlParameters = { artifactGuidParameter };

            IDBContext workspaceContext = API.Services.Helper.GetDBContext(workspaceArtifactId);
            string displayName = workspaceContext.ExecuteSqlStatementAsScalar<string>(_DISPLAY_NAME_SQL, sqlParameters);
            return displayName;
        }
    }
}