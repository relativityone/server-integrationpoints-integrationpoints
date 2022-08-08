using System;
using System.Threading.Tasks;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation
{
    internal class SecretStoreFacade : ISecretStoreFacade
    {
        private readonly Lazy<ISecretStore> _secretStore;

        public SecretStoreFacade(Lazy<ISecretStore> secretStore)
        {
            _secretStore = secretStore;
        }

        public Task<Secret> GetAsync(string path)
        {
            return _secretStore.Value.GetAsync(path);
        }

        public Task SetAsync(string path, Secret secret)
        {
            return _secretStore.Value.SetAsync(path, secret);
        }

        public Task DeleteAsync(string path)
        {
            return _secretStore.Value.DeleteAsync(path);
        }
    }
}
