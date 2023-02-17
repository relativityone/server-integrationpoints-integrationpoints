using System;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators
{
    public class PreCascadeDeleteEventHandlerValidator : IPreCascadeDeleteEventHandlerValidator
    {
        #region Fields

        private readonly IQueueRepository _queueRepository;
        private readonly IIntegrationPointRepository _integrationPointRepository;

        #endregion Fields

        #region Constructors

        public PreCascadeDeleteEventHandlerValidator(IQueueRepository queueRepository, IIntegrationPointRepository integrationPointRepository)
        {
            _queueRepository = queueRepository;
            _integrationPointRepository = integrationPointRepository;
        }

        #endregion // Constructors

        #region Methods

        public void Validate(int workspaceId, int integrationPointId)
        {
            if (_queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceId, integrationPointId) > 0)
            {
                string integrationPointName = _integrationPointRepository.GetName(integrationPointId);
                throw new Exception($"Integration Point '{integrationPointName}' can not be deleted as the associated agent job has been already started!");
            }
        }

        #endregion // Methods
    }
}
