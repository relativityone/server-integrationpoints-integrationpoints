using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Logging;
using User = kCura.Relativity.Client.DTOs.User;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public class JobHistoryRepository : IJobHistoryRepository
	{
		private const string _JOB_HISTORY_OBJECT_TYPE_GUID = "08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9";

		private readonly ILog _logger;
		private readonly IServiceHelper _helper;

		public JobHistoryRepository(ILog logger)
		{
			_logger = logger;
			_helper = global::Relativity.API.Services.Helper;
		}
		public JobHistorySummaryModel GetJobHistory(JobHistoryRequest request)
		{
			try
			{
				// Determine if the user first has access to workspaces and object type
				IAuthenticationMgr authenticationManager = _helper.GetAuthenticationManager();
				var userArtifactId = authenticationManager.UserInfo.ArtifactID;
				var jobHistorySummary = new JobHistorySummaryModel();

				FieldValueList<Workspace> workspaces = GetWorkspacesUserHasPermissionToView(userArtifactId);
				if (!workspaces.Any())
				{
					return jobHistorySummary;
				}

				IList<JobHistoryModel> jobHistories = new List<JobHistoryModel>(request.PageSize);
				IRSAPIService service = new RSAPIService(_helper, request.WorkspaceArtifactId);

				var queryResult = service.JobHistoryLibrary
					.Query(new Query<RDO>() { ArtifactTypeGuid = new Guid(_JOB_HISTORY_OBJECT_TYPE_GUID), Fields = FieldValue.AllFields })
					.Where(s => string.Equals(s.JobStatus.Name, "Completed", StringComparison.InvariantCultureIgnoreCase) 
					            || string.Equals(s.JobStatus.Name,"Completed With Errors", StringComparison.InvariantCultureIgnoreCase));
				
				queryResult = SortJobHistories(request, queryResult);

				var start = request.Page * request.PageSize;
				var end = start + request.PageSize;

				var totalAvailable = 0;
				var totalDocuments = 0;

				foreach (var res in queryResult)
				{
					var userHasPermission = DoesUserHavePermissionToThisDestinationWorkspace(workspaces, res.DestinationWorkspace);
					if (!userHasPermission)
					{
						continue;
					}

					if ((totalAvailable >= start) && (totalAvailable < end))
					{
						var jobHistory = new JobHistoryModel
						{
							ItemsTransferred = res.ItemsTransferred ?? 0,
							EndTimeUTC = res.EndTimeUTC.GetValueOrDefault(),
							DestinationWorkspace = res.DestinationWorkspace
						};
						jobHistories.Add(jobHistory);
					}

					totalDocuments += res.ItemsTransferred ?? 0;
					totalAvailable++;
				}

				jobHistorySummary.Data = jobHistories.ToArray();
				jobHistorySummary.TotalAvailable = totalAvailable;
				jobHistorySummary.TotalDocumentsPushed = totalDocuments;

				return jobHistorySummary;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{0}.{1}", nameof(JobHistoryManager), nameof(GetJobHistory));
				throw;
			}
		}
		
		private IEnumerable<JobHistory> SortJobHistories(JobHistoryRequest request, IEnumerable<JobHistory> queryResult)
		{
			bool sortDescending = request.SortDescending ?? false;
			string sortColumn = GetSortColumn(request.SortColumnName);

			queryResult = sortDescending
				? queryResult.OrderByDescending(x => x.GetType().GetProperty(sortColumn).GetValue(x, null))
				: queryResult.OrderBy(x => x.GetType().GetProperty(sortColumn).GetValue(x, null));

			return queryResult;
		}
		
		private string GetSortColumn(string sortColumnName)
		{
			string sortColumn = string.IsNullOrEmpty(sortColumnName)
				? nameof(JobHistoryModel.DestinationWorkspace)
				: sortColumnName;

			return sortColumn;
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
	}
}