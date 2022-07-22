using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync
{
    internal interface IExecutor<in T> where T : IConfiguration
    {
        Task<ExecutionResult> ExecuteAsync(T configuration, CompositeCancellationToken token);
    }
}