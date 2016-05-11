using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class Import : HelperBase
	{
		public Import(Helper helper) : base(helper)
		{
		}

		public void ImportNewDocuments(int workspaceId, DataTable importTable)
		{
			const string relativityWebApiUrl = "http://localhost/RelativityWebAPI/";

			ImportApiFactory _factory = new ImportApiFactory();
			ImportSettings setting = new ImportSettings()
			{
				ArtifactTypeId = (int)ArtifactType.Document,
				CaseArtifactId = workspaceId,
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword,
				WebServiceURL = relativityWebApiUrl,
				ExtractedTextFieldContainsFilePath = false,
				ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
				FieldOverlayBehavior = "Use Field Settings",
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay,
			};

			string settings = JsonConvert.SerializeObject(setting);
			RdoSynchronizerPush pusher = new RdoSynchronizerPush(null, _factory);

			FieldMap mapIdentifier = new FieldMap();
			mapIdentifier.FieldMapType = FieldMapTypeEnum.Identifier;
			mapIdentifier.SourceField = new FieldEntry()
			{
				DisplayName = "Control Number",
				IsIdentifier = true,
				FieldIdentifier = "Control Number",
			};
			mapIdentifier.DestinationField = new FieldEntry()
			{
				DisplayName = "Control Number",
				FieldIdentifier = "1003667",
				IsIdentifier = true,
			};

			FieldMap mapIdentifier2 = new FieldMap();
			mapIdentifier2.FieldMapType = FieldMapTypeEnum.NativeFilePath;
			mapIdentifier2.SourceField = new FieldEntry()
			{
				DisplayName = "NATIVE_FILE_PATH_001",
				IsIdentifier = false,
				FieldIdentifier = "NATIVE_FILE_PATH_001",
			};
			mapIdentifier2.DestinationField = new FieldEntry()
			{
				DisplayName = "NATIVE_FILE_PATH_001",
				FieldIdentifier = "NATIVE_FILE_PATH_001",
				IsIdentifier = false,
			};

			pusher.SyncData(importTable.CreateDataReader(), new FieldMap[] { mapIdentifier }, settings);
		}
	}
}