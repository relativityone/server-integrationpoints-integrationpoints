using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class IntegrationPointTypeValidator : IValidator
	{
		private readonly IRelativityObjectManager _objectManager;
		public string Key => Constants.IntegrationPointProfiles.Validation.INTEGRATION_POINT_TYPE;

		public IntegrationPointTypeValidator(IRelativityObjectManager objectManager)
		{
			_objectManager = objectManager;
		}

		public ValidationResult Validate(object value)
		{
			var integrationModel = value as IntegrationPointProviderValidationModel;
			var result = new ValidationResult();

			IntegrationPointType integrationPointType = _objectManager.Read<IntegrationPointType>(integrationModel.Type);

			if (integrationPointType == null)
			{
				result.Add(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_TYPE_INVALID);
				return result;
			}

			if (integrationModel.SourceProviderIdentifier.ToUpper() == IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID.ToUpper())
			{
				if (integrationPointType.Identifier.ToUpper() !=
					Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString().ToUpper())
				{
					result.Add(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_TYPE_INVALID);
				}
			}
			else
			{
				if (integrationPointType.Identifier.ToUpper() !=
					Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString().ToUpper())
				{
					result.Add(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_TYPE_INVALID);
				}
			}

			return result;
		}
	}
}
