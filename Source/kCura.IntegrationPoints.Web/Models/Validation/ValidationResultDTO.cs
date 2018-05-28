using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Web.Models.Validation
{
	public class ValidationResultDTO
	{
		public List<ValidationErrorDTO> Errors { get; }
		public bool IsValid => !Errors.Any();

		public ValidationResultDTO(List<ValidationErrorDTO> errors)
		{
			Errors = errors;
		}
	}
}