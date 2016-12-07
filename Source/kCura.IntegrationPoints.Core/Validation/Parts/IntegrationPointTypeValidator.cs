

using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class IntegrationPointTypeValidator : IValidator
	{
		public string Key => Constants.IntegrationPointProfiles.Validation.INTEGRATION_POINT_TYPE;

		private readonly ICaseServiceContext _context;

		public IntegrationPointTypeValidator(ICaseServiceContext context)
		{
			_context = context;
		}

		public ValidationResult Validate(object value)
		{
			var integrationModel = value as IntegrationPointProviderValidationModel;
			var result = new ValidationResult();
			
			IntegrationPointType integrationPointType = _context.RsapiService.IntegrationPointTypeLibrary.Read(integrationModel.Type);

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
