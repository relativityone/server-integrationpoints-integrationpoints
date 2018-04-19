using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.Validation
{
	public interface IAgentValidator
	{
		void Validate(IntegrationPoint integrationPointDto, int submittedByUserId);
	}

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

		public void Validate(IntegrationPoint integrationPoint, int submittedByUserId)
		{
			var sourceProvider = _caseServiceContext.RsapiService.RelativityObjectManager.Read<SourceProvider>(integrationPoint.SourceProvider.Value);
			var destinationProvider = _caseServiceContext.RsapiService.RelativityObjectManager.Read<DestinationProvider>(integrationPoint.DestinationProvider.Value);

			IntegrationPointModelBase model = IntegrationPointModel.FromIntegrationPoint(integrationPoint);

			IntegrationPointType integrationPointType =
				_caseServiceContext.RsapiService.RelativityObjectManager.Read<IntegrationPointType>(integrationPoint.Type.Value);

			ValidationContext context = new ValidationContext()
			{
				SourceProvider = sourceProvider,
				DestinationProvider = destinationProvider,
				IntegrationPointType = integrationPointType,
				Model = model,
				ObjectTypeGuid = ObjectTypeGuids.IntegrationPoint,
				UserId = submittedByUserId
			};

			_validationExecutor.ValidateOnRun(context);
		}

		#endregion //Methods
	}


}
