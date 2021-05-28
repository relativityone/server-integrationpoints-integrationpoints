using System.IO;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Import;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

using SystemInterface.IO;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class ImportFileLocationService : IImportFileLocationService
	{
		private const string _ERROR_FILE_FOLDER_NAME = "Error_Files";
		private const string _ERROR_FILE_NAME_STUB = "Error_file";
		private const string _ELEMENT_SEPARATOR = "-";

		private readonly IDataTransferLocationService _locationService;
		private readonly ISerializer _serializer;
		private readonly IDirectory _directoryHelper;
		private readonly IFileInfoFactory _fileInfoFactory;

		public ImportFileLocationService(
			IDataTransferLocationService locationService,
			ISerializer serializer,
			IDirectory directoryHelper,
			IFileInfoFactory fileInfoFactory)
		{
			_locationService = locationService;
			_serializer = serializer;
			_directoryHelper = directoryHelper;
			_fileInfoFactory = fileInfoFactory;
		}

		public string ErrorFilePath(IntegrationPoint integrationPoint)
		{
			ImportProviderSettings settings = _serializer.Deserialize<ImportProviderSettings>(integrationPoint.SourceConfiguration);
			ImportSettings destinationConfig = _serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration);
			string loadFileBasePath = Path.GetDirectoryName(settings.LoadFile);
			string errorFileDirectory = Path.Combine(_locationService.GetWorkspaceFileLocationRootPath(destinationConfig.CaseArtifactId),
				loadFileBasePath,
				_ERROR_FILE_FOLDER_NAME);
			
			if (! _directoryHelper.Exists(errorFileDirectory))
			{
				_directoryHelper.CreateDirectory(errorFileDirectory);
			}
			return Path.Combine(errorFileDirectory, GetErrorFileName(settings.LoadFile, integrationPoint.Name, integrationPoint.ArtifactId));
		}

		public LoadFileInfo LoadFileInfo(IntegrationPoint integrationPoint)
		{
			ImportProviderSettings settings = _serializer.Deserialize<ImportProviderSettings>(integrationPoint.SourceConfiguration);
			ImportSettings destinationConfig = _serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

			// Retrieve the root path of the workspace file location as well as the relative path of the DataTransfer\Import folder
			// We will verify that the fullPath of the load File exists in this location
			string fileLocationRootPath = _locationService.GetWorkspaceFileLocationRootPath(destinationConfig.CaseArtifactId);
			string dataTransferImportPath = _locationService.GetDefaultRelativeLocationFor(Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid);
			
			// Doing the GetFullPath make sure that if the LoadFile has "../../" in it we get the true path
			string loadFileFullPath = Path.GetFullPath(Path.Combine(fileLocationRootPath, settings.LoadFile));
			
			//We need to do a security check here to ensure that we don't allow paths that are not in the DataTransfer/Import directory
			if (Path.IsPathRooted(settings.LoadFile) || !loadFileFullPath.StartsWith(Path.Combine(fileLocationRootPath, dataTransferImportPath)))
			{
				throw new System.Exception("Invalid Load File Location");
			}

			IFileInfo fileInfo = _fileInfoFactory.Create(loadFileFullPath);

			return new LoadFileInfo
			{
				FullPath = loadFileFullPath,
				Size = fileInfo.Length,
				LastModifiedDate = fileInfo.LastWriteTimeUtc.DateTimeInstance
			};
		}

		private string GetErrorFileName(string loadFilePath, string integrationPointName, int integrationPointArtifactId)
		{
			string ext = Path.GetExtension(loadFilePath);
			return string.Concat(integrationPointName, _ELEMENT_SEPARATOR, integrationPointArtifactId.ToString(), _ELEMENT_SEPARATOR, _ERROR_FILE_NAME_STUB, ext);
		}
	}
}
