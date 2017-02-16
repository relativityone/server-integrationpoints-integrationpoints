using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Services.JobHistory;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Toggles;
using IWorkspaceManager = kCura.IntegrationPoints.Core.Managers.IWorkspaceManager;

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
		private readonly IDestinationWorkspaceParser _destinationWorkspaceParser;
		private readonly IContextContainerFactory _contextContainerFactory;

		public JobHistoryRepository(IHelper helper, IHelperFactory helperFactory, IRelativityIntegrationPointsRepository relativityIntegrationPointsRepository,
			ICompletedJobsHistoryRepository completedJobsHistoryRepository, IManagerFactory managerFactory, IContextContainerFactory contextContainerFactory, 
			IJobHistoryAccess jobHistoryAccess, IJobHistorySummaryModelBuilder summaryModelBuilder, IDestinationWorkspaceParser destinationWorkspaceParser)
		{
			_helper = helper;
			_helperFactory = helperFactory;
			_relativityIntegrationPointsRepository = relativityIntegrationPointsRepository;
			_completedJobsHistoryRepository = completedJobsHistoryRepository;
			_managerFactory = managerFactory;
			_contextContainerFactory = contextContainerFactory;
			_jobHistoryAccess = jobHistoryAccess;
			_summaryModelBuilder = summaryModelBuilder;
			_destinationWorkspaceParser = destinationWorkspaceParser;
		}

		public JobHistorySummaryModel GetJobHistory(JobHistoryRequest request)
		{
			List<Core.Models.IntegrationPointModel> integrationPoints = _relativityIntegrationPointsRepository.RetrieveIntegrationPoints();

			var allCompletedJobs = new List<JobHistoryModel>();
			var workspacesWithAccess = new Dictionary<string, IList<int>>();

			foreach (var integrationPoint in integrationPoints)
			{
				IList<JobHistoryModel> queryResult = _completedJobsHistoryRepository.RetrieveCompleteJobsForIntegrationPoint(request, integrationPoint.ArtifactID);

				IEnumerable<string> instanceNames = queryResult.Select(qr => _destinationWorkspaceParser.GetInstanceName(qr.DestinationWorkspace)).Distinct();

				foreach (string instanceName in instanceNames)
				{
					if (!workspacesWithAccess.ContainsKey(instanceName))
					{
						var federatedInstanceManager =
							_managerFactory.CreateFederatedInstanceManager(_contextContainerFactory.CreateContextContainer(_helper));

						FederatedInstanceDto federatedInstance = federatedInstanceManager.RetrieveFederatedInstanceByName(instanceName);

						IHelper targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstance.ArtifactId,
							integrationPoint.SecuredConfiguration);

						IWorkspaceManager workspaceManager =
							_managerFactory.CreateWorkspaceManager(_contextContainerFactory.CreateContextContainer(
								_helper, targetHelper.GetServicesManager()));

						IList<int> userWorkspaces = workspaceManager.GetUserWorkspaces().Select(w => w.ArtifactId).ToList();

						workspacesWithAccess.Add(instanceName, userWorkspaces);
					}
				}

				allCompletedJobs.AddRange(queryResult);
			}

			IList<JobHistoryModel> jobHistories = _jobHistoryAccess.Filter(allCompletedJobs, workspacesWithAccess);

			return _summaryModelBuilder.Create(request.Page, request.PageSize, jobHistories);
		}
	}
}