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

        [IdentifiedTestCase("A3D65A1A-F453-4DD3-9772-169F69D6FA11", false, true, "email@email.com", "Use Field Settings", ImportOverwriteModeEnum.AppendOnly)]
        [IdentifiedTestCase("6FBF4847-587C-46F3-945C-325D5AECC441", true, false, "", "Replace Values", ImportOverwriteModeEnum.OverlayOnly)]
        [IdentifiedTestCase("109F0C6E-6A57-449B-B0CD-700017633865", false, false, "", "Use Field Settings", ImportOverwriteModeEnum.AppendOverlay)]
        public async Task CreateIntegrationPointProfileAsync_ShouldReturnCorrectProfile(bool importNativeFile, bool logErrors,
            string emailNotificationRecipients, string fieldOverlayBehavior, ImportOverwriteModeEnum overwriteMode)
        {
            //Arrange         
            CreateIntegrationPointRequest request = GetRequestForIntegrationPointProfile(importNativeFile, logErrors, emailNotificationRecipients, fieldOverlayBehavior, overwriteMode);

            //Act
            IntegrationPointModel result = await _sut.CreateIntegrationPointProfileAsync(request).ConfigureAwait(false);

            //Assert
            IntegrationPointProfileTest expectedProfile = SourceWorkspace.IntegrationPointProfiles.Where(x => x.ArtifactId == result.ArtifactId).FirstOrDefault();
            expectedProfile.Should().NotBeNull();
            AssertIntegrationPointProfile(request.IntegrationPoint, expectedProfile);
            AssertIntegrationPointProfile(result, expectedProfile);
        }

        [IdentifiedTest("6CD0B0CB-0EB2-4291-A22C-4EFFBAC614D6")]
        public async Task CreateIntegrationPointProfileFromIntegrationPointAsync_ShouldReturnCorrectProfile()
        {
            //Arrange         
            CreateIntegrationPointRequest request = GetRequestForIntegrationPointProfile(importNativeFile: false,
                logErrors: true, emailNotificationRecipients: string.Empty, fieldOverlayBehavior: "Use Field Settings", ImportOverwriteModeEnum.AppendOverlay);
            
            string profileName = $"Test profile from Integration Point {request.IntegrationPoint.ArtifactId}";
            //Act
            IntegrationPointModel result = await _sut.CreateIntegrationPointProfileFromIntegrationPointAsync(SourceWorkspace.ArtifactId, request.IntegrationPoint.ArtifactId, profileName).ConfigureAwait(false);

            //Assert
            IntegrationPointProfileTest expectedProfile = SourceWorkspace.IntegrationPointProfiles.Where(x => x.ArtifactId == result.ArtifactId).FirstOrDefault();
            expectedProfile.Should().NotBeNull();
            AssertIntegrationPointProfile(request.IntegrationPoint, expectedProfile);
            AssertIntegrationPointProfile(result, expectedProfile);
        }


        [IdentifiedTest("C994A507-5495-42D3-BE10-62A56B971C78")]
        public async Task GetOverwriteFieldsChoicesAsync_ShouldReturnCorrectSetOfValues()
        {
            //Arrange
            List<Relativity.Services.ChoiceQuery.Choice> expectedChoices = Const.Choices.OverwriteFields;
            //Act
            IList<OverwriteFieldsModel> results = await _sut.GetOverwriteFieldsChoicesAsync(SourceWorkspace.ArtifactId).ConfigureAwait(false);
            //Assert
            results.Should().NotBeNull();
            results.Should().HaveSameCount(expectedChoices);

            results.Select(x => x.ArtifactId)
                .All(x => expectedChoices.Select(y => y.ArtifactID).Contains(x))
                .Should().BeTrue();

            results.Select(x => x.Name)
                .All(x => expectedChoices.Select(y => y.Name).Contains(x))
                .Should().BeTrue();
        }

        [IdentifiedTest("BB68F9F2-A8BD-49D6-AAF7-0AFBE748F24A")]
        public async Task GetIntegrationPointProfileAsync_ShouldReturnCorrectObject()
        {
            //Arrange
            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace(destinationWorkspaceArtifactId);
            IntegrationPointProfileTest expectedProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(destinationWorkspace);

            //Act
            IntegrationPointModel result = await _sut.GetIntegrationPointProfileAsync(SourceWorkspace.ArtifactId, expectedProfile.ArtifactId).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            AssertIntegrationPointProfile(result, expectedProfile);
        }

        [IdentifiedTest("6DD5E1C4-F767-4BBB-B336-56216464846F")]
        public async Task GetAllIntegrationPointProfilesAsync_ShouldReturnCorrectObjectSet()
        {
            //Arrange
            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace(destinationWorkspaceArtifactId);
            List<IntegrationPointProfileTest> expectedProfiles = new List<IntegrationPointProfileTest>();
            expectedProfiles.Add(SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateEmptyIntegrationPointProfile());
            expectedProfiles.Add(SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(destinationWorkspace));
            
            //Act
            IList<IntegrationPointModel> results = await _sut.GetAllIntegrationPointProfilesAsync(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            //Assert
            results.Should().NotBeNull();
            results.Should().NotBeEmpty();
            results.Should().HaveSameCount(expectedProfiles);
            foreach(var result in results)
            {
                AssertIntegrationPointProfile(result, expectedProfiles.Single(x => x.ArtifactId == result.ArtifactId));
            }            
        }

        private void AssertIntegrationPointProfile(IntegrationPointModel integrationPoint, IntegrationPointProfileTest expectedProfile)
        {
            expectedProfile.SourceProvider.Should().Be(integrationPoint.SourceProvider);
            expectedProfile.DestinationProvider.Should().Be(integrationPoint.DestinationProvider);
            expectedProfile.EmailNotificationRecipients.Should().BeEquivalentTo(integrationPoint.EmailNotificationRecipients);
            expectedProfile.Type.Should().Be(integrationPoint.Type);
            expectedProfile.LogErrors.Should().Be(integrationPoint.LogErrors);
            //assert destination config
            //assert source config
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
