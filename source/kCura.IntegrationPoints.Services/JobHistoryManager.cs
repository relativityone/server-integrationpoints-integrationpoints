using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Interfaces.Private.Requests;
using Relativity.API;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	/// This class is using direct sql because kepler does not provide the ability to aggregate data.
	/// </summary>
	public class JobHistoryManager : IJobHistoryManager
	{
		private const string _ASCENDING_SORT = "ASC";
		private const string _DESCENDING_SORT = "DESC";
		private const string _DOCUMENT_COLUMN = "documents";
		private const string _DATE_COLUMN = "date";
		private const string _WORKSPACE_COLUMN = "workspacename";

		private const string _ITEMS_IMPORTED_COLUMN = "ItemsImported";
		private const string _DESTINATION_WORKSPACE_COLUMN = "DestinationWorkspace";
		private const string _END_TIME_UTC_COLUMN = "EndTimeUTC";

		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}

		public async Task<JobHistorySummaryModel> GetJobHistory(JobHistoryRequest request)
		{
			return await Task.Run(() => GetJobHistoryInternal(request)).ConfigureAwait(false);
		}

		private JobHistorySummaryModel GetJobHistoryInternal(JobHistoryRequest request)
		{
			if (!String.Equals(request.SortDirection, _ASCENDING_SORT, StringComparison.OrdinalIgnoreCase)
				&& !String.Equals(request.SortDirection, _DESCENDING_SORT, StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception("Sort direction should be 'ASC' or 'DESC'");
			}
			string sortColumn = GetSortColumn(request.SortColumnName);

			IDBContext dbContext = Relativity.API.Services.Helper.GetDBContext(request.WorkspaceArtifactId);

			int sumItemsImported = GetSumItemsImported(dbContext);

			// TODO: We need to retrieve WorkspaceUserArtifactID but this currently does not work - Dan Nelson 3/25/2016
			IAuthenticationMgr authenticationManager = Relativity.API.Services.Helper.GetAuthenticationManager();
			IEnumerable<int> accessControlListIds = GetAccessControlListIds(dbContext, authenticationManager.UserInfo.ArtifactID);

			using (SqlDataReader reader = GetJobHistoryReader(dbContext, sortColumn, request.SortDirection, accessControlListIds))
			{
				JobHistoryModel[] jobHistories = GetJobHistories(reader, request.Page, request.PageSize);

				JobHistorySummaryModel jobHistorySummaryModel = new JobHistorySummaryModel
				{
					JobHistories = jobHistories,
					TotalDocumentsPushed = sumItemsImported
				};
				return jobHistorySummaryModel;
			}

		}

		private string GetSortColumn(string sortColumnName)
		{
			string realSortColumn;
			switch (sortColumnName.ToLower())
			{
				case _DOCUMENT_COLUMN:
					realSortColumn = _ITEMS_IMPORTED_COLUMN;
					break;
				case _DATE_COLUMN:
					realSortColumn = _END_TIME_UTC_COLUMN;
					break;
				case _WORKSPACE_COLUMN:
					realSortColumn = _DESTINATION_WORKSPACE_COLUMN;
					break;
				default:
					throw new Exception(String.Format("Sort column name must be {0}, {1}, or {2}", _DOCUMENT_COLUMN, _DATE_COLUMN, _WORKSPACE_COLUMN));
			}
			return realSortColumn;
		}

		private int GetSumItemsImported(IDBContext context)
		{
			string sqlItemsSum = @"SELECT SUM([ItemsImported])
									FROM [EDDSDBO].[JobHistory]";

			int sumItemsImported = context.ExecuteSqlStatementAsScalar<int>(sqlItemsSum);
			return sumItemsImported;
		}

		private IEnumerable<int> GetAccessControlListIds(IDBContext context, int userArtifactId)
		{
			List<int> accessControllListIds = new List<int>();

			SqlParameter userArtifactIdParameter = new SqlParameter("@userArtifactId", userArtifactId);
			string sql = @"SELECT DISTINCT(AccessControlListID) FROM [EDDSDBO].[GroupUser] as GU
								INNER JOIN [EDDSDBO].[AccessControlListPermission] as ACLP
								ON GU.GroupArtifactID = ACLP.GroupID
								WHERE GU.UserArtifactID = @userArtifactId";

			using (SqlDataReader reader = context.ExecuteParameterizedSQLStatementAsReader(sql, new[] {userArtifactIdParameter}))
			{
				while (reader.Read())
				{
					accessControllListIds.Add(reader.GetInt32(0));
				}
			}
			return accessControllListIds;
		}

		// TODO: This needs to only return items the user has permissions for
		private SqlDataReader GetJobHistoryReader(IDBContext context, string sortColumn, string sortDirection, IEnumerable<int> accessControlListIds)
		{
			string sql = String.Format(@"SELECT [ItemsImported], [EndTimeUTC], [DestinationWorkspace]
											FROM [EDDSDBO].[JobHistory] as JH
											INNER JOIN [EDDSDBO].Artifact as A
											ON JH.ArtifactID = A.ArtifactID
											WHERE A.AccessControlListID in ({2})
											ORDER BY [{0}] {1}", sortColumn, sortDirection, String.Join(",", accessControlListIds));

			SqlDataReader reader = context.ExecuteSQLStatementAsReader(sql);
			return reader;
		}

		private JobHistoryModel[] GetJobHistories(SqlDataReader reader, int page, int pageSize)
		{
			int start = (page - 1) * pageSize;
			int end = start + pageSize;

			List<JobHistoryModel> jobHistoryModels = new List<JobHistoryModel>();
			int count = 0;
			while (reader.Read())
			{
				if (count >= start && count < end)
				{
					jobHistoryModels.Add(new JobHistoryModel()
					{
						Documents = reader.GetInt32(0),
						Date = reader.GetDateTime(1),
						WorkspaceName = reader.GetString(2)
					});
				}
				count++;
			}
			return jobHistoryModels.ToArray();
		}

		public void Dispose()
		{
		}
	}
}