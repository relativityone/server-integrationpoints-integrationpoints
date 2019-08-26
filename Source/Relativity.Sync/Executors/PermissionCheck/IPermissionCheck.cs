using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Executors.PermissionCheck
{
	internal interface IPermissionCheck
	{
		Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration);
	}
}