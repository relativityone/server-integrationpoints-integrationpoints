using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.Services.Choice;

namespace Relativity.IntegrationPoints.Services.Tests.Repositories
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointProfileRepositoryTests : TestBase
    {
        private IntegrationPointProfileRepository _integrationPointProfileRepository;
        private IIntegrationPointProfileService _integrationPointProfileService;
        private IIntegrationPointService _integrationPointService;
        private IChoiceQuery _choiceQuery;
        private IBackwardCompatibility _backwardCompatibility;
        private ICaseServiceContext _caseContext;

        private int _workspaceArtifactId = 100;

        public override void SetUp()
        {
            _integrationPointProfileService = Substitute.For<IIntegrationPointProfileService>();
            _integrationPointService = Substitute.For<IIntegrationPointService>();
            _choiceQuery = Substitute.For<IChoiceQuery>();
            _backwardCompatibility = Substitute.For<IBackwardCompatibility>();
            
            _caseContext = Substitute.For<ICaseServiceContext>();
            _caseContext.WorkspaceID.Returns(_workspaceArtifactId);

            _integrationPointProfileRepository = new IntegrationPointProfileRepository(_backwardCompatibility, _integrationPointProfileService, _choiceQuery,
                _integrationPointService, _caseContext);
        }

        [Test]
        public void ItShouldCreateIntegrationPoint()
        {
            var overwriteFieldsChoiceId = 588;
            var overwriteFieldsChoiceName = "Append/Overlay";
            var integrationPointProfileArtifactId = 219;

            var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointProfileArtifactId);

            _integrationPointProfileService.SaveIntegration(Arg.Is<IntegrationPointProfileModel>(x => x.ArtifactID == 0)).Returns(integrationPointProfileArtifactId);

            _integrationPointProfileRepository.CreateIntegrationPointProfile(createRequest);

            _backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
            _integrationPointProfileService.Received(1).ReadIntegrationPointProfile(integrationPointProfileArtifactId);
        }

        [Test]
        public void ItShouldUpdateIntegrationPoint()
        {
            var overwriteFieldsChoiceId = 152;
            var overwriteFieldsChoiceName = "Append/Overlay";
            var integrationPointProfileArtifactId = 289;

            var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointProfileArtifactId);

            _integrationPointProfileService.SaveIntegration(Arg.Is<IntegrationPointProfileModel>(x => x.ArtifactID == createRequest.IntegrationPoint.ArtifactId))
                .Returns(integrationPointProfileArtifactId);

            _integrationPointProfileRepository.CreateIntegrationPointProfile(createRequest);

            _backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
            _integrationPointProfileService.Received(1).ReadIntegrationPointProfile(integrationPointProfileArtifactId);
        }

        private UpdateIntegrationPointRequest SetUpCreateOrUpdateTest(int overwriteFieldsChoiceId, string overwriteFieldsChoiceName, int integrationPointProfileArtifactId)
        {
            var createRequest = new UpdateIntegrationPointRequest
            {
                WorkspaceArtifactId = 921,
                IntegrationPoint = new IntegrationPointModel
                {
                    OverwriteFieldsChoiceId = overwriteFieldsChoiceId,
                    ArtifactId = 591,
                    ScheduleRule = new ScheduleModel
                    {
                        EnableScheduler = false
                    }
                }
            };

            _choiceQuery.GetChoicesOnField(_workspaceArtifactId, new Guid(IntegrationPointProfileFieldGuids.OverwriteFields)).Returns(new List<ChoiceRef>
            {
                new ChoiceRef(overwriteFieldsChoiceId)
                {
                    Name = overwriteFieldsChoiceName
                }
            });
            _integrationPointProfileService.ReadIntegrationPointProfile(integrationPointProfileArtifactId).Returns(new IntegrationPointProfile
            {
                ArtifactId = integrationPointProfileArtifactId,
                Name = "name_982",
                SourceProvider = 268,
                DestinationProvider = 288
            });
            return createRequest;
        }

        [Test]
        public void ItShouldGetIntegrationPoint()
        {
            int artifactId = 924;
            var integrationPointProfile = new IntegrationPointProfile
            {
                ArtifactId = 235,
                Name = "ip_name_350",
                SourceProvider = 471,
                DestinationProvider = 817
            };

            _integrationPointProfileService.ReadIntegrationPointProfile(artifactId).Returns(integrationPointProfile);

            var result = _integrationPointProfileRepository.GetIntegrationPointProfile(artifactId);

            _integrationPointProfileService.Received(1).ReadIntegrationPointProfile(artifactId);

            Assert.That(result.SourceProvider, Is.EqualTo(integrationPointProfile.SourceProvider));
            Assert.That(result.DestinationProvider, Is.EqualTo(integrationPointProfile.DestinationProvider));
            Assert.That(result.ArtifactId, Is.EqualTo(integrationPointProfile.ArtifactId));
            Assert.That(result.Name, Is.EqualTo(integrationPointProfile.Name));
        }

        [Test]
        public void ItShouldGetAllIntegrationPoints()
        {
            var integrationPointProfile1 = new IntegrationPointProfile
            {
                ArtifactId = 844,
                Name = "ip_name_234",
                SourceProvider = 871,
                DestinationProvider = 143
            };
            var integrationPointProfile2 = new IntegrationPointProfile
            {
                ArtifactId = 526,
                Name = "ip_name_617",
                SourceProvider = 723,
                DestinationProvider = 158
            };

            var expectedResult = new List<IntegrationPointProfile> {integrationPointProfile1, integrationPointProfile2};
            _integrationPointProfileService.GetAllRDOs().Returns(expectedResult);

            var result = _integrationPointProfileRepository.GetAllIntegrationPointProfiles();

            _integrationPointProfileService.Received(1).GetAllRDOs();

            Assert.That(result, Is.EquivalentTo(expectedResult).
                Using(new Func<IntegrationPointModel, IntegrationPointProfile, bool>(
                    (actual, expected) => (actual.Name == expected.Name) && (actual.SourceProvider == expected.SourceProvider.Value) && (actual.ArtifactId == expected.ArtifactId)
                                        && (actual.DestinationProvider == expected.DestinationProvider.Value))));
        }


        [Test]
        public void ItShouldRetrieveAllOverwriteFieldChoices()
        {
            var expectedChoices = new List<ChoiceRef>
            {
                new ChoiceRef(688)
                {
                    Name = "name_516"
                },
                new ChoiceRef(498)
                {
                    Name = "name_712"
                }
            };

            _choiceQuery.GetChoicesOnField(_workspaceArtifactId, Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields)).Returns(expectedChoices);

            var actualChoicesModels = _integrationPointProfileRepository.GetOverwriteFieldChoices();

            Assert.That(actualChoicesModels,
                Is.EquivalentTo(expectedChoices).Using(new Func<OverwriteFieldsModel, ChoiceRef, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactID))));
        }

        [Test]
        public void ItShouldCreateProfileBasedOnIntegrationPoint()
        {
            int integrationPointArtifactId = 988586;
            string profileName = "profile_name_100";
            int artifactId = 984783;

            var integrationPoint = new IntegrationPoint
            {
                OverwriteFields = new ChoiceRef(271635),
                SourceProvider = 284,
                DestinationConfiguration = "975426",
                SourceConfiguration = "559417",
                DestinationProvider = 346,
                Type = 190,
                EnableScheduler = false,
                ScheduleRule = string.Empty,
                EmailNotificationRecipients = "432824",
                LogErrors = false,
                NextScheduledRuntimeUTC = DateTime.MaxValue,
                FieldMappings = "159339",
                Name = "ip_346",
                SecuredConfiguration = "{}"
            };
            var integrationPointProfile = new IntegrationPointProfile
            {
                Name = "profile_990",
                SourceProvider = 451,
                DestinationProvider = 443
            };

            _integrationPointService.ReadIntegrationPoint(integrationPointArtifactId).Returns(integrationPoint);
            _integrationPointProfileService.SaveIntegration(Arg.Any<IntegrationPointProfileModel>()).Returns(artifactId);
            _integrationPointProfileService.ReadIntegrationPointProfile(artifactId).Returns(integrationPointProfile);

            _integrationPointProfileRepository.CreateIntegrationPointProfileFromIntegrationPoint(integrationPointArtifactId, profileName);

            _integrationPointService.Received(1).ReadIntegrationPoint(integrationPointArtifactId);
            _integrationPointProfileService.Received(1).SaveIntegration(Arg.Is<IntegrationPointProfileModel>(x => x.Name == profileName && x.ArtifactID == 0));
            _integrationPointProfileService.Received(1).ReadIntegrationPointProfile(artifactId);
        }
    }
}