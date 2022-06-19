using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    public class IntegrationPointProfileManagerTests : TestsBase
    {
        private IIntegrationPointProfileManager _sut;
        private WorkspaceTest _destinationWorkspace;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sut = Container.Resolve<IIntegrationPointProfileManager>();
            PrepareDestinationWorkspace();
        }

        [IdentifiedTestCase("A3D65A1A-F453-4DD3-9772-169F69D6FA11", false, true, "email@email.com", Const.FieldOverlayBehaviorName.USE_FIELD_SETTINGS, ImportOverwriteModeEnum.AppendOnly)]
        [IdentifiedTestCase("6FBF4847-587C-46F3-945C-325D5AECC441", true, false, "", Const.FieldOverlayBehaviorName.REPLACE_VALUES, ImportOverwriteModeEnum.OverlayOnly)]
        [IdentifiedTestCase("109F0C6E-6A57-449B-B0CD-700017633865", false, false, "", Const.FieldOverlayBehaviorName.USE_FIELD_SETTINGS, ImportOverwriteModeEnum.AppendOverlay)]
        public async Task CreateIntegrationPointProfileAsync_ShouldReturnCorrectProfile(bool importNativeFile, bool logErrors,
            string emailNotificationRecipients, string fieldOverlayBehavior, ImportOverwriteModeEnum overwriteMode)
        {
            //Arrange         
            CreateIntegrationPointRequest request = SetUpInitialDataAndGetRequest(RequestType.Create, importNativeFile, logErrors, emailNotificationRecipients, fieldOverlayBehavior, overwriteMode);

            //Act
            IntegrationPointModel result = await _sut.CreateIntegrationPointProfileAsync(request).ConfigureAwait(false);

            //Assert
            IntegrationPointProfileTest testedProfile = SourceWorkspace.IntegrationPointProfiles.Where(x => x.ArtifactId == result.ArtifactId).FirstOrDefault();
            testedProfile.Should().NotBeNull();
            AssertCreatedIntegrationPointProfile(request.IntegrationPoint, testedProfile);
        }

        [IdentifiedTest("6CD0B0CB-0EB2-4291-A22C-4EFFBAC614D6")]
        public async Task CreateIntegrationPointProfileFromIntegrationPointAsync_ShouldReturnCorrectProfile()
        {
            //Arrange         
            CreateIntegrationPointRequest request = SetUpInitialDataAndGetRequest(RequestType.CreateFromIntegrationPoint);
            string profileName = $"Test profile from Integration Point {request.IntegrationPoint.ArtifactId}";
            //Act
            IntegrationPointModel result = await _sut.CreateIntegrationPointProfileFromIntegrationPointAsync(SourceWorkspace.ArtifactId, request.IntegrationPoint.ArtifactId, profileName).ConfigureAwait(false);

            //Assert
            IntegrationPointTest existingIntegrationPoint = SourceWorkspace.IntegrationPoints.Where(x => x.ArtifactId == request.IntegrationPoint.ArtifactId).FirstOrDefault();
            IntegrationPointProfileTest testedProfile = SourceWorkspace.IntegrationPointProfiles.Where(x => x.ArtifactId == result.ArtifactId).FirstOrDefault();

            testedProfile.Should().NotBeNull();
            testedProfile.Name.Should().Be(profileName);
            testedProfile.SourceProvider.Should().Be(existingIntegrationPoint.SourceProvider);
            testedProfile.DestinationProvider.Should().Be(existingIntegrationPoint.DestinationProvider);
            testedProfile.EmailNotificationRecipients.Should().BeEquivalentTo(existingIntegrationPoint.EmailNotificationRecipients);
            testedProfile.Type.Should().Be(existingIntegrationPoint.Type);
            testedProfile.LogErrors.Should().Be(existingIntegrationPoint.LogErrors);           
            Const.Choices.OverwriteFields.Where(x => x.ArtifactID == testedProfile.OverwriteFields.ArtifactID).FirstOrDefault().Name.Should().Be(existingIntegrationPoint.OverwriteFields.Name);            
            existingIntegrationPoint.DestinationConfiguration.Should().BeEquivalentTo(testedProfile.DestinationConfiguration);
        }

        [IdentifiedTest("EC4158B6-785B-42BD-9779-CA8851F6CA03")]
        public async Task UpdateIntegrationPointProfileAsync_ShouldChangeProfileCorrectly()
        {
            //Arrange
            CreateIntegrationPointRequest request = SetUpInitialDataAndGetRequest(RequestType.Update, importNativeFile: false,
                logErrors: true, emailNotificationRecipients: "test@test.com", fieldOverlayBehavior: Const.FieldOverlayBehaviorName.REPLACE_VALUES, overwriteMode: ImportOverwriteModeEnum.OverlayOnly);

            //Act
            IntegrationPointModel result = await _sut.UpdateIntegrationPointProfileAsync(request).ConfigureAwait(false);

            //Assert
            IntegrationPointProfileTest testedProfile = SourceWorkspace.IntegrationPointProfiles.Where(x => x.ArtifactId == result.ArtifactId).FirstOrDefault();
            testedProfile.Should().NotBeNull();
            AssertCreatedIntegrationPointProfile(request.IntegrationPoint, testedProfile);
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
            IntegrationPointProfileTest expectedProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(_destinationWorkspace);

            //Act
            IntegrationPointModel result = await _sut.GetIntegrationPointProfileAsync(SourceWorkspace.ArtifactId, expectedProfile.ArtifactId).ConfigureAwait(false);

            //Assert
            result.Should().NotBeNull();
            AssertObtainedIntegrationPointProfile(result, expectedProfile);
        }

        [IdentifiedTest("6DD5E1C4-F767-4BBB-B336-56216464846F")]
        public async Task GetAllIntegrationPointProfilesAsync_ShouldReturnCorrectObjectSet()
        {
            //Arrange           
            List<IntegrationPointProfileTest> expectedProfiles = new List<IntegrationPointProfileTest>();
            expectedProfiles.Add(SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(_destinationWorkspace));
            expectedProfiles.Add(SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(_destinationWorkspace));

            //Act
            IList<IntegrationPointModel> results = await _sut.GetAllIntegrationPointProfilesAsync(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            //Assert
            results.Should().NotBeNull();
            results.Should().NotBeEmpty();
            results.Should().HaveSameCount(expectedProfiles);
            foreach (var result in results)
            {
                AssertObtainedIntegrationPointProfile(result, expectedProfiles.Single(x => x.ArtifactId == result.ArtifactId));
            }
        }

        private CreateIntegrationPointRequest SetUpInitialDataAndGetRequest(RequestType requestType, bool importNativeFile = false, bool logErrors = true, string emailNotificationRecipients = "",
            string fieldOverlayBehavior = Const.FieldOverlayBehaviorName.USE_FIELD_SETTINGS, ImportOverwriteModeEnum overwriteMode = ImportOverwriteModeEnum.AppendOnly)
        {
            string name = string.Empty;
            int artifactId = 0;
            int sourceProviderId = 0;
            int destinationProviderId = 0;
            int type = 0;

            switch (requestType)
            {
                case RequestType.Create:
                    artifactId = ArtifactProvider.NextId();
                    name = $"Integration Point {artifactId}";
                    sourceProviderId = SourceWorkspace.SourceProviders.Where(x => x.Name == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY_NAME).Single().ArtifactId;
                    destinationProviderId = SourceWorkspace.DestinationProviders.Where(x => x.Name == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY_NAME).Single().ArtifactId;
                    type = SourceWorkspace.IntegrationPointTypes.First(x =>
                x.Identifier == kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()).ArtifactId;
                    break;
                case RequestType.CreateFromIntegrationPoint:
                    IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateSavedSearchSyncIntegrationPoint(_destinationWorkspace);
                    artifactId = integrationPoint.ArtifactId;
                    name = integrationPoint.Name;
                    sourceProviderId = (int)integrationPoint.SourceProvider;
                    destinationProviderId = (int)integrationPoint.DestinationProvider;
                    type = (int)integrationPoint.Type;
                    emailNotificationRecipients = integrationPoint.EmailNotificationRecipients;
                    logErrors = (bool)integrationPoint.LogErrors;
                    break;
                case RequestType.Update:
                    IntegrationPointProfileTest integrationPointProfile = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPointProfile(_destinationWorkspace);
                    artifactId = integrationPointProfile.ArtifactId;
                    name = integrationPointProfile.Name;
                    sourceProviderId = (int)integrationPointProfile.SourceProvider;
                    destinationProviderId = (int)integrationPointProfile.DestinationProvider;
                    type = (int)integrationPointProfile.Type;
                    break;
            }

            RelativityProviderDestinationConfiguration destinationConfig = new RelativityProviderDestinationConfiguration
            {
                ArtifactTypeID = (int)ArtifactType.Document,
                CaseArtifactId = _destinationWorkspace.ArtifactId,
                ImportNativeFile = importNativeFile,
                UseFolderPathInformation = false,
                FolderPathSourceField = 0,
                FieldOverlayBehavior = fieldOverlayBehavior,
                DestinationFolderArtifactId = _destinationWorkspace.Folders.First().ArtifactId
            };
            RelativityProviderSourceConfiguration sourceConfig = new RelativityProviderSourceConfiguration
            {
                SourceWorkspaceArtifactId = SourceWorkspace.ArtifactId,
                SavedSearchArtifactId = SourceWorkspace.SavedSearches.First().ArtifactId,
                TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch
            };

            IntegrationPointModel integrationPointModel = new IntegrationPointModel
            {
                Name = name,
                ArtifactId = artifactId,
                OverwriteFieldsChoiceId = Const.Choices.OverwriteFields.Where(x => x.Name == overwriteMode.GetDescription()).SingleOrDefault().ArtifactID,
                SourceProvider = sourceProviderId,
                DestinationConfiguration = destinationConfig,
                SourceConfiguration = sourceConfig,
                DestinationProvider = destinationProviderId,
                Type = type,
                EmailNotificationRecipients = emailNotificationRecipients,
                LogErrors = logErrors,
                ScheduleRule = new ScheduleModel
                {
                    EnableScheduler = false
                },
                FieldMappings = GetFieldMapping((int)ArtifactType.Document)
            };

            return new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = SourceWorkspace.ArtifactId,
                IntegrationPoint = integrationPointModel
            };
        }

        private void AssertObtainedIntegrationPointProfile(IntegrationPointModel integrationPointModel, IntegrationPointProfileTest expectedProfile)
        {
            expectedProfile.ArtifactId.Should().Be(integrationPointModel.ArtifactId);
            expectedProfile.Name.Should().Be(integrationPointModel.Name);
            expectedProfile.SourceProvider.Should().Be(integrationPointModel.SourceProvider);
            expectedProfile.DestinationProvider.Should().Be(integrationPointModel.DestinationProvider);
        }

        private void AssertCreatedIntegrationPointProfile(IntegrationPointModel integrationPoint, IntegrationPointProfileTest expectedProfile)
        {
            expectedProfile.SourceProvider.Should().Be(integrationPoint.SourceProvider);
            expectedProfile.DestinationProvider.Should().Be(integrationPoint.DestinationProvider);
            expectedProfile.EmailNotificationRecipients.Should().BeEquivalentTo(integrationPoint.EmailNotificationRecipients);
            expectedProfile.Type.Should().Be(integrationPoint.Type);
            expectedProfile.LogErrors.Should().Be(integrationPoint.LogErrors);
            expectedProfile.OverwriteFields.ArtifactID = integrationPoint.OverwriteFieldsChoiceId;

            if (!string.IsNullOrEmpty(expectedProfile.DestinationConfiguration))
            {
                RelativityProviderDestinationConfiguration expectedProfileDestinationConfig = Serializer.Deserialize<RelativityProviderDestinationConfiguration>(expectedProfile.DestinationConfiguration);
                expectedProfileDestinationConfig.ArtifactTypeID.Should().Be(((RelativityProviderDestinationConfigurationBackwardCompatibility)integrationPoint.DestinationConfiguration).ArtifactTypeID);
                expectedProfileDestinationConfig.CaseArtifactId.Should().Be(((RelativityProviderDestinationConfigurationBackwardCompatibility)integrationPoint.DestinationConfiguration).CaseArtifactId);
                expectedProfileDestinationConfig.ImportNativeFile.Should().Be(((RelativityProviderDestinationConfigurationBackwardCompatibility)integrationPoint.DestinationConfiguration).ImportNativeFile);
                expectedProfileDestinationConfig.UseFolderPathInformation.Should().Be(((RelativityProviderDestinationConfigurationBackwardCompatibility)integrationPoint.DestinationConfiguration).UseFolderPathInformation);
                expectedProfileDestinationConfig.FolderPathSourceField.Should().Be(((RelativityProviderDestinationConfigurationBackwardCompatibility)integrationPoint.DestinationConfiguration).FolderPathSourceField);
                expectedProfileDestinationConfig.FieldOverlayBehavior.Should().Be(((RelativityProviderDestinationConfigurationBackwardCompatibility)integrationPoint.DestinationConfiguration).FieldOverlayBehavior);
                expectedProfileDestinationConfig.DestinationFolderArtifactId.Should().Be(((RelativityProviderDestinationConfigurationBackwardCompatibility)integrationPoint.DestinationConfiguration).DestinationFolderArtifactId);
            }
        }

        private void PrepareDestinationWorkspace()
        {
            int destinationWorkspaceArtifactId = ArtifactProvider.NextId();
            _destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace(destinationWorkspaceArtifactId);
        }

        private List<FieldMap> GetFieldMapping(int artifactTypeId)
        {
            FieldTest sourceIdentifier = SourceWorkspace.Fields.First(x => x.ObjectTypeId == artifactTypeId && x.IsIdentifier);
            FieldTest destinationIdentifier = _destinationWorkspace.Fields.First(x => x.ObjectTypeId == artifactTypeId && x.IsIdentifier);

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

        private enum RequestType
        {
            Create,
            CreateFromIntegrationPoint,
            Update
        }
    }
}


