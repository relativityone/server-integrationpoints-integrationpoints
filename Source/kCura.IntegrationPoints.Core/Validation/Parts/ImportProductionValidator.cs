using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
	public class ImportProductionValidator : BasePartsValidator<int>
	{
		private readonly IPermissionManager _permissionManager;
		private readonly IProductionManager _productionManager;
		private readonly int _workspaceArtifactId;
		private readonly int? _federatedInstanceArtifactId;
		private readonly string _federatedInstanceCredentials;
		public ImportProductionValidator(int workspaceArtifactId, IProductionManager productionManager, IPermissionManager permissionManager, int? federatedInstanceArtifactId, string credentials)
		{
			_federatedInstanceArtifactId = federatedInstanceArtifactId;
			_federatedInstanceCredentials = credentials ?? string.Empty;
			_workspaceArtifactId = workspaceArtifactId;
			_productionManager = productionManager;
			_permissionManager = permissionManager;
		}

		public override ValidationResult Validate(int productionId)
		{
			var result = new ValidationResult();
			result.Add(ValidateViewPermissionForProduction(productionId));
			if (result.IsValid)
			{
				result.Add(ValidateCreatePermissionForProductionSource(productionId));
			}
			return result;
		}

		private ValidationResult ValidateViewPermissionForProduction(int productionId)
		{
			var result = new ValidationResult();
			try
			{
				ProductionDTO production = _productionManager.GetProductionsForImport(_workspaceArtifactId, _federatedInstanceArtifactId, _federatedInstanceCredentials)
					.FirstOrDefault(x => x.ArtifactID.Equals(productionId.ToString()));

				if (production == null)
				{
					result.Add(ValidationMessages.MissingDestinationProductionPermissions);
				}
			}
			catch
			{
				result.Add(ValidationMessages.MissingDestinationProductionPermissions);
			}

			return result;
		}

		private ValidationResult ValidateCreatePermissionForProductionSource(int productionId)
		{
			var result = new ValidationResult();
			bool canAddSubfolders = _permissionManager.UserHasArtifactInstancePermission(_workspaceArtifactId, Constants.ObjectTypeArtifactTypesGuid.ProductionDataSource, productionId, ArtifactPermission.Create);
			if (!canAddSubfolders)
			{
				result.Add(ValidationMessages.MissingDestinationProductionPermissions);
			}
			return result;
		}
	}
}
