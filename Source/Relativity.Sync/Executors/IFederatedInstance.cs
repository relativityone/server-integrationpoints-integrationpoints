using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface IFederatedInstance
    {
        Task<int> GetInstanceIdAsync();
        Task<string> GetInstanceNameAsync();
    }
}