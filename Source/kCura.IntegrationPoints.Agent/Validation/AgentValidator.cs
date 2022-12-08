using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.Validation
{
    public class AgentValidator : IAgentValidator
    {
        #region Fields

        private readonly IValidationExecutor _validationExecutor;
        private readonly ICaseServiceContext _caseServiceContext;

        #endregion //Fields

        #region Constructors

        public AgentValidator(IValidationExecutor validationExecutor, ICaseServiceContext caseServiceContext)
        {
            _validationExecutor = validationExecutor;
            _caseServiceContext = caseServiceContext;
        }

        #endregion //Constructors

        #region Methods

        public void Validate(IntegrationPointDto dto, int submittedByUserId)
        {
            var sourceProvider = _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(dto.SourceProvider);
            var destinationProvider = _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<DestinationProvider>(dto.DestinationProvider);

            IntegrationPointType integrationPointType =
                _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<IntegrationPointType>(dto.Type);

            var context = new ValidationContext
            {
                SourceProvider = sourceProvider,
                DestinationProvider = destinationProvider,
                IntegrationPointType = integrationPointType,
                Model = dto,
                ObjectTypeGuid = ObjectTypeGuids.IntegrationPointGuid,
                UserId = submittedByUserId
            };

            _validationExecutor.ValidateOnRun(context);
        }

        #endregion //Methods
    }
}
