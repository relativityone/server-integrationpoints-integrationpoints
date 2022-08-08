using System.Threading.Tasks;
using Relativity.Storage;
using Relativity.Storage.Extensions.Models;

namespace Relativity.Sync.Transfer.ADF
{
    internal interface IHelperWrapper
    {    
        Task<StorageEndpoint[]> GetStorageEndpointsAsync(ApplicationDetails applicationDetails);
    }
}