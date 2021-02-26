#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using System;
using System.IO;
using System.Linq;
using SystemInterface.IO;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using kCura.IntegrationPoints.Core.Helpers;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DataTransferLocationService : IDataTransferLocationService
	{
		#region Fields

		private const string _WORKSPACE_FOLDER_FORMAT = "EDDS{0}";
		private const string _EDDS_PARENT_FOLDER = "DataTransfer";
		private const string _INVALID_PATH_ERROR_MSG = "Given Destination Folder path is invalid!";
		private readonly IAPILog _logger;
		private readonly IHelper _helper;

		private readonly IIntegrationPointTypeService _integrationPointTypeService;
		private readonly IDirectory _directoryService;
		private readonly ICryptographyHelper _cryptographyHelper;
		#endregion //Fields

		#region Constructors

		public DataTransferLocationService(IHelper helper, IIntegrationPointTypeService integrationPointTypeService, IDirectory directoryService, ICryptographyHelper cryptographyHelper)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationService>();

			_integrationPointTypeService = integrationPointTypeService;
			_directoryService = directoryService;
			_cryptographyHelper = cryptographyHelper;
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
					WorkspaceRef workspace = new WorkspaceRef {ArtifactID = workspaceArtifactId};

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

		protected virtual Workspace GetWorkspace(int workspaceId)
		{
			var rsapiClientFactory = new RsapiClientFactory();
			using (IRSAPIClient rsApiClient = rsapiClientFactory.CreateAdminClient(_helper))
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

		private void LogMissingWorkspaceError(Exception ex, int workspaceId)
		{
			_logger.LogError(ex,"Cannot find workspace with artifact id: {WorkspaceId}.", workspaceId);
		}

		private void LogMissingDirectoryCreation(string path)
		{
			_logger.LogInformation("Creating missing directory: {path}", _cryptographyHelper.CalculateHash(path));
		}

		#endregion

		#endregion //Methods
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
