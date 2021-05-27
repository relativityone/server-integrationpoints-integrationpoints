using System.IO;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Synchronizers.RDO;

using SystemInterface.IO;

using FileInfo = System.IO.FileInfo;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class ImportFileLocationService : IImportFileLocationService
	{

		private const string _ERROR_FILE_FOLDER_NAME = "Error_Files";
		private const string _ERROR_FILE_NAME_STUB = "Error_file";
		private const string _ELEMENT_SEPARATOR = "-";

		private readonly IIntegrationPointService _integrationPointReader;
		private readonly IDataTransferLocationService _locationService;
		private readonly ISerializer _serializer;
		private readonly IDirectory _directoryHelper;

		public ImportFileLocationService(IIntegrationPointService integrationPointReader,
			IDataTransferLocationService locationService,
			ISerializer serializer,
			IDirectory directoryHelper)
		{
			_integrationPointReader = integrationPointReader;
			_locationService = locationService;
			_serializer = serializer;
			_directoryHelper = directoryHelper;
		}

		public string ErrorFilePath(int integrationPointArtifactId)
		{
			IntegrationPoint ip = _integrationPointReader.ReadIntegrationPoint(integrationPointArtifactId);
			ImportProviderSettings settings = _serializer.Deserialize<ImportProviderSettings>(ip.SourceConfiguration);
			ImportSettings destinationConfig = _serializer.Deserialize<ImportSettings>(ip.DestinationConfiguration);
			string loadFileBasePath = Path.GetDirectoryName(settings.LoadFile);
			string errorFileDirectory = Path.Combine(_locationService.GetWorkspaceFileLocationRootPath(destinationConfig.CaseArtifactId),
				loadFileBasePath,
				_ERROR_FILE_FOLDER_NAME);
			
			if (! _directoryHelper.Exists(errorFileDirectory))
			{
				_directoryHelper.CreateDirectory(errorFileDirectory);
			}
			return Path.Combine(errorFileDirectory, GetErrorFileName(settings.LoadFile, ip.Name, integrationPointArtifactId));
		}

		public string LoadFileFullPath(int integrationPointArtifactId)
		{
			IntegrationPoint ip = _integrationPointReader.ReadIntegrationPoint(integrationPointArtifactId);
			ImportProviderSettings settings = _serializer.Deserialize<ImportProviderSettings>(ip.SourceConfiguration);
			ImportSettings destinationConfig = _serializer.Deserialize<ImportSettings>(ip.DestinationConfiguration);

			// Retrieve the root path of the workspace file location as well as the relative path of the DataTransfer\Import folder
			// We will verify that the fullPath of the load File exists in this location
			string fileLocationRootPath = _locationService.GetWorkspaceFileLocationRootPath(destinationConfig.CaseArtifactId);
			string dataTransferImportPath = _locationService.GetDefaultRelativeLocationFor(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid);
			
			// Doing the GetFullPath make sure that if the LoadFile has "../../" in it we get the true path
			string loadFileFullPath = Path.GetFullPath(Path.Combine(fileLocationRootPath, settings.LoadFile));
			
			//We need to do a security check here to ensure that we don't allow paths that are not in the DataTransfer/Import directory
			if (Path.IsPathRooted(settings.LoadFile) || !loadFileFullPath.StartsWith(Path.Combine(fileLocationRootPath, dataTransferImportPath)))
			{
				throw new System.Exception("Invalid Load File Location");
			}
			return loadFileFullPath;
		}

		public FileInfo LoadFileInfo(int integrationPointArtifactId)
		{
			string loadFileFullPath = LoadFileFullPath(integrationPointArtifactId);

			return new FileInfo(loadFileFullPath);
		}

		private string GetErrorFileName(string loadFilePath, string integrationPointName, int integrationPointArtifactId)
		{
			string ext = Path.GetExtension(loadFilePath);
			return string.Concat(integrationPointName, _ELEMENT_SEPARATOR, integrationPointArtifactId.ToString(), _ELEMENT_SEPARATOR, _ERROR_FILE_NAME_STUB, ext);
		}
	}
}
