using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class JobNameValidator : IValidator
	{
		private const string _ERROR_JOB_NAME_EMPTY = "Job name cannot be empty.";
		private const string _ERROR_JOB_NAME_INVALID = "Job name contains one or more illegal characters (<>:\\\"\\\\\\/|\\?\\* TAB).";
		private readonly ISyncLog _logger;

		private static readonly char[] _allForbiddenCharacters = System.IO.Path.GetInvalidFileNameChars();

		public JobNameValidator(ISyncLog logger)
		{
			_logger = logger;
		}

		public Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating job name: {jobName}", configuration.JobName);

			ValidationResult result = new ValidationResult();

			if (string.IsNullOrWhiteSpace(configuration.JobName))
			{
				result.Add(_ERROR_JOB_NAME_EMPTY);
			}
			else
			{
				bool areForbiddenCharactersPresent = configuration.JobName.Any(c => _allForbiddenCharacters.Contains(c));
				if (areForbiddenCharactersPresent)
				{
					result.Add(_ERROR_JOB_NAME_INVALID);
				}
			}

			return Task.FromResult(result);
		}
	}
}