using System.Threading.Tasks;

namespace Relativity.Sync.Executors.FederatedInstance
{
	internal interface IFederatedInstance
	{
		Task<int> GetInstanceIdAsync();
		Task<string> GetInstanceNameAsync();
	}
}