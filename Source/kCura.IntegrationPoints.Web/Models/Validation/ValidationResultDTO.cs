using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.Web.Models.Validation
{
	[DataContract]
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