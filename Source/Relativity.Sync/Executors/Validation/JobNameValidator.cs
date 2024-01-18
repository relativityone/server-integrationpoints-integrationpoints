using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
    internal sealed class JobNameValidator : IValidator
    {
        private const string _ERROR_JOB_NAME_EMPTY = "Job name cannot be empty.";
        private const string _ERROR_JOB_NAME_INVALID = "Job name contains one or more illegal characters (<>:\\\"\\\\\\/|\\?\\* TAB).";
        private readonly IAPILog _logger;

        private static readonly char[] _allForbiddenCharacters = System.IO.Path.GetInvalidFileNameChars();

        public JobNameValidator(IAPILog logger)
        {
            _logger = logger;
        }

        public Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
        {
            _logger.LogInformation("Validating job name.");

            ValidationResult result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(configuration.GetJobName()))
            {
                result.Add(_ERROR_JOB_NAME_EMPTY);
            }
            else
            {
                bool areForbiddenCharactersPresent = configuration.GetJobName().Any(c => _allForbiddenCharacters.Contains(c));
                if (areForbiddenCharactersPresent)
                {
                    result.Add(_ERROR_JOB_NAME_INVALID);
                }
            }

            return Task.FromResult(result);
        }

        public bool ShouldValidate(ISyncPipeline pipeline) => true;
    }
}
