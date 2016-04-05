using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Interfaces.Private.Requests;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Services.Security;
using API = Relativity.API;

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

		public void Dispose() { }

		private JobHistorySummaryModel GetJobHistoryInternal(JobHistoryRequest request)
		{
			var jobHistorySummary = new JobHistorySummaryModel
			{
				Data = new JobHistoryModel[0],
				TotalAvailable = 0,
				TotalDocumentsPushed = 0
			};

			IAuthenticationMgr authenticationManager = API.Services.Helper.GetAuthenticationManager();
			int userArtifactId = authenticationManager.UserInfo.ArtifactID;
			int workspaceUserArtifactId = GetWorkspaceUserArtifactId(request.WorkspaceArtifactId, userArtifactId);
			IDBContext workspaceContext = API.Services.Helper.GetDBContext(request.WorkspaceArtifactId);

			ArrayList accessControlListIds = GetJobHistoryPermissions(workspaceContext, request.WorkspaceArtifactId, workspaceUserArtifactId);
			if (accessControlListIds.Count == 0)
			{
				return jobHistorySummary;
			}

			FieldValueList<Workspace> workspaces = GetWorkspacesUserHasPermissionToView(userArtifactId);
			if (!workspaces.Any())
			{
				return jobHistorySummary;
			}

			string sortDirection = request.SortDescending ? _DESCENDING_SORT : _ASCENDING_SORT;
			string sortColumn = GetSortColumn(request.SortColumnName);

			using (SqlDataReader reader = GetJobHistoryReader(workspaceContext, accessControlListIds, sortColumn, sortDirection))
			{
				jobHistorySummary = GetJobHistorySummary(reader, request.Page, request.PageSize, workspaces);
				return jobHistorySummary;
			}
		}

		private FieldValueList<Workspace> GetWorkspacesUserHasPermissionToView(int userArtifactId)
		{
			IRSAPIClient rsapiClient = API.Services.Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			Relativity.Client.DTOs.User user = rsapiClient.Repositories.User.ReadSingle(userArtifactId);
			FieldValueList<Workspace> workspaces = user.Workspaces;
			return workspaces;
		}

		private int GetWorkspaceUserArtifactId(int workspaceArtifactId, int masterUserArtifactId)
		{
			var workspaceArtifactIdParameter = new SqlParameter("@caseArtifactId", workspaceArtifactId);
			var masterUserArtifactIdParameter = new SqlParameter("@userArtifactId", masterUserArtifactId);
			var parameters = new[] { workspaceArtifactIdParameter, masterUserArtifactIdParameter };

			string sql = @"SELECT [CaseUserArtifactID] FROM [UserCaseUser]
							WHERE [CaseArtifactID] = @caseArtifactId AND [UserArtifactID] = @userArtifactId";
			
			IDBContext masterContext = API.Services.Helper.GetDBContext(-1);
			using (SqlDataReader reader = masterContext.ExecuteParameterizedSQLStatementAsReader(sql, parameters))
			{
				if (reader.Read())
				{
					int workspaceUserArtifactId = reader.GetInt32(0);
					return workspaceUserArtifactId;
				}

				throw new Exception("You do not have permission to access this service.");
			}
		}

		private ArrayList GetJobHistoryPermissions(IDBContext workspaceContext, int workspaceArtifactId, int workspaceUserArtifactId)
		{
			string artifactTypeIdSql = @"SELECT [ArtifactTypeID] FROM [Artifact] WHERE [ArtifactID] =
										(SELECT TOP 1 [ArtifactID] FROM [JobHistory])";

			int jobHistoryArtifactTypeId = workspaceContext.ExecuteSqlStatementAsScalar<int>(artifactTypeIdSql);

			IPermissionHelper permissionHelper = API.Services.Helper.GetServicesManager().CreateProxy<IPermissionHelper>(ExecutionIdentity.System);
			ArrayList jobHistoryObjectPermissions = permissionHelper.GetViewAclList(workspaceUserArtifactId, workspaceArtifactId, jobHistoryArtifactTypeId);

			return jobHistoryObjectPermissions;
		}

		private string GetSortColumn(string sortColumnName)
		{
			string realSortColumn = _DESTINATION_WORKSPACE_COLUMN;

			switch (sortColumnName.ToLower())
			{
				case _DATE_COLUMN:
					realSortColumn = _END_TIME_UTC_COLUMN;
					break;
				case _DOCUMENT_COLUMN:
					realSortColumn = _ITEMS_IMPORTED_COLUMN;
					break;
				case _WORKSPACE_COLUMN:
					realSortColumn = _DESTINATION_WORKSPACE_COLUMN;
					break;
			}

			return realSortColumn;
		}

		private SqlDataReader GetJobHistoryReader(IDBContext workspaceContext, ArrayList accessControlListIds, string sortColumn, string sortDirection)
		{
			string joinedAclIds = String.Join(",", accessControlListIds.ToArray());

			string sql = String.Format(@"SELECT [ItemsImported], [EndTimeUTC], [DestinationWorkspace]
							FROM [EDDSDBO].[JobHistory] as JH
							INNER JOIN [EDDSDBO].Artifact as A
							ON JH.ArtifactID = A.ArtifactID
							WHERE A.AccessControlListID in ({0})
							ORDER BY [{1}] {2}", joinedAclIds, sortColumn, sortDirection);

			SqlDataReader reader = workspaceContext.ExecuteSQLStatementAsReader(sql);
			return reader;
		}

		private JobHistorySummaryModel GetJobHistorySummary(SqlDataReader reader, int page, int pageSize, FieldValueList<Workspace> workspaces)
		{
			int start = (page - 1) * pageSize;
			int end = start + pageSize;

			IList<JobHistoryModel> jobHistories = new List<JobHistoryModel>(pageSize);
			Int64 totalAvailable = 0;
			Int64 totalDocuments = 0;

			while(reader.Read())
			{
				string destinationWorkspace = reader.GetString(2);
				bool userHasPermission = DoesUserHavePermissionToThisDestinationWorkspace(workspaces, destinationWorkspace);
				if (!userHasPermission)
				{
					continue;
				}

				int jobHistoryItemsImported = reader.GetInt32(0);

				if (totalAvailable >= start && totalAvailable < end)
				{
					DateTime endTimeUtc = reader.GetDateTime(1);

					var jobHistory = new JobHistoryModel
					{
						ItemsImported = jobHistoryItemsImported,
						EndTimeUtc = endTimeUtc,
						DestinationWorkspace = destinationWorkspace
					};
					jobHistories.Add(jobHistory);
				}

				totalDocuments += jobHistoryItemsImported;
				totalAvailable++;
			}

			var jobHistorySummary = new JobHistorySummaryModel
			{
				Data = jobHistories.ToArray(),
				TotalAvailable = totalAvailable,
				TotalDocumentsPushed = totalDocuments
			};

			return jobHistorySummary;
		}

		private bool DoesUserHavePermissionToThisDestinationWorkspace(FieldValueList<Workspace> accessibleWorkspaces, string destinationWorkspace)
		{
			try
			{
				string substringCheck = "[Id::";
				int workspaceArtifactIdStartIndex = destinationWorkspace.LastIndexOf(substringCheck, StringComparison.CurrentCulture) + substringCheck.Length;
				int workspaceArtifactIdEndIndex = destinationWorkspace.LastIndexOf("]", StringComparison.CurrentCulture);
				string workspaceArtifactIdSubstring = destinationWorkspace.Substring(workspaceArtifactIdStartIndex, workspaceArtifactIdEndIndex - workspaceArtifactIdStartIndex);
				int workspaceArtifactId = Int32.Parse(workspaceArtifactIdSubstring);

				return accessibleWorkspaces.Any(t => t.ArtifactID == workspaceArtifactId);
			}
			catch (Exception e)
			{
				throw new Exception("The formatting of the destination workspace information has changed and cannot be parsed.", e);
			}
		}
	}
}