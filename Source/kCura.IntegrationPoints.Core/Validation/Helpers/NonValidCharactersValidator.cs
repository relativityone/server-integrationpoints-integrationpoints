using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Utility.Extensions;

namespace kCura.IntegrationPoints.Core.Validation.Helpers
{
    public class NonValidCharactersValidator : INonValidCharactersValidator
    {
        private static readonly char[] _allForbiddenCharacters = System.IO.Path.GetInvalidFileNameChars();

        public ValidationResult Validate(string name, string errorMessage)
        {
            var result = new ValidationResult();

            bool areForbiddenCharactersPresent = name.Any(c => c.In(_allForbiddenCharacters));
            if (areForbiddenCharactersPresent)
            {
                result.Add(errorMessage);
            }

            return result;
        }
    }
}
