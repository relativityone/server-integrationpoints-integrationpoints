using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemInterface.IO;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DataTransferLocationService : IDataTransferLocationService
	{
		#region Fields

		private const string _WORKSPACE_FOLDER_FORMAT = "EDDS{0}";
		private const string _EDDS_PARENT_FOLDER = "DataTransfer";
	    private const string _INVALID_PATH_ERROR_MSG = "Given Destination Folder path is invalid!";
	    private const string _PSL_DISABLED_ERROR_MSG = "Given Destination Folder path is invalid. Processing Source Location is not enabled!";
	    private const string _PSL_INVALID_ERROR_MSG = "Given Destination Folder path is invalid for Processing Source Location!";

        private readonly IAPILog _logger;
		private readonly IHelper _helper;

		private readonly IIntegrationPointTypeService _integrationPointTypeService;
		private readonly IDirectory _directoryService;
        private readonly IResourcePoolContext _resourcePoolContext;
	    private readonly IResourcePoolManager _resourcePoolManager;


        #endregion //Fields

        #region Constructors

        public DataTransferLocationService(IHelper helper, IIntegrationPointTypeService integrationPointTypeService, IDirectory directoryService, IResourcePoolContext resourcePoolContext, IResourcePoolManager resourcePoolManager)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationService>();

			_integrationPointTypeService = integrationPointTypeService;
			_directoryService = directoryService;
		    _resourcePoolContext = resourcePoolContext;
		    _resourcePoolManager = resourcePoolManager;
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

			return Path.Combine(_EDDS_PARENT_FOLDER, type.Name);
		}


	    public bool IsEddsPath(string path)
	    {
	        return path.StartsWith(_EDDS_PARENT_FOLDER);
	    }

	    private string VerifyAndPrepareEdds(int workspaceArtifactId, string path, Guid providerType)
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

	    private string VerifyAndPrepareProcessingSourceLocation(int workspaceArtifactId, string path)
	    {
	        if (!_resourcePoolContext.IsProcessingSourceLocationEnabled())
	        {
	            throw new ArgumentException(_PSL_DISABLED_ERROR_MSG,path);
	        }

	        List<ProcessingSourceLocationDTO> processingSourceLocations =
	            _resourcePoolManager.GetProcessingSourceLocation(workspaceArtifactId);

	        if (processingSourceLocations.Select(dto => dto.Location).All(location => location != path))
	        {
	            throw new ArgumentException(_PSL_INVALID_ERROR_MSG, path);
	        }

            return path;
	    }

		public string VerifyAndPrepare(int workspaceArtifactId, string path, Guid providerType)
		{
		    if (IsEddsPath(path))
		    {
		        return VerifyAndPrepareEdds(workspaceArtifactId, path, providerType);
		    }

		    return VerifyAndPrepareProcessingSourceLocation(workspaceArtifactId, path);
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

			return Path.Combine(workspaceFileLocation, _EDDS_PARENT_FOLDER);
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