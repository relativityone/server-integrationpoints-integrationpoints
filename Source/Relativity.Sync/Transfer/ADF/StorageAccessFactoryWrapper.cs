using System.Threading.Tasks;
using Relativity.Storage;

namespace Relativity.Sync.Transfer.ADF
{
	internal sealed class StorageAccessFactoryWrapper : IStorageAccessFactory
	{
		public Task<IStorageDiscovery> CreateStorageDiscoveryAsync(string teamId, string serviceName = "")
		{
			return StorageAccessFactory.CreateStorageDiscoveryAsync(teamId, serviceName);
		}
	}
}