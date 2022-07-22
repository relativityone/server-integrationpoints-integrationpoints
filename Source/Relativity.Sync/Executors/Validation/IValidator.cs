using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
    internal interface IValidator
    {
        Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token);
        bool ShouldValidate(ISyncPipeline pipeline);
    }
}