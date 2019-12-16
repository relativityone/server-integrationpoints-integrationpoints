using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Extensions
{
	public static class ImportSettingsExtensions
	{
		private const string _SENSITIVE_DATA_REMOVED = "[Sensitive user data has been removed]";

		public static string SerializeWithoutSensitiveData(this ImportSettings settingsToClear, ISerializer serializer)
		{
			string settingsString = serializer.Serialize(settingsToClear);
			ImportSettings result = serializer.Deserialize<ImportSettings>(settingsString);
			result.FederatedInstanceCredentials = RemoveDataIfNotEmpty(result.FederatedInstanceCredentials);
			result.ErrorFilePath = RemoveDataIfNotEmpty(result.ErrorFilePath);
			result.FolderPathSourceFieldName = RemoveDataIfNotEmpty(result.FolderPathSourceFieldName);
			result.NativeFilePathSourceFieldName = RemoveDataIfNotEmpty(result.NativeFilePathSourceFieldName);
			result.FileNameColumn = RemoveDataIfNotEmpty(result.FileNameColumn);
			result.FileSizeColumn = RemoveDataIfNotEmpty(result.FileSizeColumn);
			result.SupportedByViewerColumn = RemoveDataIfNotEmpty(result.SupportedByViewerColumn);
			result.RelativityPassword = RemoveDataIfNotEmpty(result.RelativityPassword);
			result.RelativityUsername = RemoveDataIfNotEmpty(result.RelativityUsername);
			result.SelectedCaseFileRepoPath = RemoveDataIfNotEmpty(result.SelectedCaseFileRepoPath);
			result.DestinationIdentifierField = RemoveDataIfNotEmpty(result.DestinationIdentifierField);
			result.ProductionPrecedence = RemoveDataIfNotEmpty(result.ProductionPrecedence);
			result.ImagePrecedence = result.ImagePrecedence.Select(prod => new ProductionDTO
			{
				ArtifactID = prod.ArtifactID,
				DisplayName = _SENSITIVE_DATA_REMOVED
			});
			return serializer.Serialize(result);
		}

		private static string RemoveDataIfNotEmpty(string original)
		{
			return string.IsNullOrEmpty(original) ? original : _SENSITIVE_DATA_REMOVED;
		}
	}
}
