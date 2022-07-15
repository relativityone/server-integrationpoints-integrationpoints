using System.Threading.Tasks;
using Relativity.Storage;
using Relativity.Storage.Extensions.Models;

namespace Relativity.Sync.Transfer.ADF
{
	public interface IHelperFactory
	{
		Task<StorageEndpoint[]> GetStorageEndpointsAsync(ApplicationDetails applicationDetails);
	}
}