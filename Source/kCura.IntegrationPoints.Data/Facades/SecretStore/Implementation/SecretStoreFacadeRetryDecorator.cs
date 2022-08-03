using System.Threading.Tasks;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation
{
    internal class SecretStoreFacadeRetryDecorator : ISecretStoreFacade
    {
        private const ushort _MAX_NUMBER_OF_RETRIES = 3;
        private const ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC = 3;

        private readonly IRetryHandler _retryHandler;
        private readonly ISecretStoreFacade _secretStore;

        public SecretStoreFacadeRetryDecorator(
            ISecretStoreFacade secretStore, 
            IRetryHandlerFactory retryHandlerFactory)
        {
            _secretStore = secretStore;
            _retryHandler = retryHandlerFactory.Create(
                _MAX_NUMBER_OF_RETRIES, 
                _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC
            );
        }

        public Task<Secret> GetAsync(string path)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _secretStore.GetAsync(path)
            );
        }

        public Task SetAsync(string path, Secret secret)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _secretStore.SetAsync(path, secret)
            );
        }

        public Task DeleteAsync(string path)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _secretStore.DeleteAsync(path)
            );
        }
    }
}
