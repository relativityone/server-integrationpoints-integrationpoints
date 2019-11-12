using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.DataExchange.Service;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals
{
	internal static class ExportTestSettingsBuilder
	{
		public static Core.ExportSettings CreateExportSettings(ExportTestConfiguration testConfiguration, ExportTestContext testContext)
		{
			Dictionary<int, FieldEntry> fieldIds = BuildFieldsDictionary(testContext);

			var settings = new Core.ExportSettings
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				TypeOfExport = Core.ExportSettings.ExportType.SavedSearch,
				ExportFilesLocation = Path.Combine(testConfiguration.DestinationPath, DateTime.UtcNow.ToString("HHmmss_fff")),
				WorkspaceId = testContext.WorkspaceID,
				SavedSearchArtifactId = testContext.ExportedObjArtifactID,
				SavedSearchName = testConfiguration.SavedSearchArtifactName,
				SelViewFieldIds = fieldIds,
				SelectedImageDataFileFormat = Core.ExportSettings.ImageDataFileFormat.None,
				TextPrecedenceFieldsIds = new List<int> { int.Parse(testContext.LongTextField.FieldIdentifier) },
				DataFileEncoding = Encoding.Unicode,
				VolumeMaxSize = 650,
				ImagePrecedence =
					new[]
					{
						new ProductionDTO
						{
							ArtifactID = testContext.ProductionArtifactID.ToString(),
							DisplayName = "Production"
						}
					},
				SubdirectoryStartNumber = 1,
				VolumeStartNumber = 1
			};

			return settings;
		}

		private static Dictionary<int, FieldEntry> BuildFieldsDictionary(ExportTestContext testContext)
		{
			Dictionary<int, FieldEntry> fieldsDictionary = testContext
				.DefaultFields
				.ToDictionary(item => int.Parse(item.FieldIdentifier));

			AddLongTextFieldIfMissing(fieldsDictionary, testContext);
			return fieldsDictionary;
		}

		private static void AddLongTextFieldIfMissing(IDictionary<int, FieldEntry> fieldsDictionary, ExportTestContext testContext)
		{
			int longTextFieldIdentifier = int.Parse(testContext.LongTextField.FieldIdentifier);
			if (!fieldsDictionary.ContainsKey(longTextFieldIdentifier))
			{
				fieldsDictionary.Add(longTextFieldIdentifier, testContext.LongTextField);
			}
		}
	}
}
