using LanguageExt;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Email.Extensions
{
    internal static class ValidationExtensions
    {
        public static Either<string, T> ToEither<T>(this Validation<string, T> validation, string errorHeader)
        {
            return validation
                .ToEither()
                .MapLeft(errors => BuildValidationError(errorHeader, errors));
        }

        private static string BuildValidationError(string errorHeader, IEnumerable<string> validationErrors)
        {
            string allErrors = string.Join(";", validationErrors);
            return $"{errorHeader}. Errors: {allErrors}";
        }
    }
}
