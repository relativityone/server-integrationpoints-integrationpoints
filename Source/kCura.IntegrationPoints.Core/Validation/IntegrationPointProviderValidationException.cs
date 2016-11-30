using System;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation
{
	public class IntegrationPointProviderValidationException : Exception
	{
		private const string _MESSAGE = "Integration Points provider validation failed, please review result property for the details.";

		public ValidationResult Result { get; private set; }

		public IntegrationPointProviderValidationException(ValidationResult result) : base(_MESSAGE)
		{
			Result = result;
		}
	}
}