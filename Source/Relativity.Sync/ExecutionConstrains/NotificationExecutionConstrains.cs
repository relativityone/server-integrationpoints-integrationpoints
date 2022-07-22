using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class NotificationExecutionConstrains : IExecutionConstrains<INotificationConfiguration>
    {
        public Task<bool> CanExecuteAsync(INotificationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(configuration.SendEmails);
        }
    }
}