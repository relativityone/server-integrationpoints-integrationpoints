using System.Threading.Tasks;
using Relativity.API;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;

namespace Relativity.Sync.Transfer.ADF
{
	internal class HelperWrapper : IHelperWrapper
	{
		private readonly IHelper _helper;
		
		public HelperWrapper(IHelper helper)
		{
			_helper = helper;
		}
		
		public Task<StorageEndpoint[]> GetStorageEndpointsAsync(ApplicationDetails applicationDetails)
		{
			return _helper.GetStorageEndpointsAsync(applicationDetails);
		}
	}
}