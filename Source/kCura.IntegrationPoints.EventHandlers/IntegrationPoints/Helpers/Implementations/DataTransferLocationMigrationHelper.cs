using System;
using System.Collections.Generic;
using System.IO;
using kCura.Apps.Common.Utils.Serializers;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class DataTransferLocationMigrationHelper : IDataTransferLocationMigrationHelper
	{
		private const string SOURCECONFIGURATION_FILESHARE_KEY = "Fileshare";
		private readonly ISerializer _serializer;

		public DataTransferLocationMigrationHelper(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public string GetUpdatedSourceConfiguration(Data.IntegrationPoint integrationPoint, IList<string> processingSourceLocations, string newDataTransferLocationRoot)
		{
			Dictionary<string, object> sourceConfiguration = DeserializeSourceConfigurationString(integrationPoint.SourceConfiguration);
			UpdateDataTransferLocation(sourceConfiguration, processingSourceLocations, newDataTransferLocationRoot);

			return SerializeSourceConfiguration(sourceConfiguration);
		}

		public void UpdateDataTransferLocation(IDictionary<string, object> sourceConfiguration, IList<string> processingSourceLocations, string newDataTransferLocationRoot)
		{
			string currentPath = sourceConfiguration[SOURCECONFIGURATION_FILESHARE_KEY] as string;
			string exportDestinationFolder = ExtractExportDestinationFolder(processingSourceLocations, currentPath);
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