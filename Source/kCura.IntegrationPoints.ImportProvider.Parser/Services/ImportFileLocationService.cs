using System.IO;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Synchronizers.RDO;

using SystemInterface.IO;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class ImportFileLocationService : IImportFileLocationService
	{

		private const string _ERROR_FILE_FOLDER_NAME = "Error_Files";
		private const string _ERROR_FILE_NAME_STUB = "Error_file";
		private const string _ELEMENT_SEPARATOR = "-";

		private IIntegrationPointService _integrationPointReader;
		private IDataTransferLocationService _locationService;
		private ISerializer _serializer;
		IDirectory _directoryHelper;

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
			IntegrationPoint ip = _integrationPointReader.GetRdo(integrationPointArtifactId);
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
			IntegrationPoint ip = _integrationPointReader.GetRdo(integrationPointArtifactId);
			ImportProviderSettings settings = _serializer.Deserialize<ImportProviderSettings>(ip.SourceConfiguration);
			ImportSettings destinationConfig = _serializer.Deserialize<ImportSettings>(ip.DestinationConfiguration);
			return Path.Combine(_locationService.GetWorkspaceFileLocationRootPath(destinationConfig.CaseArtifactId),
				settings.LoadFile);
		}

		private string GetErrorFileName(string loadFilePath, string integrationPointName, int integrationPointArtifactId)
		{
			string ext = Path.GetExtension(loadFilePath);
			return string.Concat(integrationPointName, _ELEMENT_SEPARATOR, integrationPointArtifactId.ToString(), _ELEMENT_SEPARATOR, _ERROR_FILE_NAME_STUB, ext);
		}
	}
}
