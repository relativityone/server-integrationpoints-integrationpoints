using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.Validation.PreValidation
{
	internal interface IPreValidator
	{
		Task<ValidationResult> ValidateAsync(IPreValidationConfiguration configuration, CancellationToken token);
	}
}
