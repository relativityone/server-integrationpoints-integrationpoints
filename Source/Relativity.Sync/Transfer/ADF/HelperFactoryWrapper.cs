using System.Threading.Tasks;
using Relativity.API;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;

namespace Relativity.Sync.Transfer.ADF
{
	public class HelperFactoryWrapper : IHelperFactory
	{
		private readonly IHelper _helper;

		public HelperFactoryWrapper(IHelper helper)
		{
			_helper = helper;
		}
		public Task<StorageEndpoint[]> GetStorageEndpointsAsync(ApplicationDetails applicationDetails)
		{
			return _helper.GetStorageEndpointsAsync(applicationDetails);
		}
	}
}