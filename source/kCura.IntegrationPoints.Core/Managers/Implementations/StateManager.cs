using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class StateManager : IStateManager
	{
		private readonly IQueueRepository _queueRepository;

		internal StateManager(IRepositoryFactory repositoryFactory)
		{
			_queueRepository = repositoryFactory.GetQueueRepository();
		}

		public ButtonStateDTO GetButtonState(int workspaceId, int integrationPointId, bool permissionSuccess, bool hasErrors)
		{
			bool hasJobsRunning = _queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceId,integrationPointId) > 0;
			return new ButtonStateDTO()
			{
				RunNowButtonEnabled = !hasJobsRunning && permissionSuccess,
				RetryErrorsButtonEnabled = !hasJobsRunning && permissionSuccess && hasErrors,
				ViewErrorsLinkEnabled = !hasJobsRunning && hasErrors,
			};
		}
	}
}
