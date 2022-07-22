using System.Threading.Tasks;
using Refit;

namespace Relativity.Sync.Dashboards.Api
{
    [Headers("Content-Type: application/json")]
    public interface ISplunkApi
    {
        [Delete("/servicesNS/nobody/rel_splunk_engineering/storage/collections/data/{collectionName}?output_mode=json")]
        Task ClearKVStoreCollectionAsync(string collectionName);

        [Post("/servicesNS/nobody/rel_splunk_engineering/storage/collections/data/{collectionName}?output_mode=json")]
        Task AddToKVStoreCollectionAsync(string collectionName, [Body] SplunkKVCollectionItem item);
    }
}