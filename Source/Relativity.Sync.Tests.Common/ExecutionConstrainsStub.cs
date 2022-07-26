using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common
{
    [ExcludeFromCodeCoverage]
    internal sealed class ExecutionConstrainsStub<T> : IExecutionConstrains<T> where T : IConfiguration
    {
        public Task<bool> CanExecuteAsync(T configuration, CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}