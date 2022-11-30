using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.PermissionCheck
{
    internal interface IPermissionCheck
    {
        Task<ValidationResult> ValidateAsync(IPermissionsCheckConfiguration configuration);

        bool ShouldValidate(ISyncPipeline pipeline);
    }
}
