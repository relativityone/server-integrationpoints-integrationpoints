using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    public class IntegrationPointProfileManagerTests : TestsBase
    {
        private IIntegrationPointProfileManager _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sut = Container.Resolve<IIntegrationPointProfileManager>();
        }

        [IdentifiedTestCase("A3D65A1A-F453-4DD3-9772-169F69D6FA11", false, true, "a421248620@relativity.com", "Use Field Settings", ImportOverwriteModeEnum.OverlayOnly)]
        public async Task CreateIntegrationPointProfileAsync_ShouldReturnCorrectIntegrationPointModel(bool importNativeFile, bool logErrors,
            string emailNotificationRecipients, string fieldOverlayBehavior, ImportOverwriteModeEnum overwriteMode)
        {
            //Arrange
            var test = SourceWorkspace.IntegrationPointProfiles;
            CreateIntegrationPointRequest request = GetRequestForIntegrationPointProfile(importNativeFile, logErrors, emailNotificationRecipients, fieldOverlayBehavior, overwriteMode);

            //Act
            IntegrationPointModel result = await _sut.CreateIntegrationPointProfileAsync(request);

            //Assert
            IntegrationPointProfileTest expectedProfile = SourceWorkspace.IntegrationPointProfiles.Where(x => x.ArtifactId == result.ArtifactId).FirstOrDefault();
            expectedProfile.Should().NotBeNull();
            AssertIntegrationPointProfile(request.IntegrationPoint, expectedProfile);
            AssertIntegrationPointProfile(result, expectedProfile);
        }

        private void AssertIntegrationPointProfile(IntegrationPointModel integrationPoint, IntegrationPointProfileTest expectedProfile)
        {
            expectedProfile.SourceProvider.Should().Be(integrationPoint.SourceProvider);
            expectedProfile.DestinationProvider.Should().Be(integrationPoint.DestinationProvider);
            expectedProfile.EmailNotificationRecipients.Should().BeEquivalentTo(integrationPoint.EmailNotificationRecipients);
            expectedProfile.Type.Should().Be(integrationPoint.Type);
            expectedProfile.LogErrors.Should().Be(integrationPoint.LogErrors);
            expectedProfile.SourceConfiguration.Should().BeEquivalentTo((string)integrationPoint.SourceConfiguration);
            expectedProfile.DestinationConfiguration.Should().BeEquivalentTo((string)integrationPoint.DestinationConfiguration);
        }

        private CreateIntegrationPointRequest GetRequestForIntegrationPointProfile(bool importNativeFile, bool logErrors, string emailNotificationRecipients,
            string fieldOverlayBehavior, ImportOverwriteModeEnum overwriteMode)
        {
            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace(destinationWorkspaceArtifactId);
            IntegrationPointModel integrationPointModel = CreateIntegrationPointModel(destinationWorkspace, importNativeFile, logErrors, emailNotificationRecipients, fieldOverlayBehavior, overwriteMode);
            return new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                IntegrationPoint = integrationPointModel
            };
        }

        private IntegrationPointModel CreateIntegrationPointModel(WorkspaceTest destinationWorkspace, bool importNativeFile, bool logErrors, string emailNotificationRecipients,
            string fieldOverlayBehavior, ImportOverwriteModeEnum overwriteMode)
        {
            IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(destinationWorkspace);
            List<FieldMap> fieldMap = GetFieldMapping(destinationWorkspace, (int)ArtifactType.Document);

            RelativityProviderDestinationConfiguration destinationConfig = new RelativityProviderDestinationConfiguration
            {
                ArtifactTypeID = (int)ArtifactType.Document,
                CaseArtifactId = destinationWorkspace.ArtifactId,
                ImportNativeFile = importNativeFile,
                UseFolderPathInformation = false,
                FolderPathSourceField = 0,
                FieldOverlayBehavior = fieldOverlayBehavior,
                DestinationFolderArtifactId = destinationWorkspace.Folders.First().ArtifactId
            };
            RelativityProviderSourceConfiguration sourceConfig = new RelativityProviderSourceConfiguration
            {
                SourceWorkspaceArtifactId = SourceWorkspace.ArtifactId,
                SavedSearchArtifactId = SourceWorkspace.SavedSearches.First().ArtifactId,
                TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch
            };

            IntegrationPointModel integrationPointModel = new IntegrationPointModel
            {
                Name = integrationPoint.Name,
                ArtifactId = integrationPoint.ArtifactId,
                OverwriteFieldsChoiceId = Const.Choices.OverwriteFields.Where(x => x.Name == overwriteMode.GetDescription()).SingleOrDefault().ArtifactID,
                SourceProvider = integrationPoint.SourceProvider.GetValueOrDefault(0),
                DestinationConfiguration = destinationConfig,
                SourceConfiguration = sourceConfig,
                DestinationProvider = integrationPoint.DestinationProvider.GetValueOrDefault(0),
                Type = integrationPoint.Type.GetValueOrDefault(0),
                EmailNotificationRecipients = emailNotificationRecipients,
                LogErrors = logErrors,
                ScheduleRule = new ScheduleModel
                {
                    EnableScheduler = false
                },
                FieldMappings = fieldMap
            };

            return integrationPointModel;
        }

        public List<FieldMap> GetFieldMapping(WorkspaceTest destinationWorkspace, int artifactTypeId)
        {
            FieldTest sourceIdentifier = SourceWorkspace.Fields.First(x => x.ObjectTypeId == artifactTypeId && x.IsIdentifier);

            FieldTest destinationIdentifier = destinationWorkspace.Fields.First(x => x.ObjectTypeId == artifactTypeId && x.IsIdentifier);

            return new List<FieldMap>
            {
                new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        DisplayName = sourceIdentifier.Name,
                        FieldIdentifier = sourceIdentifier.ArtifactId.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = true,
                        IsRequired = true,
                        Type = ""
                    },
                    DestinationField = new FieldEntry
                    {
                        DisplayName = destinationIdentifier.Name,
                        FieldIdentifier = destinationIdentifier.ArtifactId.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = true,
                        IsRequired = true,
                        Type = ""
                    },
                    FieldMapType = FieldMapType.Identifier
                }
            };
        }
    }

}
