using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	static class DefaultExportConfigLoader
	{
		private const string WorkspaceIdKey = "WorkspaceId";
		private const string SavedSearchArtifactIdKey = "SavedSearchArtifactId";
		private const string DefaultSelectedFieldIdsKey = "DefaultSelectedFieldIds";
		private const string DestinationPathKey = "DestinationPath";

		internal static ExportSettings Create()
		{
			return new ExportSettings()
			{
				ArtifactTypeId = 10, //Document
				WorkspaceId = Convert.ToInt32(ConfigurationManager.AppSettings[WorkspaceIdKey]),
				ExportedObjArtifactId = Convert.ToInt32(ConfigurationManager.AppSettings[SavedSearchArtifactIdKey]),
				SelViewFieldIds = ConfigurationManager.AppSettings[DefaultSelectedFieldIdsKey]
					.Split(',')
					.Select(item => Convert.ToInt32(item))
					.ToList(),
				ExportFilesLocation = ConfigurationManager.AppSettings[DestinationPathKey]
			};
		}
	}
}
