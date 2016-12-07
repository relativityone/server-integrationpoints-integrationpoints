using kCura.Relativity.Client;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class TransferredObjectValidator : BasePartsValidator<int>
	{
		public override ValidationResult Validate(int value)
		{
			var result = new ValidationResult();

			if (value != (int) ArtifactType.Document)
			{
				result.Add($"{RelativityProviderValidationMessages.TRANSFERRED_OBJECT_INVALIDA_TYPE} {value}");
			}

			return result;
		}
		
	}
}
