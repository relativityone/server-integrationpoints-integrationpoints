using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Logging;
using Relativity.Services.Security;
using User = kCura.Relativity.Client.DTOs.User;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	///     This class is using direct sql because kepler does not provide the ability to aggregate data.
	/// </summary>
	public class JobHistoryManager : KeplerServiceBase, IJobHistoryManager
	{
		private const string _ASCENDING_SORT = "ASC";
		private const string _DESCENDING_SORT = "DESC";
		private const string _RELATIVITY_PROVIDER_GUID = "423b4d43-eae9-4e14-b767-17d629de4bb2";
		private const string _NO_ACCESS_EXCEPTION_MESSAGE = "You do not have permission to access this service.";

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositryFactory"></param>
		internal JobHistoryManager(ILog logger, IPermissionRepositoryFactory permissionRepositryFactory) : base(logger, permissionRepositryFactory)
		{
		}

		public JobHistoryManager(ILog logger) : base(logger)
		{
		}

		public void Dispose()
		{
		}

		public async Task<JobHistorySummaryModel> GetJobHistoryAsync(JobHistoryRequest request)
		{
			CheckPermissions(request.WorkspaceArtifactId);
			return await Task.Run(() => GetJobHistoryInternal(request));
		}

		private JobHistorySummaryModel GetJobHistoryInternal(JobHistoryRequest request)
		{
			try
			{
				// Determine if the user first has access to workspaces and object type
				IAuthenticationMgr authenticationManager = global::Relativity.API.Services.Helper.GetAuthenticationManager();
				int userArtifactId = authenticationManager.UserInfo.ArtifactID;
				int workspaceUserArtifactId = GetWorkspaceUserArtifactId(request.WorkspaceArtifactId, userArtifactId);

				var jobHistorySummary = new JobHistorySummaryModel();

				IDBContext workspaceContext = global::Relativity.API.Services.Helper.GetDBContext(request.WorkspaceArtifactId);

				int jobHistoryArtifactTypeId = GetJobHistoryArtifactTypeId(workspaceContext);
				if (jobHistoryArtifactTypeId == 0)
				{
					return jobHistorySummary;
				}

				ArrayList accessControlListIds = GetArtifactTypePermissions(request.WorkspaceArtifactId, workspaceUserArtifactId, jobHistoryArtifactTypeId);
				if (accessControlListIds.Count == 0)
				{
					throw new Exception(_NO_ACCESS_EXCEPTION_MESSAGE);
				}

				FieldValueList<Workspace> workspaces = GetWorkspacesUserHasPermissionToView(userArtifactId);
				if (!workspaces.Any())
				{
					return jobHistorySummary;
				}

				IList<int> jobHistoryArtifactIds = GetRelativityProviderJobHistoryArtifactIds(workspaceContext, jobHistoryArtifactTypeId);
				if (jobHistoryArtifactIds.Count == 0)
				{
					return jobHistorySummary;
				}

				bool sortDescending = request.SortDescending ?? false;
				string sortDirection = sortDescending ? _DESCENDING_SORT : _ASCENDING_SORT;
				string sortColumn = GetSortColumn(request.SortColumnName);

				using (SqlDataReader reader = GetJobHistoryReader(workspaceContext, accessControlListIds, jobHistoryArtifactIds, sortColumn, sortDirection))
				{
					jobHistorySummary = GetJobHistorySummary(reader, request.Page, request.PageSize, workspaces);
					return jobHistorySummary;
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "{0}.{1}", nameof(JobHistoryManager), nameof(GetJobHistoryInternal));
				throw;
			}
		}

		private int GetWorkspaceUserArtifactId(int workspaceArtifactId, int masterUserArtifactId)
		{
			var workspaceArtifactIdParameter = new SqlParameter("@caseArtifactId", workspaceArtifactId);
			var masterUserArtifactIdParameter = new SqlParameter("@userArtifactId", masterUserArtifactId);
			var sqlParameters = new[] {workspaceArtifactIdParameter, masterUserArtifactIdParameter};

			IDBContext masterContext = global::Relativity.API.Services.Helper.GetDBContext(-1);
			using (SqlDataReader reader = masterContext.ExecuteParameterizedSQLStatementAsReader(_USER_WORKSPACE_ARTIFACT_ID_SQL, sqlParameters))
			{
				if (reader.Read())
				{
					int workspaceUserArtifactId = reader.GetInt32(0);
					return workspaceUserArtifactId;
				}

				throw new Exception(_NO_ACCESS_EXCEPTION_MESSAGE);
			}
		}

		private int GetJobHistoryArtifactTypeId(IDBContext workspaceContext)
		{
			try
			{
				int artifactTypeId = workspaceContext.ExecuteSqlStatementAsScalar<int>(_JOB_HISTORY_ARTIFACT_TYPE_ID_SQL);
				return artifactTypeId;
			}
			catch (Exception sqlException)
			{
				if (sqlException.InnerException?.Message.Equals("Invalid object name 'JobHistory'.") == true)
				{
					return 0;
				}

				throw;
			}
		}

		private ArrayList GetArtifactTypePermissions(int workspaceArtifactId, int workspaceUserArtifactId, int artifactTypeId)
		{
			using (IPermissionHelper permissionHelper = global::Relativity.API.Services.Helper.GetServicesManager().CreateProxy<IPermissionHelper>(ExecutionIdentity.System))
			{
				ArrayList artifactTypePermissions = permissionHelper.GetViewAclList(workspaceUserArtifactId, workspaceArtifactId, artifactTypeId);
				return artifactTypePermissions;
			}
		}

		private FieldValueList<Workspace> GetWorkspacesUserHasPermissionToView(int userArtifactId)
		{
			using (IRSAPIClient rsapiClient = global::Relativity.API.Services.Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				User user = rsapiClient.Repositories.User.ReadSingle(userArtifactId);
				FieldValueList<Workspace> workspaces = user.Workspaces;
				return workspaces;
			}
		}

		private IList<int> GetRelativityProviderJobHistoryArtifactIds(IDBContext workspaceContext, int jobHistoryArtifactTypeId)
		{
			IList<int> integrationPointArtifactIds = GetProviderIntegrationPointArtifactIds(workspaceContext, _RELATIVITY_PROVIDER_GUID);
			IList<int> jobHistoryArtifactIds = GetJobHistoryArtifactIds(workspaceContext, integrationPointArtifactIds, jobHistoryArtifactTypeId);
			return jobHistoryArtifactIds;
		}

		private IList<int> GetProviderIntegrationPointArtifactIds(IDBContext workspaceContext, string providerGuid)
		{
			var relativityProviderParameter = new SqlParameter("@sourceProviderIdentifier", providerGuid);
			var sqlParameters = new[] {relativityProviderParameter};

			using (SqlDataReader reader = workspaceContext.ExecuteParameterizedSQLStatementAsReader(_PROVIDER_INTEGRATION_POINT_ARTIFACT_IDS_SQL, sqlParameters))
			{
				IList<int> integrationPointArtifactIds = new List<int>();
				while (reader.Read())
				{
					int integrationPointArtifactId = reader.GetInt32(0);
					integrationPointArtifactIds.Add(integrationPointArtifactId);
				}

				return integrationPointArtifactIds;
			}
		}

		private IList<int> GetJobHistoryArtifactIds(IDBContext workspaceContext, IList<int> integrationPointArtifactIds, int jobHistoryArtifactTypeId)
		{
			var fieldAssociativeArtifactTypeIdParameter = new SqlParameter("@fieldAssociativeArtifactTypeID", jobHistoryArtifactTypeId);
			int integrationPointArtifactId = integrationPointArtifactIds.FirstOrDefault();
			var artifactIdParameter = new SqlParameter("@artifactID", integrationPointArtifactId);
			var relationalSqlParameters = new[] {fieldAssociativeArtifactTypeIdParameter, artifactIdParameter};

			string relationalTableSchemaName;
			string relationalTableFieldColumnName1;
			string relationalTableFieldColumnName2;

			using (SqlDataReader reader = workspaceContext.ExecuteParameterizedSQLStatementAsReader(
				_INTEGRATION_POINT_JOB_HISTORY_RELATIONAL_TABLE_INFORMATION_SQL, relationalSqlParameters))
			{
				if (!reader.Read())
				{
					return new List<int>(0);
				}

				relationalTableSchemaName = reader.GetString(0);
				relationalTableFieldColumnName1 = reader.GetString(1);
				relationalTableFieldColumnName2 = reader.GetString(2);
			}

			string joinedIntegrationPointArtifactIds = string.Join(",", integrationPointArtifactIds);
			string integrationPointJobHistoryArtifactIdsSql = string.Format(
				@"SELECT {0} FROM {1} WHERE {2} IN ({3})",
				relationalTableFieldColumnName2, relationalTableSchemaName,
				relationalTableFieldColumnName1, joinedIntegrationPointArtifactIds);

			using (SqlDataReader reader = workspaceContext.ExecuteSQLStatementAsReader(integrationPointJobHistoryArtifactIdsSql))
			{
				IList<int> integrationPointJobHistoryArtifactIds = new List<int>();
				while (reader.Read())
				{
					integrationPointJobHistoryArtifactIds.Add(reader.GetInt32(0));
				}

				return integrationPointJobHistoryArtifactIds;
			}
		}

		private SqlDataReader GetJobHistoryReader(IDBContext workspaceContext, ArrayList accessControlListIds, IList<int> jobHistoryArtifactIds, string sortColumn,
			string sortDirection)
		{
			string joinedAclIds = string.Join(",", accessControlListIds.ToArray());
			string joinedJobHistoryArtifactIds = string.Join(",", jobHistoryArtifactIds);
			string formattedJobHistoriesSql = string.Format(_JOB_HISTORIES_COMPLETED_WITH_ITEMS_PUSHED_SQL, joinedAclIds, joinedJobHistoryArtifactIds, sortColumn, sortDirection);

			SqlDataReader reader = workspaceContext.ExecuteSQLStatementAsReader(formattedJobHistoriesSql);
			return reader;
		}

		private JobHistorySummaryModel GetJobHistorySummary(SqlDataReader reader, int page, int pageSize, FieldValueList<Workspace> workspaces)
		{
			int start = page*pageSize;
			int end = start + pageSize;

			IList<JobHistoryModel> jobHistories = new List<JobHistoryModel>(pageSize);
			long totalAvailable = 0;
			long totalDocuments = 0;

			while (reader.Read())
			{
				string destinationWorkspace = reader.GetString(2);
				bool userHasPermission = DoesUserHavePermissionToThisDestinationWorkspace(workspaces, destinationWorkspace);
				if (!userHasPermission)
				{
					continue;
				}

				int jobHistoryItemsTransferred = reader.GetInt32(0);

				if ((totalAvailable >= start) && (totalAvailable < end))
				{
					DateTime endTimeUtc = reader.GetDateTime(1);

					var jobHistory = new JobHistoryModel
					{
						ItemsTransferred = jobHistoryItemsTransferred,
						EndTimeUtc = endTimeUtc,
						DestinationWorkspace = destinationWorkspace
					};
					jobHistories.Add(jobHistory);
				}

				totalDocuments += jobHistoryItemsTransferred;
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
				string substringCheck = "-";
				int workspaceArtifactIdStartIndex = destinationWorkspace.LastIndexOf(substringCheck, StringComparison.CurrentCulture) + substringCheck.Length;
				int workspaceArtifactIdEndIndex = destinationWorkspace.Length;
				string workspaceArtifactIdSubstring = destinationWorkspace.Substring(workspaceArtifactIdStartIndex, workspaceArtifactIdEndIndex - workspaceArtifactIdStartIndex);
				int workspaceArtifactId = int.Parse(workspaceArtifactIdSubstring);

				return accessibleWorkspaces.Any(t => t.ArtifactID == workspaceArtifactId);
			}
			catch (Exception e)
			{
				throw new Exception("The formatting of the destination workspace information has changed and cannot be parsed.", e);
			}
		}

		private string GetSortColumn(string sortColumnName)
		{
			string sortColumn = string.IsNullOrEmpty(sortColumnName)
				? nameof(JobHistoryModel.DestinationWorkspace)
				: sortColumnName;

			return sortColumn;
		}

		#region SQL Queries

		private const string _INTEGRATION_POINT_JOB_HISTORY_RELATIONAL_TABLE_INFORMATION_SQL = @"
			SELECT OFR.[RelationalTableSchemaName], OFR.[RelationalTableFieldColumnName1], OFR.[RelationalTableFieldColumnName2]
			FROM [ObjectsFieldRelation] AS OFR WITH (NOLOCK)
			INNER JOIN [Field] AS F WITH (NOLOCK)
			ON F.[ArtifactID] = OFR.[FieldArtifactId1]
			INNER JOIN [Artifact] AS A WITH (NOLOCK)
			ON F.[FieldArtifactTypeID] = A.[ArtifactTypeID]
			WHERE F.[AssociativeArtifactTypeID] = @fieldAssociativeArtifactTypeID AND A.[ArtifactID] = @artifactID";

		private const string _PROVIDER_INTEGRATION_POINT_ARTIFACT_IDS_SQL = @"
			SELECT IP.[ArtifactID]
			FROM [SourceProvider] AS SP WITH (NOLOCK)
			INNER JOIN [IntegrationPoint] AS IP WITH (NOLOCK)
			ON SP.[ArtifactID] = IP.[SourceProvider]
			WHERE SP.[Identifier] = @sourceProviderIdentifier";

		private const string _USER_WORKSPACE_ARTIFACT_ID_SQL = @"
			SELECT [CaseUserArtifactID] FROM [UserCaseUser] WITH (NOLOCK)
			WHERE [CaseArtifactID] = @caseArtifactId AND [UserArtifactID] = @userArtifactId";

		private const string _JOB_HISTORY_ARTIFACT_TYPE_ID_SQL = @"
			SELECT[ArtifactTypeID] FROM[Artifact] WITH (NOLOCK) WHERE[ArtifactID] =
				(SELECT TOP 1 [ArtifactID] FROM[JobHistory] WITH (NOLOCK))";

		private const string _JOB_HISTORIES_COMPLETED_WITH_ITEMS_PUSHED_SQL = @"
			SELECT [ItemsTransferred], [EndTimeUTC], [DestinationWorkspace]
			FROM [JobHistory] AS JH WITH (NOLOCK)
			INNER JOIN [Artifact] AS A WITH (NOLOCK)
			ON JH.[ArtifactID] = A.[ArtifactID]
			WHERE A.[AccessControlListID] in ({0}) AND JH.[ArtifactID] in ({1}) AND JH.[EndTimeUTC] IS NOT NULL AND JH.[ItemsTransferred] > 0
			ORDER BY [{2}] {3}";

		#endregion
	}
}