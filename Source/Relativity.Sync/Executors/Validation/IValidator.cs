using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.Validation
{
	internal interface IValidator
	{
		Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token);
	}
}