using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints]
    public class SetTypeOfExportDefaultValueCommandTests : RelativityProviderTemplate
    {
        private IEHHelper _ehHelper;
        private IntegrationPointDto _defaultIntegrationPointDto;
        private IntegrationPointProfileDto _defaultIntegrationPointProfileDto;
        private SetTypeOfExportDefaultValueCommand _setTypeOfExportCommand;

        public SetTypeOfExportDefaultValueCommandTests() : base($"TypeOfExport_Source_{Utils.FormattedDateTimeNow}", $"TypeOfExport_Dest_{Utils.FormattedDateTimeNow}")
        {
        }

        public override void SuiteSetup()
        {
            base.SuiteSetup();
            _ehHelper = new EHHelper(Helper, WorkspaceArtifactId);
            _defaultIntegrationPointDto = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
                "IntegrationPointWithSourceConfToBeCorrected", "Append Only");
            _defaultIntegrationPointProfileDto = CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum.AppendOnly,
                "IntegrationPointWithSourceConfToBeCorrected", "Append Only");
            _setTypeOfExportCommand = SetTypeOfExportDefaultValueCommandFactory.Create(_ehHelper, WorkspaceArtifactId);
        }

        [IdentifiedTest("e311cc35-ced1-4515-a80b-5a63270d4900")]
        public void ItShouldAddDefaultTypeOfExportToSourceConfiguration()
        {
            // Arrange
            IDBContext context = Helper.GetDBContext(WorkspaceArtifactId);
            IntegrationPointDto dbIntegrationPoint = PrepareIntegrationPoint(context);
            IntegrationPointProfileDto dbIntegrationPointProfile = PrepareIntegrationPointProfile(context);

            // Act
            _setTypeOfExportCommand.Execute();

            // Assert
            JToken integrationPointConfigNode = TryGetTypeOfExportNodeFromDbSourceConfig(context, "IntegrationPoint", dbIntegrationPoint);
            Assert.NotNull(integrationPointConfigNode);
            JToken integrationPointProfileConfigNode = TryGetTypeOfExportNodeFromDbSourceConfig(context, "IntegrationPointProfile", dbIntegrationPointProfile);
            Assert.NotNull(integrationPointProfileConfigNode);
        }

        private JToken TryGetTypeOfExportNodeFromDbSourceConfig(IDBContext context, string tableName,
            IntegrationPointDtoBase dbIntegrationPoint)
        {
            DataTable dataTable =
                context.ExecuteSqlStatementAsDataTable(
                    $"SELECT SourceConfiguration FROM {tableName} WHERE ArtifactID = '{dbIntegrationPoint.ArtifactId}'");
            string sourceConfigFromDb = dataTable.Rows[0]["SourceConfiguration"].ToString();
            JObject jObject = JObject.Parse(sourceConfigFromDb);
            return jObject.SelectToken(SourceConfigurationTypeOfExportUpdater.TYPE_OF_EXPORT_NODE_NAME);
        }

        private IntegrationPointDto PrepareIntegrationPoint(IDBContext context)
        {
            IntegrationPointDto createdDto = CreateOrUpdateIntegrationPoint(_defaultIntegrationPointDto);
            SetSourceConfigurationWithoutTypeOfExport(context, "IntegrationPoint", createdDto);
            return createdDto;
        }

        private IntegrationPointProfileDto PrepareIntegrationPointProfile(IDBContext context)
        {
            IntegrationPointProfileDto createdDto = CreateOrUpdateIntegrationPointProfile(_defaultIntegrationPointProfileDto);
            SetSourceConfigurationWithoutTypeOfExport(context, "IntegrationPointProfile", createdDto);
            return createdDto;
        }

        private void SetSourceConfigurationWithoutTypeOfExport(IDBContext context, string tableName, IntegrationPointDtoBase createdModel)
        {
            var sourceConfig = new SqlParameter("@SourceConfiguration", SqlDbType.NVarChar)
            {
                Value = CreateSourceConfigurationWithoutTypeOfExportField()
            };

            context.ExecuteNonQuerySQLStatement(
                $"UPDATE {tableName} SET SourceConfiguration = @SourceConfiguration WHERE ArtifactId = '{createdModel.ArtifactId}'",
                new[] {sourceConfig});
        }

        private string CreateSourceConfigurationWithoutTypeOfExportField()
        {
            string config = CreateSerializedSourceConfigWithTargetWorkspace(SourceWorkspaceArtifactID);
            JObject jObject = JObject.Parse(config);
            JToken typeOfExportNode = jObject.SelectToken(SourceConfigurationTypeOfExportUpdater.TYPE_OF_EXPORT_NODE_NAME);
            typeOfExportNode?.Parent.Remove();
            return jObject.ToString();
        }
    }
}
