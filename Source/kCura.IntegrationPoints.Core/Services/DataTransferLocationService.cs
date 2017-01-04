using System;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DataTransferLocationService : IDataTransferLocationService
	{
		private const string _WORKSPACE_FOLDER_FORMAT = "EDDS{0}";
		private const string _PARENT_FOLDER = "DataTransfer";

		private readonly IAPILog _logger;
		private readonly IHelper _helper;

		private readonly IIntegrationPointTypeService _integrationPointTypeService;

		public DataTransferLocationService(IHelper helper, IIntegrationPointTypeService integrationPointTypeService)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationService>();

			_integrationPointTypeService = integrationPointTypeService;
		}

		public void CreateForAllTypes(int workspaceArtifactId)
		{
			string rootPath = GetWorkspaceRootPath(workspaceArtifactId);

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

		public string GetRootLocationFor(int workspaceArtifactId)
		{
			return GetWorkspaceRootPath(workspaceArtifactId);
		}

		public string GetLocationFor(int workspaceArtifactId, Guid integrationPointTypeIdentifier)
		{
			string rootPath = GetWorkspaceRootPath(workspaceArtifactId);

			IntegrationPointType type = _integrationPointTypeService.GetIntegrationPointType(integrationPointTypeIdentifier);

			string path = Path.Combine(rootPath, type.Name);

			// TODO figure out what to do in case of the Import types if path does not exists
			// for now we will create it however there won't be anything to import...
			CreateDirectoryIfNotExists(path);

			return path;
		}

		public string VerifyAndPrepare(int workspaceArtifactId, string path)
		{
			string rootPath = GetWorkspaceRootPath(workspaceArtifactId);

			// remove previous root from path...
			if (Path.IsPathRooted(path))
			{
				// Path.GetPathRoot() returns whole path as root 
				var root = path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

				path = path.Substring(root.Length + 2);
			}

			// ...and append a proper one
			path = Path.Combine(rootPath, path);

			CreateDirectoryIfNotExists(path);

			return path;
		}

		private Workspace GetWorkspace(int workspaceId)
		{
			using (IRSAPIClient rsApiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				rsApiClient.APIOptions.WorkspaceID = -1;
				return rsApiClient.Repositories.Workspace.Read(workspaceId).Results.FirstOrDefault()?.Artifact;
			}
		}

		private string GetWorkspaceRootPath(int workspaceArtifactId)
		{
			Workspace workspace = GetWorkspace(workspaceArtifactId);

			if (workspace == null)
			{
				LogMissingWorkspaceError(workspaceArtifactId);
				throw new Exception(nameof(CreateForAllTypes));
			}

			return Path.Combine(
				workspace.DefaultFileLocation.Name,
				String.Format(_WORKSPACE_FOLDER_FORMAT, workspace.ArtifactID),
				_PARENT_FOLDER
			);
		}

		private void CreateDirectoryIfNotExists(string path)
		{
			if (!Directory.Exists(path))
			{
				LogMissingDirectoryCreation(path);

				Directory.CreateDirectory(path);
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
	}
}