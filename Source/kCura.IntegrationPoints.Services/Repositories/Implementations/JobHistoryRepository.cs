using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Services.JobHistory;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class JobHistoryRepository : IJobHistoryRepository
	{
		private readonly IHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IRelativityIntegrationPointsRepository _relativityIntegrationPointsRepository;
		private readonly ICompletedJobsHistoryRepository _completedJobsHistoryRepository;
		private readonly IManagerFactory _managerFactory;
		private readonly IJobHistoryAccess _jobHistoryAccess;
		private readonly IJobHistorySummaryModelBuilder _summaryModelBuilder;
		private readonly IDestinationParser _destinationParser;
		private readonly IContextContainerFactory _contextContainerFactory;

		public JobHistoryRepository(IHelper helper, IHelperFactory helperFactory, IRelativityIntegrationPointsRepository relativityIntegrationPointsRepository,
			ICompletedJobsHistoryRepository completedJobsHistoryRepository, IManagerFactory managerFactory, IContextContainerFactory contextContainerFactory,
			IJobHistoryAccess jobHistoryAccess, IJobHistorySummaryModelBuilder summaryModelBuilder, IDestinationParser destinationParser)
		{
			_helper = helper;
			_helperFactory = helperFactory;
			_relativityIntegrationPointsRepository = relativityIntegrationPointsRepository;
			_completedJobsHistoryRepository = completedJobsHistoryRepository;
			_managerFactory = managerFactory;
			_contextContainerFactory = contextContainerFactory;
			_jobHistoryAccess = jobHistoryAccess;
			_summaryModelBuilder = summaryModelBuilder;
			_destinationParser = destinationParser;
		}

		public JobHistorySummaryModel GetJobHistory(JobHistoryRequest request)
		{
			List<Core.Models.IntegrationPointModel> integrationPoints = _relativityIntegrationPointsRepository.RetrieveIntegrationPoints(request.WorkspaceArtifactId);

			var allCompletedJobs = new List<JobHistoryModel>();
			var workspacesWithAccess = new Dictionary<int, IList<int>>();

			foreach (var integrationPoint in integrationPoints)
			{
				IList<JobHistoryModel> queryResult = _completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoint(request, integrationPoint.ArtifactID);

				IEnumerable<int> instanceIds = queryResult.Select(qr =>
				{
					if (qr.DestinationInstance == FederatedInstanceManager.LocalInstance.Name)
					{
						return -1;
					}
					return _destinationParser.GetArtifactId(qr.DestinationInstance);
				}).Distinct();

				foreach (int instanceId in instanceIds)
				{
					if (!workspacesWithAccess.ContainsKey(instanceId))
					{
						IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, instanceId == -1 ? null : (int?) instanceId,
							integrationPoint.SecuredConfiguration);

						IWorkspaceManager workspaceManager =
							_managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(
								_helper, targetHelper.GetServicesManager()));

						IList<int> userWorkspaces = workspaceManager.GetUserWorkspaces().Select(w => w.ArtifactId).ToList();

						workspacesWithAccess.Add(instanceId, userWorkspaces);
					}
				}

				allCompletedJobs.AddRange(queryResult);
			}

			IList<JobHistoryModel> jobHistories = _jobHistoryAccess.Filter(allCompletedJobs, workspacesWithAccess);

			IList<JobHistoryModel> orderedJobHistories = SortJobHistories(jobHistories, request);

			return _summaryModelBuilder.Create(request.Page, request.PageSize, orderedJobHistories);
		}

		private IList<JobHistoryModel> SortJobHistories(IList<JobHistoryModel> jobHistories, JobHistoryRequest request)
		{
			var sortColumnName = request.SortColumnName ?? nameof(JobHistoryModel.DestinationWorkspace);
			PropertyInfo prop = typeof(JobHistoryModel).GetProperty(sortColumnName);

			IEnumerable<JobHistoryModel> orderedJobHistories = null;
			if (request.SortDescending.HasValue && request.SortDescending == true)
			{
				orderedJobHistories = jobHistories.OrderByDescending(x => prop.GetValue(x, null));
			}
			else
			{
				orderedJobHistories = jobHistories.OrderBy(x => prop.GetValue(x, null));
			}
			return orderedJobHistories.ToList();
		}
	}
}