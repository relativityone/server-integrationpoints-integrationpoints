using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.Web.Models.Validation
{
	[DataContract]
	public class ValidationResultDTO
	{
		[DataMember]
		public List<ValidationErrorDTO> Errors { get; } = new List<ValidationErrorDTO>();

		public bool IsValid => !Errors.Any();

		public ValidationResultDTO()
		{

		}

		public ValidationResultDTO(List<ValidationErrorDTO> errors)
		{
			Errors = errors ?? new List<ValidationErrorDTO>();
		}
	}
}