using System.Threading.Tasks;
using Relativity.Storage;
using Relativity.Storage.Extensions.Models;

namespace Relativity.Sync.Transfer.ADF
{
	/// <summary>
	/// Use to mock IHelperFactory
	/// </summary>
	public interface IHelperFactory
	{
		/// <summary>
		/// Method for mocking endpoints returned from Bedrock
		/// </summary>
		/// <param name="applicationDetails"></param>
		/// <returns></returns>
		Task<StorageEndpoint[]> GetStorageEndpointsAsync(ApplicationDetails applicationDetails);
	}
}