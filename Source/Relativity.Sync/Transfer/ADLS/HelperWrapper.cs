using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;

namespace Relativity.Sync.Transfer.ADLS
{
    internal class HelperWrapper : IHelperWrapper
    {
        private const string _ADLER_SIEBEN_PTCI_ID = "PTCI-2456712";
        private const string _RELATIVITY_SYNC_SERVICE_NAME = "relativity-sync";

        private readonly IHelper _helper;
        private readonly IAPILog _logger;
        private readonly ApplicationDetails _applicationDetails;

        public HelperWrapper(IHelper helper)
        {
            _helper = helper;
            _logger = helper.GetLoggerFactory().GetLogger();
            _applicationDetails = new ApplicationDetails(_ADLER_SIEBEN_PTCI_ID, _RELATIVITY_SYNC_SERVICE_NAME);
        }

        public IAPILog GetLogger() => _logger;

        public async Task<StorageEndpoint[]> GetStorageEndpointsAsync(CancellationToken cancellationToken = default)
        {
            StorageEndpoint[] storageEndpoints = await _helper.GetStorageEndpointsAsync(_applicationDetails, cancellationToken).ConfigureAwait(false);
            if (storageEndpoints == null || storageEndpoints.Length < 1)
            {
                string message = "Storage Endpoints for current Tenant not found. Please check if tenant is fully migrated to ADLS.";
                _logger.LogError(message);
                throw new NotFoundException(message);
            }

            return storageEndpoints;
        }

        public async Task<IStorageAccess<string>> GetStorageAccessorAsync(CancellationToken cancellationToken)
        {
            IStorageAccess<string> storageAccessor = await _helper.GetStorageAccessorAsync(StorageAccessPermissions.GenericReadWrite, _applicationDetails, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (storageAccessor == null)
            {
                string message = "Storage Accessor not found.";
                _logger.LogError(message);
                throw new NotFoundException(message);
            }

            return storageAccessor;
        }
    }
}
