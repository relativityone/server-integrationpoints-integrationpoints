using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Properties;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Interfaces.Private.Models.IntegrationPoint;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    internal class IntegrationPointManagerTests : TestsBase
    {
        private IIntegrationPointManager _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sut = Container.Resolve<IIntegrationPointManager>();
        }

        [IdentifiedTest("65A0D498-0C79-4271-B1E0-3B68C718497D")]
        public async Task CreateIntegrationPointAsync_ShouldCreateIntegrationPoint()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint();

            // Act
            IntegrationPointModel integrationPointModel = await _sut.CreateIntegrationPointAsync(request).ConfigureAwait(false);

            // Assert
            integrationPointModel.ArtifactId.Should().NotBe(request.IntegrationPoint.ArtifactId);
            integrationPointModel.Name.ShouldBeEquivalentTo(request.IntegrationPoint.Name);
            integrationPointModel.SourceProvider.ShouldBeEquivalentTo(request.IntegrationPoint.SourceProvider);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(request.IntegrationPoint.DestinationProvider);
        }

        [IdentifiedTest("D0D9F342-EF26-4A6F-BB30-57637C9ED812")]
        public async Task UpdateIntegrationPointAsync_ShouldUpdateIntegrationPoint()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint();
            UpdateIntegrationPointRequest updateRequest = new UpdateIntegrationPointRequest
            {
                IntegrationPoint = request.IntegrationPoint,
                WorkspaceArtifactId = request.WorkspaceArtifactId
            };

            // Act
            IntegrationPointModel integrationPointModel = await _sut.UpdateIntegrationPointAsync(updateRequest).ConfigureAwait(false);

            // Assert
            integrationPointModel.ArtifactId.Should().Be(request.IntegrationPoint.ArtifactId);
            integrationPointModel.Name.ShouldBeEquivalentTo(request.IntegrationPoint.Name);
            integrationPointModel.SourceProvider.ShouldBeEquivalentTo(request.IntegrationPoint.SourceProvider);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(request.IntegrationPoint.DestinationProvider);
        }

        [IdentifiedTest("1316A6DD-F54E-49C9-AAEE-C7B66D444A09")]
        public async Task GetIntegrationPointAsync_ShouldGetExistingIntegrationPoint()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint();
            IntegrationPointModel createdIntegrationPointModel = await _sut.CreateIntegrationPointAsync(request).ConfigureAwait(false);
            
            // Act
            IntegrationPointModel integrationPointModel =  await _sut
                .GetIntegrationPointAsync(request.WorkspaceArtifactId, createdIntegrationPointModel.ArtifactId).ConfigureAwait(false);

            // Assert
            integrationPointModel.ArtifactId.Should().Be(createdIntegrationPointModel.ArtifactId);
            integrationPointModel.Name.ShouldBeEquivalentTo(createdIntegrationPointModel.Name);
            integrationPointModel.SourceProvider.ShouldBeEquivalentTo(createdIntegrationPointModel.SourceProvider);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(createdIntegrationPointModel.DestinationProvider);
        }

        [IdentifiedTest("FAAD45A6-F2F7-4042-A0A9-8D46700154A1")]
        public async Task RunIntegrationPointAsync_ShouldNotThrowError()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint();
            IntegrationPointModel createdIntegrationPointModel = await _sut.CreateIntegrationPointAsync(request).ConfigureAwait(false);
            CreateAgentDataTable();

            // Act
            Action action = async () => await _sut.RunIntegrationPointAsync(request.WorkspaceArtifactId, createdIntegrationPointModel.ArtifactId);

            // Assert
            action.ShouldNotThrow();
        }

        [IdentifiedTest("2035CBCC-D56D-493C-9B13-D273C2EA2790")]
        public async Task RetryIntegrationPointAsync_ShouldNotThrowError()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint();
            IntegrationPointModel createdIntegrationPointModel = await _sut.CreateIntegrationPointAsync(request).ConfigureAwait(false);
            CreateAgentDataTable();

            // Act
            Action action = async () => await _sut.RetryIntegrationPointAsync(request.WorkspaceArtifactId, createdIntegrationPointModel.ArtifactId);

            // Assert
            action.ShouldNotThrow();
        }

        [IdentifiedTest("A04A9599-B4FE-4F7B-8F07-2851F981865A")]
        public async Task GetAllIntegrationPointsAsync_ShouldReturnAllCreatedIntegrationPoints()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint();

            const int integrationPointsCount = 5;
            // integrationPointsCount is decreased because on is already created in PrepareIntegrationPoint
            for (int i = 0; i < integrationPointsCount - 1; i++)
            {
                await _sut.CreateIntegrationPointAsync(request).ConfigureAwait(false);
            }

            // Act
            IList<IntegrationPointModel> integrationPoints = await _sut.GetAllIntegrationPointsAsync(SourceWorkspace.ArtifactId);

            // Assert
            integrationPoints.Count.ShouldBeEquivalentTo(integrationPointsCount);
            foreach (IntegrationPointModel integrationPointModel in integrationPoints)
            {
                integrationPointModel.Name.ShouldBeEquivalentTo(request.IntegrationPoint.Name);
                integrationPointModel.SourceProvider.ShouldBeEquivalentTo(request.IntegrationPoint.SourceProvider);
                integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(request.IntegrationPoint.DestinationProvider);
            }
        }

        private CreateIntegrationPointRequest PrepareIntegrationPoint()
        {
            SourceWorkspace.Fields.Add(new FieldTest
            {
                Guid = IntegrationPointFieldGuids.OverwriteFieldsGuid,
                Artifact = { ArtifactId = Const.OVERWRITE_FIELD_ARTIFACT_ID }
            });

            Helper.DbContextMock
                .Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.Is<SqlParameter[]>(y => 
                    y[0].ParameterName == "@WorkspaceID" &&
                    y[1].ParameterName == "@RelatedObjectArtifactID")))
                .Returns(new DataTable());

            SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(SourceWorkspace);
            IntegrationPointTest integrationPoint = SourceWorkspace.IntegrationPoints[SourceWorkspace.IntegrationPoints.Count - 1];
            CreateIntegrationPointRequest request = new CreateIntegrationPointRequest
            {
                IntegrationPoint = new IntegrationPointModel
                {
                    ArtifactId = integrationPoint.ArtifactId,
                    Name = integrationPoint.Name,
                    SourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(integrationPoint.SourceConfiguration),
                    DestinationConfiguration = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration),
                    SourceProvider = (int)integrationPoint.SourceProvider,
                    DestinationProvider = (int)integrationPoint.DestinationProvider,
                    FieldMappings = JsonConvert.DeserializeObject<List<FieldMap>>(integrationPoint.FieldMappings),
                    ImportFileCopyMode = ImportFileCopyModeEnum.DoNotImportNativeFiles,
                    LogErrors = true,
                    EmailNotificationRecipients = "",
                    OverwriteFieldsChoiceId = Const.Choices.OverwriteFields.Single(x => x.Name == "Append/Overlay").ArtifactID,
                    Type = (int)integrationPoint.Type,
                    ScheduleRule = new ScheduleModel()
                },
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            return request;
        }

        private void CreateAgentDataTable()
        {
            DataTable agentDataTable = new DataTable();
            agentDataTable.Columns.Add("AgentTypeID", typeof(int));
            agentDataTable.Columns.Add("Name", typeof(string));
            agentDataTable.Columns.Add("Fullnamespace", typeof(string));
            agentDataTable.Columns.Add("Guid", typeof(Guid));
            DataRow row = agentDataTable.NewRow();
            row["AgentTypeID"] = 0;
            row["Name"] = "Adler Sieben Agent";
            row["Fullnamespace"] = "Fullnamespace";
            row["Guid"] = Guid.NewGuid();
            agentDataTable.Rows.Add(row);
            Helper.DbContextMock
                .Setup(x => x.ExecuteSqlStatementAsDataTable(Resources.GetAgentTypeInformation, It.Is<List<SqlParameter>>(y =>
                    y[0].ParameterName == "@AgentID" &&
                    y[1].ParameterName == "@AgentGuid")))
                .Returns(agentDataTable);
        }
    }
}
