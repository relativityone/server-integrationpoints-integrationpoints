using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators
{
	public class PreCascadeDeleteEventHandlerValidator : IPreCascadeDeleteEventHandlerValidator
	{
		#region Fields

		private readonly IQueueRepository _queueRepository;
		private readonly IRSAPIServiceFactory _rsapiServiceFactory;

		#endregion Fields

		#region Constructors

		public PreCascadeDeleteEventHandlerValidator(IQueueRepository queueRepository, IRSAPIServiceFactory rsapiServiceFactory)
		{
			_queueRepository = queueRepository;
			_rsapiServiceFactory = rsapiServiceFactory;
		}

		#endregion //Constructors

		#region Methods

		public void Validate(int workspaceId, int integrationPointId)
		{
			if (_queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceId, integrationPointId) > 0)
			{
				IntegrationPoint integrationPoint = _rsapiServiceFactory.Create(workspaceId).RelativityObjectManager.Read<IntegrationPoint>(integrationPointId);
				throw new Exception($"Integration Point '{integrationPoint.Name}' can not be deleted as the associated agent job has been already started!");
			}
		}

		#endregion //Methods
	}
}