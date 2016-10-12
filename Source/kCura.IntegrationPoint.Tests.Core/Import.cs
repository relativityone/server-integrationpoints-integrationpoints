using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Import
	{
		public static void ImportNewDocuments(int workspaceId, DataTable importTable)
		{
			var helper = NSubstitute.Substitute.For<IHelper>();
			ImportApiFactory factory = new ImportApiFactory(helper);
			ImportSettings setting = new ImportSettings()
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				CaseArtifactId = workspaceId,
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword,
				WebServiceURL = SharedVariables.RelativityWebApiUrl,
				ExtractedTextFieldContainsFilePath = false,
				ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
				FieldOverlayBehavior = "Use Field Settings",
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay,
				ExtractedTextFileEncoding = "utf-8"
			};

			string settings = JsonConvert.SerializeObject(setting);
			RdoSynchronizerPush pusher = new RdoSynchronizerPush(null, factory, helper);

			FieldMap mapIdentifier = new FieldMap
			{
				FieldMapType = FieldMapTypeEnum.Identifier,
				SourceField = new FieldEntry()
				{
					DisplayName = "Control Number",
					IsIdentifier = true,
					FieldIdentifier = "Control Number",
				},
				DestinationField = new FieldEntry()
				{
					DisplayName = "Control Number",
					FieldIdentifier = "1003667",
					IsIdentifier = true,
				}
			};

			//FieldMap mapIdentifier = FieldMaps.CreateIdentifierFieldMapForRelativityProvider(workspaceId, workspaceId);

			FieldMap[] allFieldMaps = { mapIdentifier };

			//FieldMap datefield2 = new FieldMap
			//{
			//	FieldMapType = FieldMapTypeEnum.None,
			//	SourceField = new FieldEntry()
			//	{
			//		DisplayName = "Date Sent",
			//		IsIdentifier = false,
			//		FieldIdentifier = "Date Sent",
			//	},
			//	DestinationField = new FieldEntry()
			//	{
			//		DisplayName = "Date Sent",
			//		FieldIdentifier = "1035355",
			//		IsIdentifier = false,
			//	}
			//};

			// TODO: make this work
			//FieldMap mapIdentifier2 = new FieldMap
			//{
			//	FieldMapType = FieldMapTypeEnum.NativeFilePath,
			//	SourceField = new FieldEntry()
			//	{
			//		DisplayName = "NATIVE_FILE_PATH_001",
			//		IsIdentifier = false,
			//		FieldIdentifier = "NATIVE_FILE_PATH_001",
			//	},
			//	DestinationField = new FieldEntry()
			//	{
			//		DisplayName = "NATIVE_FILE_PATH_001",
			//		FieldIdentifier = "NATIVE_FILE_PATH_001",
			//		IsIdentifier = false,
			//	}
			//};

			pusher.SyncData(importTable.CreateDataReader(), allFieldMaps, settings);
		}

		public static DataTable GetImportTable(string documentPrefix, int numberOfDocuments)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));

			for (int index = 1; index <= numberOfDocuments; index++)
			{
				string controlNumber = $"{documentPrefix}{index}";
				table.Rows.Add(controlNumber);
			}
			return table;
		}
	}
}