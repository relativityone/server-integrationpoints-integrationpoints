using System;
using System.IO;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using SystemInterface.IO;

namespace kCura.IntegrationPoints.Core.Services
{
    public class DataTransferLocationService : IDataTransferLocationService
    {
        private const string _WORKSPACE_FOLDER_FORMAT = "EDDS{0}";
        private const string _EDDS_PARENT_FOLDER = "DataTransfer";
        private const string _INVALID_PATH_ERROR_MSG = "Given Destination Folder path is invalid!";

        private readonly IHelper _helper;
        private readonly IIntegrationPointTypeService _integrationPointTypeService;
        private readonly IDirectory _directoryService;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IAPILog _logger;

        public DataTransferLocationService(IHelper helper, IIntegrationPointTypeService integrationPointTypeService, IDirectory directoryService, ICryptographyHelper cryptographyHelper)
        {
            _helper = helper;
            _integrationPointTypeService = integrationPointTypeService;
            _directoryService = directoryService;
            _cryptographyHelper = cryptographyHelper;

            _logger = _helper.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationService>();
        }

        public void CreateForAllTypes(int workspaceArtifactId)
        {
            string rootPath = GetDestinationFolderRootPath(workspaceArtifactId);

            CreateDirectoryIfNotExists(rootPath);

            foreach (var type in _integrationPointTypeService.GetAllIntegrationPointTypes())
            {
                CreateDirectoryIfNotExists(Path.Combine(rootPath, type.Name));
            }
        }

        public string GetDefaultRelativeLocationFor(Guid integrationPointTypeIdentifier)
        {
            IntegrationPointType type = _integrationPointTypeService.GetIntegrationPointType(integrationPointTypeIdentifier);

            return Path.Combine(_EDDS_PARENT_FOLDER, type.Name);
        }

        public bool IsEddsPath(string path)
        {
            return path.StartsWith(_EDDS_PARENT_FOLDER);
        }

        public string VerifyAndPrepare(int workspaceArtifactId, string path, Guid providerType)
        {
            string providerTypeRelativePathPrefix = GetDefaultRelativeLocationFor(providerType);

            // First validate if provided path match the correct destnation folder on the server (eg: DataTransfer\Export)
            if (!path.StartsWith(providerTypeRelativePathPrefix))
            {
                throw new ArgumentException($@"Provided realtive path '{path}' does not match the correct destination folder!", path);
            }

            string fileShareRootLocation = GetWorkspaceFileLocationRootPath(workspaceArtifactId);
            string fileShareRootLocationWithRelativePath = Path.Combine(fileShareRootLocation, providerTypeRelativePathPrefix);

            // Get physical path for destination folder eg: \\localhost\FileShare\EDDS123456\Export\SomeFolder
            string destinationFolderPhysicalPath = Path.Combine(fileShareRootLocation, path);

            if (!destinationFolderPhysicalPath.IsSubPathOf(fileShareRootLocationWithRelativePath))
            {
                throw new ArgumentException(_INVALID_PATH_ERROR_MSG, path);
            }

            CreateDirectoryIfNotExists(destinationFolderPhysicalPath);
            return destinationFolderPhysicalPath;
        }

        public string GetWorkspaceFileLocationRootPath(int workspaceArtifactId)
        {
            try
            {
                using (IWorkspaceManager workspaceManager = _helper.GetServicesManager().CreateProxy<IWorkspaceManager>(ExecutionIdentity.System))
                {
                    WorkspaceRef workspace = new WorkspaceRef { ArtifactID = workspaceArtifactId };

                    FileShareResourceServer fileShare = workspaceManager.GetDefaultWorkspaceFileShareResourceServerAsync(workspace).GetAwaiter().GetResult();
                    return Path.Combine(fileShare.UNCPath, string.Format(_WORKSPACE_FOLDER_FORMAT, workspaceArtifactId));
                }
            }
            catch (NotFoundException ex)
            {
                LogMissingWorkspaceError(ex, workspaceArtifactId);
                throw;
            }
        }

        private string GetDestinationFolderRootPath(int workspaceArtifactId)
        {
            string workspaceFileLocation = GetWorkspaceFileLocationRootPath(workspaceArtifactId);

            return Path.Combine(workspaceFileLocation, _EDDS_PARENT_FOLDER);
        }

        private void CreateDirectoryIfNotExists(string path)
        {
            if (!_directoryService.Exists(path))
            {
                LogMissingDirectoryCreation(path);

                try
                {
                    _directoryService.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create directory: {path}", path);
                    throw new IOException($"Failed to create directory: {path}");
                }
            }
        }

        private void LogMissingWorkspaceError(NotFoundException ex, int workspaceId)
        {
            _logger.LogError(ex, "Cannot find workspace with artifact id: {WorkspaceId}.", workspaceId);
        }

        private void LogMissingDirectoryCreation(string path)
        {
            _logger.LogInformation("Creating missing directory: {path}", _cryptographyHelper.CalculateHash(path));
        }

    }
}
