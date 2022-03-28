using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Web.Models.Validation
{
	internal class ValidationResultMapper : IValidationResultMapper
	{
		private const string HELP_PAGE_URL =
			"https://help.relativity.com/RelativityOne/Content/Relativity/Integration_Points/Relativity_Integration_Points.htm";

		public ValidationResultDTO Map(ValidationResult validationResult)
		{
			List<ValidationErrorDTO> errors = validationResult.Messages.Select(MapValidationError).ToList();
			return new ValidationResultDTO(errors);
		}

		private ValidationErrorDTO MapValidationError(ValidationMessage message)
		{
			return new ValidationErrorDTO(message.ErrorCode, message.ShortMessage, HELP_PAGE_URL);
		}
	}
}