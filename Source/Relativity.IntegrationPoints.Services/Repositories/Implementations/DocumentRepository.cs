using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Factories;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Services.Repositories.Implementations
{
    public class DocumentRepository : IDocumentRepository
    {
        private const string _DESTINATION_WORKSPACE_FIELD_GUID = "8980C2FA-0D33-4686-9A97-EA9D6F0B4196";
        private const string _PROMOTE_GUID = "4E418A56-90C5-4E59-A1C5-C43C11A3CCFF";
        private const string _INCLUDE_GUID = "6884BAC4-DD8F-4087-9C17-B4BCE99815D5";
        private const string _EXCLUDE_GUID = "DB110A00-AC87-4C40-96E2-827BF9B18909";

        private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int) ArtifactType.Document;

        private readonly IRelativityObjectManagerFactory _releRelativityObjectManagerFactory;

        public DocumentRepository(IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _releRelativityObjectManagerFactory = relativityObjectManagerFactory;
        }

        public async Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request)
        {
            var releRelativityObjectManager =
                _releRelativityObjectManagerFactory.CreateRelativityObjectManager(request.WorkspaceArtifactId);
            var queryDocs = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID }

            };
            string destinationWorkspaceDisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_FIELD_GUID);
            var queryDocsPushedToReview = new QueryRequest
            {
                Condition = $"'{destinationWorkspaceDisplayName}' ISSET",
                Fields = new [] { new FieldRef { Guid = new Guid(_DESTINATION_WORKSPACE_FIELD_GUID) } },
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID }

            };
            int totalDocuments = await releRelativityObjectManager.QueryTotalCountAsync(queryDocs).ConfigureAwait(false);
            int totalDocumentsPushedToReview = await releRelativityObjectManager.QueryTotalCountAsync(queryDocsPushedToReview).ConfigureAwait(false);

            PercentagePushedToReviewModel model = new PercentagePushedToReviewModel
            {
                TotalDocuments = totalDocuments,
                TotalDocumentsPushedToReview = totalDocumentsPushedToReview
            };
            return model;
        }

        public async Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request)
        {
            var relativityObjectManager =
                _releRelativityObjectManagerFactory.CreateRelativityObjectManager(request.WorkspaceArtifactId);
            string promotedDisplayName = GetDisplayName(request.WorkspaceArtifactId, _PROMOTE_GUID);
            string destinationWorkspacedisplayName = GetDisplayName(request.WorkspaceArtifactId, _DESTINATION_WORKSPACE_FIELD_GUID);

            var totalUntaggedDocumentsQuery = new QueryRequest
            {
                Condition = $"NOT '{promotedDisplayName}' ISSET",
                Fields = new[] { new FieldRef { Guid = new Guid(_PROMOTE_GUID) } },
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID }
            };
            var totalIncludedDocumentsQuery = new QueryRequest
            {
                Condition = $"'{promotedDisplayName}' == CHOICE {_INCLUDE_GUID}",
                Fields = new[] { new FieldRef { Guid = new Guid(_PROMOTE_GUID) } },
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID }
            };
            var totalExcludedDocumentsQuery = new QueryRequest
            {
                Condition = $"'{promotedDisplayName}' == CHOICE {_EXCLUDE_GUID}",
                Fields = new[] { new FieldRef { Guid = new Guid(_PROMOTE_GUID) } },
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID }
            };
            var totalPushedToReviewDocumentsQuery = new QueryRequest
            {
                Condition = $"'{destinationWorkspacedisplayName}' ISSET",
                Fields = new[] { new FieldRef { Guid = new Guid(_DESTINATION_WORKSPACE_FIELD_GUID) } },
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID }
            };

            var totalUntaggedDocumentsTask = relativityObjectManager.QueryTotalCountAsync(totalUntaggedDocumentsQuery).ConfigureAwait(false);
            var totalIncludedDocumentsTask = relativityObjectManager.QueryTotalCountAsync(totalIncludedDocumentsQuery).ConfigureAwait(false);
            var totalExcludedDocumentsTask = relativityObjectManager.QueryTotalCountAsync(totalExcludedDocumentsQuery).ConfigureAwait(false);
            var totalPushedToReviewTask = relativityObjectManager.QueryTotalCountAsync(totalPushedToReviewDocumentsQuery).ConfigureAwait(false);

            var totalUntaggedDocumentsResultSet = await totalUntaggedDocumentsTask;
            var totalIncludedDocumentsResultSet = await totalIncludedDocumentsTask;
            var totalExcludedDocumentsResultSet = await totalExcludedDocumentsTask;
            var totalPushedToReviewDocumentsResultSet = await totalPushedToReviewTask;

            CurrentPromotionStatusModel model = new CurrentPromotionStatusModel
            {
                TotalDocumentsUntagged = totalUntaggedDocumentsResultSet,
                TotalDocumentsIncluded = totalIncludedDocumentsResultSet,
                TotalDocumentsExcluded = totalExcludedDocumentsResultSet,
                TotalDocumentsPushedToReview = totalPushedToReviewDocumentsResultSet
            };
            return model;
        }

        public async Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request)
        {
            HistoricalPromotionStatusModel currentPromotionStatus = await GetCurrentDocumentModelAsync(request.WorkspaceArtifactId).ConfigureAwait(false);
            IList<HistoricalPromotionStatusModel> historicalPromotionStatus = await GetHistoricalDocumentModelAsync(request.WorkspaceArtifactId).ConfigureAwait(false);

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

        private async Task<HistoricalPromotionStatusModel> GetCurrentDocumentModelAsync(int workspaceId)
        {
            CurrentPromotionStatusRequest request = new CurrentPromotionStatusRequest {WorkspaceArtifactId = workspaceId};
            CurrentPromotionStatusModel currentPromotionStatus = await GetCurrentPromotionStatusAsync(request).ConfigureAwait(false);

            HistoricalPromotionStatusModel model = new HistoricalPromotionStatusModel
            {
                Date = DateTime.UtcNow,
                TotalDocumentsIncluded = currentPromotionStatus.TotalDocumentsIncluded,
                TotalDocumentsExcluded = currentPromotionStatus.TotalDocumentsExcluded,
                TotalDocumentsUntagged = currentPromotionStatus.TotalDocumentsUntagged
            };
            return model;
        }

        private async Task<IList<HistoricalPromotionStatusModel>> GetHistoricalDocumentModelAsync(int workspaceId)
        {
#pragma warning disable CS0618 // Type or member is obsolete REL-292860
            IDBContext workspaceContext = global::Relativity.API.Services.Helper.GetDBContext(workspaceId);
#pragma warning restore CS0618 // Type or member is obsolete

            List<HistoricalPromotionStatusModel> historicalModels = new List<HistoricalPromotionStatusModel>();
            using (SqlDataReader reader = workspaceContext.ExecuteSQLStatementAsReader(_DOCUMENT_VOLUME_SQL))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
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

#pragma warning disable CS0618 // Type or member is obsolete REL-292860
            IDBContext workspaceContext = global::Relativity.API.Services.Helper.GetDBContext(workspaceArtifactId);
#pragma warning restore CS0618 // Type or member is obsolete
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