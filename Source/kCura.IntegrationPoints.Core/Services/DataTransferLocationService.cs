using System;
using System.IO;
using System.Linq;
using SystemInterface.IO;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DataTransferLocationService : IDataTransferLocationService
	{
		#region Fields

		private const string _WORKSPACE_FOLDER_FORMAT = "EDDS{0}";
		private const string _PARENT_FOLDER = "DataTransfer";
	    private const string _INVALID_PATH_ERROR_MSG = "Given Destination Folder path is invalid!";


        private readonly IAPILog _logger;
		private readonly IHelper _helper;

		private readonly IIntegrationPointTypeService _integrationPointTypeService;
		private readonly IDirectory _directoryService;

		#endregion //Fields

		#region Constructors

		public DataTransferLocationService(IHelper helper, IIntegrationPointTypeService integrationPointTypeService, IDirectory directoryService)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationService>();

			_integrationPointTypeService = integrationPointTypeService;
			_directoryService = directoryService;
		}

		#endregion //Constructors

		#region Methods

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

			return Path.Combine(_PARENT_FOLDER, type.Name);
		}

		public string VerifyAndPrepare(int workspaceArtifactId, string path, Guid providerType)
		{
			// Get the given provider type path eg: DataTransfer\Export
			string providerTypeRelativePathPrefix = GetDefaultRelativeLocationFor(providerType);
			
			// First validate if provided path match the correct destnation folder on the server (eg: DataTransfer\Export)
			if (!path.StartsWith(providerTypeRelativePathPrefix))
			{
			    return path;
			}

			string fileShareRootLocation = GetWorkspaceFileLocationRootPath(workspaceArtifactId);
			string fileShareRootLocationWithRelativePath = Path.Combine(fileShareRootLocation, providerTypeRelativePathPrefix);

			// Get physical path for destination folder eg: \\localhost\FileShare\EDDS123456\Export\SomeFolder
			string destinationFolderPhysicalPath = Path.Combine(fileShareRootLocation, path);

			if (!destinationFolderPhysicalPath.IsSubPathOf(fileShareRootLocationWithRelativePath))
			{
                throw new ArgumentException(_INVALID_PATH_ERROR_MSG, paramName:path);
			}

			CreateDirectoryIfNotExists(destinationFolderPhysicalPath);
			return destinationFolderPhysicalPath;
		}

		public string GetWorkspaceFileLocationRootPath(int workspaceArtifactId)
		{
			Workspace workspace = GetWorkspace(workspaceArtifactId);

			if (workspace == null)
			{
				LogMissingWorkspaceError(workspaceArtifactId);
				throw new Exception(nameof(CreateForAllTypes));
			}
			return Path.Combine(workspace.DefaultFileLocation.Name,
				String.Format(_WORKSPACE_FOLDER_FORMAT, workspaceArtifactId));
		}

		protected virtual Workspace GetWorkspace(int workspaceId)
		{
			using (IRSAPIClient rsApiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				rsApiClient.APIOptions.WorkspaceID = -1;
				return rsApiClient.Repositories.Workspace.Read(workspaceId).Results.FirstOrDefault()?.Artifact;
			}
		}

		private string GetDestinationFolderRootPath(int workspaceArtifactId)
		{
			string workspaceFileLocation = GetWorkspaceFileLocationRootPath(workspaceArtifactId);

			return Path.Combine(workspaceFileLocation, _PARENT_FOLDER);
		}

		private void CreateDirectoryIfNotExists(string path)
		{
			if (!_directoryService.Exists(path))
			{
				LogMissingDirectoryCreation(path);

				_directoryService.CreateDirectory(path);
			}
		}

		#region Logging

		private void LogMissingWorkspaceError(int workspaceId)
		{
			_logger.LogError("Cannot find workspace with artifact id: {WorkspaceId}.", workspaceId);
		}

		private void LogMissingDirectoryCreation(string path)
		{
			_logger.LogInformation("Creating missing directory: {path}", path);
		}

		#endregion

		#endregion //Methods
	}
}