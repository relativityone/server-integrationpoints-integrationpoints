using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.LibraryApplication;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Pipelines
{
    internal class IAPIv2RunChecker : IIAPIv2RunChecker
    {
        private static readonly Guid ImportAppGuid = new Guid("21f65fdc-3016-4f2b-9698-de151a6186a2");

        private readonly ISyncToggles _toggles;
        private readonly IIAPIv2RunCheckerConfiguration _configuration;
        private readonly IFieldMappings _fieldMappings;
        private readonly IObjectFieldTypeRepository _objectFieldTypeRepository;
        private readonly IDestinationServiceFactoryForAdmin _serviceFactory;
        private readonly IAPILog _logger;

        private bool? _shouldBeUsed;

        public IAPIv2RunChecker(
            IIAPIv2RunCheckerConfiguration configuration,
            ISyncToggles toggles,
            IFieldMappings fieldMappings,
            IObjectFieldTypeRepository objectFieldTypeRepository,
            IDestinationServiceFactoryForAdmin serviceFactory,
            IAPILog logger)
        {
            _toggles = toggles;
            _configuration = configuration;
            _fieldMappings = fieldMappings;
            _objectFieldTypeRepository = objectFieldTypeRepository;
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public bool ShouldBeUsed()
        {
            if (!_shouldBeUsed.HasValue)
            {
                _shouldBeUsed = CheckConditionsAsync().GetAwaiter().GetResult();
            }

            return _shouldBeUsed.Value;
        }

        private async Task<bool> CheckConditionsAsync()
        {
            try
            {
                bool result = _toggles.IsEnabled<EnableIAPIv2Toggle>()
                                                && _configuration.RdoArtifactTypeId == (int)ArtifactType.Document
                                                && _configuration.NativeBehavior.IsIn(ImportNativeFileCopyMode.SetFileLinks, ImportNativeFileCopyMode.DoNotImportNativeFiles)
                                                && !_configuration.IsRetried
                                                && !_configuration.IsDrainStopped
                                                && !_configuration.ImageImport
                                                && !_configuration.EnableTagging
                                                && await IsImportInstalledInDestinationWorkspaceAsync().ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while checking if IAPI 2.0 should be used");
                throw;
            }
        }

        private async Task<bool> IsImportInstalledInDestinationWorkspaceAsync()
        {
            try
            {
                using (IApplicationInstallManager appManager = await _serviceFactory.CreateProxyAsync<IApplicationInstallManager>().ConfigureAwait(false))
                {
                    GetInstallStatusResponse status = await appManager.GetStatusAsync(_configuration.DestinationWorkspaceArtifactId, ImportAppGuid).ConfigureAwait(false);
                    bool isInstalled = status.InstallStatus.Code == InstallStatusCode.Completed;
                    return isInstalled;
                }
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex, "Import application is not installed in destination workspace Artifact ID: {destinationWorkspaceArtifactID}", _configuration.DestinationWorkspaceArtifactId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if Import application is installed in destination workspace Artifact ID: {destinationWorkspaceArtifactID}", _configuration.DestinationWorkspaceArtifactId);
                return false;
            }
        }
    }
}
