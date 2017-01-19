using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
	public class ProductionValidator : BasePartsValidator<ExportSettings>
	{
		private readonly IProductionService _productionService;

		public ProductionValidator(IProductionService productionService)
		{
			_productionService = productionService;
		}

		public override ValidationResult Validate(ExportSettings value)
		{
			var result = new ValidationResult();

			var production = _productionService.GetProductionsForExport(value.WorkspaceId)
				.FirstOrDefault(x => x.ArtifactID.Equals(value.ProductionId.ToString()));

			if (production == null)
			{
				result.Add(FileDestinationProviderValidationMessages.PRODUCTION_NOT_EXIST);
			}

			return result;
		}
	}
}