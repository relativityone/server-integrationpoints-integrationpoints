
using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.Services.Models
{
	public class LoadFileExportDestinationConfigurationBackwardCompatibility
	{
		public int ArtifactTypeID { get; set; }

		/// <summary>
		///     Why do we need this?
		/// </summary>
		public string DestinationProviderType { get; set; }

		/// <summary>
		///     The same as TargetWorkspaceId in source configuration
		/// </summary>
		public int CaseArtifactId { get; set; }

		/// <summary>
		///     Why do we need this?
		/// </summary>
		public string Provider { get; set; }

		/// <summary>
		///     Used only on UI
		/// </summary>
		public bool DoNotUseFieldsMapCache { get; set; }

		public LoadFileExportDestinationConfigurationBackwardCompatibility(LoadFileExportDestinationConfiguration destinationConfiguration, 
			LoadFileExportSourceConfiguration sourceConfiguration)
		{
			ArtifactTypeID = destinationConfiguration.ArtifactTypeId;
			DestinationProviderType = Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID;
			CaseArtifactId = sourceConfiguration.SourceWorkspaceArtifactId;
			Provider = Constants.IntegrationPoints.FILESHARE_PROVIDER_NAME;
			DoNotUseFieldsMapCache = false;
		}
	}
}
