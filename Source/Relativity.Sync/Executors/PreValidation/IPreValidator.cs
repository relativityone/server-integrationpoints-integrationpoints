using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Executors.PreValidation
{
	internal interface IPreValidator
	{
		Task<ValidationResult> ValidateAsync(IPreValidationConfiguration configuration, CancellationToken token);
	}
}
