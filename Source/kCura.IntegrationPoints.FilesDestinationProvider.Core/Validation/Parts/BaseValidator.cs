using System;
using System.Linq;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public abstract class BaseValidator<T> : IValidator
    {
        // inherited validators should not be reachable outside of this assembly
        public string Key => String.Empty;

        public ValidationResult Validate(object value)
        {
            return Validate((T)value);
        }

        public abstract ValidationResult Validate(T value);
    }
}