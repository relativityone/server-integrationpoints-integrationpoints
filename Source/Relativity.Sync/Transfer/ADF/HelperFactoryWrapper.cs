using System.Threading.Tasks;
using Relativity.API;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;

namespace Relativity.Sync.Transfer.ADF
{
	/// <summary>
	/// FactorWrapper for Relativity.Helper, created to enable testing and mocking
	/// </summary>
	public class HelperFactoryWrapper : IHelperFactory
	{
		private readonly IHelper _helper;

		/// <summary>
		/// Ctor for HelperFactoryWrapper, that DI IHelper from container
		/// </summary>
		/// <param name="helper"></param>
		public HelperFactoryWrapper(IHelper helper)
		{
			_helper = helper;
		}
		/// <summary>
		/// This method return Bedrock storage endpoints
		/// </summary>
		/// <param name="applicationDetails"></param>
		/// <returns></returns>
		public Task<StorageEndpoint[]> GetStorageEndpointsAsync(ApplicationDetails applicationDetails)
		{
			return _helper.GetStorageEndpointsAsync(applicationDetails);
		}
	}
}