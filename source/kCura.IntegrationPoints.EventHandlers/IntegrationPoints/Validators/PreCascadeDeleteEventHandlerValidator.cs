using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators
{
	public class PreCascadeDeleteEventHandlerValidator : IPreCascadeDeleteEventHandlerValidator
	{
		#region Fields

		private readonly IQueueManager _queueManager;
		private readonly IRepositoryFactory _repositoryFactory;

		#endregion Fields

		#region Constructors

		public PreCascadeDeleteEventHandlerValidator(IQueueManager queueManager, IRepositoryFactory repositoryFactory)
		{
			_queueManager = queueManager;
			_repositoryFactory = repositoryFactory;
		}

		#endregion //Constructors

		#region Methods

		public void Validate(int wkspId, int integrationPointId)
		{
			if (_queueManager.HasJobsExecutingOrInQueue(wkspId, integrationPointId))
			{
				IntegrationPointDTO ipDto = _repositoryFactory.GetIntegrationPointRepository(wkspId).Read(integrationPointId);
				throw new Exception($"Intgration Point '{ipDto.Name}' can not be deleted as the associated agent job has been already started!");
			}
		}

		#endregion //Methods
	}
}
