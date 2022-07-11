using System.Threading.Tasks;
using Relativity.Storage;

namespace Relativity.Sync.Transfer.ADF
{
	public interface IStorageAccessFactory
	{
		Task<IStorageDiscovery> CreateStorageDiscoveryAsync(
			string teamId,
			string serviceName = "");
	}
}