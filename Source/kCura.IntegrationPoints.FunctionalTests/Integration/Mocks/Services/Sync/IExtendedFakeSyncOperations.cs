using Relativity.Sync;
using System;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.Sync
{
    public interface IExtendedFakeSyncOperations
    {
        void SetupSyncJob(Func<CompositeCancellationToken, Task> action);
    }
}
