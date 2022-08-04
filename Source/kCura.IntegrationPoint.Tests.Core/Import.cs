using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using Newtonsoft.Json;
using NSubstitute;
using Relativity;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class Import
    {
        public static void ImportNewDocuments(int workspaceId, DataTable importTable, FieldMap[] additionalFieldMaps = null)
        {
            IAPILog logger = Substitute.For<IAPILog>();
            ISystemEventLoggingService systemEventLoggingService = Substitute.For<ISystemEventLoggingService>();

            IAuthTokenGenerator authTokenGenerator = new ClaimsTokenGenerator();

            var factory = new ImportApiFactory(authTokenGenerator, systemEventLoggingService, logger);
            var jobFactory = new ImportJobFactory(Substitute.For<IMessageService>());
            var setting = new ImportSettings
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

            IHelper helper = Substitute.For<IHelper>();
            IRelativityFieldQuery relativityFieldQuery = Substitute.For<IRelativityFieldQuery>();

            var rdoSynchronizer = new RdoSynchronizer(relativityFieldQuery, factory, jobFactory, helper);

            var mapIdentifier = new FieldMap
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

            if (additionalFieldMaps != null)
            {
                allFieldMaps = allFieldMaps.Concat(additionalFieldMaps).ToArray();
            }
            DataTableReader reader = importTable.CreateDataReader();
            var context = new DefaultTransferContext(reader);
            rdoSynchronizer.SyncData(context, allFieldMaps, settings, null);
        }


        public static DataTable GetImportTable(string documentPrefix, int numberOfDocuments)
        {
            var table = new DataTable();
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