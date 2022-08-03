using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
    public abstract class BasePartsValidator<T> : IValidator
    {
        public virtual string Key => String.Empty;

        public ValidationResult Validate(object value)
        {
            return Validate((T)value);
        }

        public abstract ValidationResult Validate(T value);
    }
}