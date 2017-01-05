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

		public string GetUpdatedSourceConfiguration(string sourceConfiguration, IList<string> processingSourceLocations, string newDataTransferLocationRoot)
		{
			Dictionary<string, object> sourceConfigurationDictionary = DeserializeSourceConfigurationString(sourceConfiguration);
			UpdateDataTransferLocation(sourceConfigurationDictionary, processingSourceLocations, newDataTransferLocationRoot);

			return SerializeSourceConfiguration(sourceConfigurationDictionary);
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

				if (currentExportLocation.StartsWith(processingSourceLocation, StringComparison.Ordinal))
				{
					return currentExportLocation.Substring(processingSourceLocation.Length);
				}
			}

			return string.Empty;
		}
	}
}