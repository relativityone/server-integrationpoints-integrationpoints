using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using NSubstitute;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Import
	{
		public static void ImportNewDocuments(int workspaceId, DataTable importTable)
		{
			var helper = Substitute.For<IHelper>();
			var systemEventLoggingService = Substitute.For<ISystemEventLoggingService>();
			var tokenProvider = Substitute.For<ITokenProvider>();
		    var authTokenGenerator = Substitute.For<IAuthTokenGenerator>();
			var federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			var serializer = Substitute.For<ISerializer>();

			ImportApiFactory factory = new ImportApiFactory(tokenProvider, authTokenGenerator, federatedInstanceManager, helper, systemEventLoggingService, serializer);
			ImportJobFactory jobFactory = new ImportJobFactory(Substitute.For<IMessageService>());
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
			var relativityFieldQuery = Substitute.For<IRelativityFieldQuery>();

			RdoSynchronizer pusher = new RdoSynchronizer(relativityFieldQuery, factory, jobFactory, helper);

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

			FieldMap[] allFieldMaps = { mapIdentifier };

			DataTableReader reader = importTable.CreateDataReader();
			var context = new DefaultTransferContext(reader);
			pusher.SyncData(context, allFieldMaps, settings);
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