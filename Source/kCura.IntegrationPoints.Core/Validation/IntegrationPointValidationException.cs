using System;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation
{
    [Serializable]
    public class IntegrationPointValidationException : Exception
    {
        public IntegrationPointValidationException(ValidationResult validationResult) : base(validationResult.ToString())
        {
            ValidationResult = validationResult;
        }

        public ValidationResult ValidationResult { get; }
    }
}
