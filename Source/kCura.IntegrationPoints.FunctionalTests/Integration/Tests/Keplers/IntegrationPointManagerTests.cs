using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
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
        public async Task CreateIntegrationPointAsync_ShouldPass()
        {
            // Arrange
            //SourceWorkspace.
            SourceWorkspace.Fields.Add(new FieldTest
            {
                Guid = IntegrationPointFieldGuids.OverwriteFieldsGuid,
                Artifact = { ArtifactId = Const.OVERWRITE_FIELD_ARTIFACT_ID }
            });
            SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(SourceWorkspace);
            IntegrationPointTest integrationPoint = SourceWorkspace.IntegrationPoints[SourceWorkspace.IntegrationPoints.Count - 1];

            CreateIntegrationPointRequest request = new CreateIntegrationPointRequest
            {
                IntegrationPoint = new IntegrationPointModel
                {
                    ArtifactId = integrationPoint.ArtifactId,
                    Name = integrationPoint.Name,
                    SourceConfiguration = integrationPoint.SourceConfiguration,
                    DestinationConfiguration = integrationPoint.DestinationConfiguration,
                    SourceProvider = (int)integrationPoint.SourceProvider,
                    DestinationProvider = (int)integrationPoint.DestinationProvider,
                    FieldMappings = JsonConvert.DeserializeObject<List<FieldMap>>(integrationPoint.FieldMappings),
                    ImportFileCopyMode = ImportFileCopyModeEnum.DoNotImportNativeFiles,
                    LogErrors = true,
                    EmailNotificationRecipients = "",
                    OverwriteFieldsChoiceId = Const.OVERWRITE_FIELD_ARTIFACT_ID,
                    Type = (int)integrationPoint.Type
                },
                WorkspaceArtifactId = SourceWorkspace.ArtifactId
            };

            // Act
            IntegrationPointModel integrationPointModel = await _sut.CreateIntegrationPointAsync(request);

            // Assert
        }
    }
}
