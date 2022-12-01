using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Storage;

namespace Relativity.Sync.Transfer.ADLS
{
    internal interface IHelperWrapper
    {
        public IAPILog GetLogger();

        Task<StorageEndpoint[]> GetStorageEndpointsAsync(CancellationToken cancellationToken = default);

        Task<IStorageAccess<string>> GetStorageAccessorAsync(CancellationToken cancellationToken);
    }
}
