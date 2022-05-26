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
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using Choice = Relativity.Services.ChoiceQuery.Choice;

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
            AssertIntegrationPointModel(integrationPointModel, new IntegrationPointDesiredState(request.IntegrationPoint), artifactIdShouldEqual: false);
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
            AssertIntegrationPointModel(integrationPointModel, new IntegrationPointDesiredState(request.IntegrationPoint), artifactIdShouldEqual: true);
        }

        [IdentifiedTest("1316A6DD-F54E-49C9-AAEE-C7B66D444A09")]
        public async Task GetIntegrationPointAsync_ShouldGetExistingIntegrationPoint()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint();

            // Act
            IntegrationPointModel integrationPointModel = await _sut
                .GetIntegrationPointAsync(request.WorkspaceArtifactId, request.IntegrationPoint.ArtifactId).ConfigureAwait(false);

            // Assert
            AssertIntegrationPointModel(integrationPointModel, new IntegrationPointDesiredState(request.IntegrationPoint), true);
        }

        [IdentifiedTest("FAAD45A6-F2F7-4042-A0A9-8D46700154A1")]
        public void RunIntegrationPointAsync_ShouldNotThrowError()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint();
            CreateAgentDataTable();

            // Act
            Func<Task> func = async () => await _sut.RunIntegrationPointAsync(request.WorkspaceArtifactId, request.IntegrationPoint.ArtifactId);

            // Assert
            func.ShouldNotThrow();
            Proxy.ObjectManager.Mock.Verify(x => x.ReadAsync(SourceWorkspace.ArtifactId, It.Is<ReadRequest>(y =>
                    y.Object.ArtifactID == request.IntegrationPoint.ArtifactId &&
                    y.Fields.Count() == typeof(IntegrationPointFieldGuids).GetFields().Length)), Times.AtLeastOnce);
        }

        [IdentifiedTest("2035CBCC-D56D-493C-9B13-D273C2EA2790")]
        public void RetryIntegrationPointAsync_ShouldNotThrowError()
        {
            // Arrange
            CreateIntegrationPointRequest request = PrepareIntegrationPoint(true);
            SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(new JobTest(), new IntegrationPointTest
            {
                Artifact = { ArtifactId = request.IntegrationPoint.ArtifactId },
                LastRuntimeUTC = DateTime.Now,
                HasErrors = true
            });

            CreateAgentDataTable();

            // Act
            Func<Task> func = () => _sut.RetryIntegrationPointAsync(request.WorkspaceArtifactId, request.IntegrationPoint.ArtifactId);

            // Assert
            func.ShouldNotThrow();
            Proxy.ObjectManager.Mock.Verify(x => x.ReadAsync(SourceWorkspace.ArtifactId, It.Is<ReadRequest>(y =>
                y.Object.ArtifactID == request.IntegrationPoint.ArtifactId &&
                y.Fields.Count() == typeof(IntegrationPointFieldGuids).GetFields().Length)), Times.AtLeastOnce);
        }

        [IdentifiedTest("A04A9599-B4FE-4F7B-8F07-2851F981865A")]
        public async Task GetAllIntegrationPointsAsync_ShouldReturnAllCreatedIntegrationPoints()
        {
            // Arrange
            const int integrationPointsCount = 5;
            List<IntegrationPointTest> integrationPointsCreated =
                Enumerable.Range(0, integrationPointsCount)
                    .Select(i => SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(SourceWorkspace))
                    .ToList();

            // Act
            IList<IntegrationPointModel> integrationPoints = await _sut.GetAllIntegrationPointsAsync(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            // Assert
            integrationPoints.Count.ShouldBeEquivalentTo(integrationPointsCount);

            for(int i = 0; i < integrationPointsCount; i++)
            {
                AssertIntegrationPointModel(
                    integrationPoints[i],
                    new IntegrationPointDesiredState(integrationPointsCreated[i]),
                    artifactIdShouldEqual: true);
            }
        }

        [IdentifiedTest("E7746701-32FF-4725-AB52-DB0325DB1015")]
        public async Task GetOverwriteFieldsChoicesAsync_ShouldReturnAllOverwriteFieldsChoices()
        {
            // Arrange
            PrepareIntegrationPoint();

            // Act
            IList<OverwriteFieldsModel> overwriteFieldsChoices = await _sut.GetOverwriteFieldsChoicesAsync(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            // Assert
            overwriteFieldsChoices.Count.ShouldBeEquivalentTo(Const.Choices.OverwriteFields.Count);
            foreach (Choice overwriteFieldsChoice in Const.Choices.OverwriteFields)
            {
                overwriteFieldsChoices.Select(x => x.ArtifactId).Contains(overwriteFieldsChoice.ArtifactID)
                    .ShouldBeEquivalentTo(true);
            }
        }

        [IdentifiedTest("BAB45AB0-CE27-4708-AC79-744C98D391B7")]
        public async Task CreateIntegrationPointFromProfileAsync_ShouldCreateIntegrationPointFromProfile()
        {
            // Arrange
            const string integrationPointName = "Adler Sieben";
            IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(SourceWorkspace);
            PrepareMocks();

            // Act
            IntegrationPointModel integrationPointModel = await _sut
                .CreateIntegrationPointFromProfileAsync(SourceWorkspace.ArtifactId, integrationPointProfile.ArtifactId, integrationPointName).ConfigureAwait(false);

            // Assert
            IntegrationPointDesiredState desiredState = new IntegrationPointDesiredState(integrationPointProfile)
            {
                Name = integrationPointName
            };
            AssertIntegrationPointModel(integrationPointModel, desiredState, false);
        }

        [IdentifiedTest("A5B7851C-2709-4A6B-A12D-2C8C219D4578")]
        public async Task GetIntegrationPointArtifactTypeIdAsync_ShouldReturnProperIntegrationPointArtifactTypeId()
        {
            // Arrange
            PrepareMocks();

            const int objectTypeArtifactId = 1234;
            Proxy.ObjectTypeManager.Mock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => Task.FromResult(new ObjectTypeResponse { ArtifactTypeID = objectTypeArtifactId }));

            // Act
            int integrationPointArtifactTypeId = await _sut.GetIntegrationPointArtifactTypeIdAsync(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            // Assert
            integrationPointArtifactTypeId.ShouldBeEquivalentTo(objectTypeArtifactId);
        }

        private CreateIntegrationPointRequest PrepareIntegrationPoint(bool integrationPointsWithErrors = false)
        {
            PrepareMocks();
            IntegrationPointTest integrationPoint = integrationPointsWithErrors ?
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPointWithErrors(SourceWorkspace) :
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(SourceWorkspace);

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

        #region private Methods

        private void PrepareMocks()
        {
            SourceWorkspace.Fields.Add(new FieldTest
            {
                Guid = IntegrationPointFieldGuids.OverwriteFieldsGuid,
                Artifact = { ArtifactId = Const.OVERWRITE_FIELD_ARTIFACT_ID }
            });

            SourceWorkspace.Fields.Add(new FieldTest
            {
                Guid = ObjectTypeGuids.IntegrationPointGuid,
                Artifact = { ArtifactId = Const.INTEGRATION_POINTS_ARTIFACT_ID }
            });

            Helper.DbContextMock
                .Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.Is<SqlParameter[]>(y =>
                    y[0].ParameterName == "@WorkspaceID" &&
                    y[1].ParameterName == "@RelatedObjectArtifactID")))
                .Returns(new DataTable());
        }

        private void AssertIntegrationPointModel(IntegrationPointModel integrationPointModel,
            IntegrationPointDesiredState integrationPointDesiredExpectedValues, bool artifactIdShouldEqual)
        {
            if (artifactIdShouldEqual)
            {
                integrationPointModel.ArtifactId.Should().Be(integrationPointDesiredExpectedValues.ArtifactId);
            }
            else
            {
                integrationPointModel.ArtifactId.Should().NotBe(integrationPointDesiredExpectedValues.ArtifactId);
            }

            integrationPointModel.Name.Should().Be(integrationPointDesiredExpectedValues.Name);
            integrationPointModel.SourceProvider.ShouldBeEquivalentTo(integrationPointDesiredExpectedValues.SourceProvider);
            integrationPointModel.DestinationProvider.ShouldBeEquivalentTo(integrationPointDesiredExpectedValues.DestinationProvider);
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

    #endregion

    internal class IntegrationPointDesiredState
    {
        internal int ArtifactId { get; set; }
        internal int SourceProvider { get; set; }
        internal int DestinationProvider { get; set; }
        internal string Name { get; set; }

        internal IntegrationPointDesiredState(IntegrationPointTest integrationPointTest)
        {
            ArtifactId = integrationPointTest.ArtifactId;
            Name = integrationPointTest.Name;
            SourceProvider = integrationPointTest.SourceProvider ?? 0;
            DestinationProvider = integrationPointTest.DestinationProvider ?? 0;
        }

        internal IntegrationPointDesiredState(IntegrationPointProfileTest integrationPointProfileTest)
        {
            ArtifactId = integrationPointProfileTest.ArtifactId;
            Name = integrationPointProfileTest.Name;
            SourceProvider = integrationPointProfileTest.SourceProvider ?? 0;
            DestinationProvider = integrationPointProfileTest.DestinationProvider ?? 0;
        }

        public IntegrationPointDesiredState(IntegrationPointModel integrationPointModel)
        {
            ArtifactId = integrationPointModel.ArtifactId;
            Name = integrationPointModel.Name;
            SourceProvider = integrationPointModel.SourceProvider;
            DestinationProvider = integrationPointModel.DestinationProvider;
        }
    }
}
