using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class DataTransferLocationMigrationHelper : IDataTransferLocationMigrationHelper
	{
		private const string SOURCECONFIGURATION_FILESHARE_KEY = "Fileshare";

		private readonly int _workspaceArtifactId;
		private readonly ISerializer _serializer;
		private readonly IResourcePoolManager _resourcePoolManager;
		private readonly IDataTransferLocationService _dataTransferLocationService;

		public DataTransferLocationMigrationHelper(int workspaceArtifactId, IDataTransferLocationService dataTransferLocationService, IResourcePoolManager resourcePoolManager, ISerializer serializer)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_serializer = serializer;
			_resourcePoolManager = resourcePoolManager;
			_dataTransferLocationService = dataTransferLocationService;
		}

		public string GetUpdatedSourceConfiguration(Data.IntegrationPoint integrationPoint)
		{
			Dictionary<string, object> sourceConfiguration = DeserializeSourceConfigurationString(integrationPoint.SourceConfiguration);
			UpdateDataTransferLocation(sourceConfiguration);

			return SerializeSourceConfiguration(sourceConfiguration);
		}

		public void UpdateDataTransferLocation(Dictionary<string, object> sourceConfiguration)
		{
			string currentPath = sourceConfiguration[SOURCECONFIGURATION_FILESHARE_KEY] as string;
			IList<string> processingSourceLocations = GetProcessingSourceLocationsForCurrentWorkspace();
			string exportDestinationFolder = ExtractExportDestinationFolder(processingSourceLocations, currentPath);
			string newDataTransferLocationRoot = _dataTransferLocationService.GetDefaultRelativeLocationFor(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);
			string newPath = Path.Combine(newDataTransferLocationRoot, exportDestinationFolder);

			sourceConfiguration[SOURCECONFIGURATION_FILESHARE_KEY] = newPath;
		}

		private Dictionary<string, object> DeserializeSourceConfigurationString(string sourceConfiguration)
		{
			return _serializer.Deserialize<Dictionary<string, object>>(sourceConfiguration);
		}

		private string SerializeSourceConfiguration(Dictionary<string, object> sourceConfiguration)
		{
			return _serializer.Serialize(sourceConfiguration);
		}

		private IList<string> GetProcessingSourceLocationsForCurrentWorkspace()
		{
			IList<ProcessingSourceLocationDTO> processingSourceLocationDtos = _resourcePoolManager.GetProcessingSourceLocation(_workspaceArtifactId);
			return processingSourceLocationDtos.Select(x => x.Location).ToList();
		}

		private string ExtractExportDestinationFolder(IList<string> processingSourceLocations, string currentExportLocation)
		{
			foreach (var processingSourceLocation in processingSourceLocations)
			{
				if (processingSourceLocation == currentExportLocation)
				{
					//This means that previous Export was done to root of Processing Source Location therefore new destination folder is also root of new Data Transfer Location
					return string.Empty;
				}

				int startIndex = currentExportLocation.IndexOf(processingSourceLocation, StringComparison.Ordinal);

				if (startIndex == -1)
				{
					//ProcessingSourceLocation not found within currentExportLocation
					continue;
				}

				int exportLocationStartIndex = startIndex + processingSourceLocation.Length + 1;
				string exportLocation = currentExportLocation.Substring(exportLocationStartIndex);

				return exportLocation;
			}

			return string.Empty;
		}
	}
}