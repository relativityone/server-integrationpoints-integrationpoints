using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal sealed class FederatedInstance : IFederatedInstance
    {
        public Task<int> GetInstanceIdAsync()
        {
            return Task.FromResult(-1);
        }

        public Task<string> GetInstanceNameAsync()
        {
            return Task.FromResult("This Instance");
        }
    }
}