using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.Services.Choice;

namespace Relativity.IntegrationPoints.Services.Tests.Repositories
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointRepositoryTests : TestBase
    {
        private IIntegrationPointSerializer _serializer;
        private IObjectTypeRepository _objectTypeRepository;
        private IUserInfo _userInfo;
        private IChoiceQuery _choiceQuery;
        private IBackwardCompatibility _backwardCompatibility;
        private IIntegrationPointService _integrationPointLocalService;
        private IIntegrationPointProfileService _integrationPointProfileService;

        private Services.Repositories.IIntegrationPointRepository _integrationPointRepository;
        private IIntegrationPointService _integrationPointService;

        private DestinationConfiguration _destinationConfiguration;
        private string _serializedDestinationConfiguration;
        private ICaseServiceContext _caseContext;

        private int _workspaceArtifactId = 100;

        public override void SetUp()
        {
            IIntegrationPointRuntimeServiceFactory serviceFactory = Substitute.For<IIntegrationPointRuntimeServiceFactory>();
            _serializer = Substitute.For<IIntegrationPointSerializer>();
            _integrationPointLocalService = Substitute.For<IIntegrationPointService>();
            _integrationPointProfileService = Substitute.For<IIntegrationPointProfileService>();
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();
            _userInfo = Substitute.For<IUserInfo>();
            _choiceQuery = Substitute.For<IChoiceQuery>();
            _backwardCompatibility = Substitute.For<IBackwardCompatibility>();
            
            _caseContext = Substitute.For<ICaseServiceContext>();
            _caseContext.WorkspaceID.Returns(_workspaceArtifactId);

            _integrationPointRepository = new IntegrationPointRepository(serviceFactory, _objectTypeRepository, _userInfo, _choiceQuery,
                _backwardCompatibility, _integrationPointLocalService, _integrationPointProfileService, _caseContext);

            _integrationPointService = Substitute.For<IIntegrationPointService>();

            serviceFactory.CreateIntegrationPointRuntimeService(Arg.Any<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>()).Returns(_integrationPointService);
        }

        [Test]
        [TestCase(null)]
        [TestCase(123)]
        public void ItShouldCreateIntegrationPoint(int? federatedInstanceArtifactId)
        {
            var overwriteFieldsChoiceId = 934;
            var overwriteFieldsChoiceName = "Append/Overlay";
            var integrationPointArtifactId = 134;

            var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName,
                integrationPointArtifactId, federatedInstanceArtifactId);

            _integrationPointService.SaveIntegration(Arg.Is<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>(x => x.ArtifactID == 0))
                .Returns(integrationPointArtifactId);

            _integrationPointRepository.CreateIntegrationPoint(createRequest);

            _backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
            _integrationPointLocalService.Received(1).ReadIntegrationPoint(integrationPointArtifactId);
        }

        [Test]
        [TestCase(null)]
        [TestCase(123)]
        public void ItShouldUpdateIntegrationPoint(int? federatedInstanceArtifactId)
        {
            var overwriteFieldsChoiceId = 215;
            var overwriteFieldsChoiceName = "Append/Overlay";
            var integrationPointArtifactId = 902;

            var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName,
                integrationPointArtifactId, federatedInstanceArtifactId);

            _integrationPointService.SaveIntegration(
                Arg.Is<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>(x => x.ArtifactID == createRequest.IntegrationPoint.ArtifactId))
                .Returns(integrationPointArtifactId);

            _integrationPointRepository.CreateIntegrationPoint(createRequest);

            _backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
            _integrationPointLocalService.Received(1).ReadIntegrationPoint(integrationPointArtifactId);
        }

        private void SetUpDestinationConfiguration(int? federatedInstanceArtifactId = null)
        {
            _destinationConfiguration = new DestinationConfiguration()
            {
                FederatedInstanceArtifactId = federatedInstanceArtifactId
            };
            IAPILog logger = Substitute.For<IAPILog>();
            _serializedDestinationConfiguration = new IntegrationPointSerializer(logger).Serialize(_destinationConfiguration);

            _serializer.Serialize(_destinationConfiguration).Returns(_serializedDestinationConfiguration);
            _serializer.Deserialize<DestinationConfiguration>(_serializedDestinationConfiguration)
                .Returns(_destinationConfiguration);
        }

        private UpdateIntegrationPointRequest SetUpCreateOrUpdateTest(int overwriteFieldsChoiceId, string overwriteFieldsChoiceName, int integrationPointArtifactId, int? federatedInstanceArtifactId = null)
        {
            SetUpGetRdo(integrationPointArtifactId, overwriteFieldsChoiceId, overwriteFieldsChoiceName, federatedInstanceArtifactId);

            var createRequest = new UpdateIntegrationPointRequest
            {
                WorkspaceArtifactId = 640,
                IntegrationPoint = new IntegrationPointModel
                {
                    OverwriteFieldsChoiceId = overwriteFieldsChoiceId,
                    ArtifactId = 916,
                    ScheduleRule = new ScheduleModel
                    {
                        EnableScheduler = false
                    },
                    DestinationConfiguration = _destinationConfiguration,
                    SecuredConfiguration = "{}"
                }
            };

            return createRequest;
        }

        private IntegrationPoint SetUpGetRdo(int integrationPointArtifactId, int overwriteFieldsChoiceId = 123, string overwriteFieldsChoiceName = "choice123", int? federatedInstanceArtifactId = null)
        {
            SetUpDestinationConfiguration(federatedInstanceArtifactId);

            var integrationPoint = CreateRdo(integrationPointArtifactId, overwriteFieldsChoiceId, overwriteFieldsChoiceName);

            _integrationPointLocalService.ReadIntegrationPoint(integrationPointArtifactId).Returns(integrationPoint);

            _choiceQuery.GetChoicesOnField(_workspaceArtifactId, new Guid(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<ChoiceRef>
            {
                new ChoiceRef(overwriteFieldsChoiceId)
                {
                    Name = overwriteFieldsChoiceName
                }
            });

            return integrationPoint;
        }

        private IntegrationPoint CreateRdo(int integrationPointArtifactId, int overwriteFieldsChoiceId = 123, string overwriteFieldsChoiceName = "choice123")
        {
            return new IntegrationPoint
            {
                ArtifactId = integrationPointArtifactId,
                Name = "name_762",
                DestinationConfiguration = _serializedDestinationConfiguration,
                DestinationProvider = 715,
                EmailNotificationRecipients = "emails",
                EnableScheduler = false,
                FieldMappings = "",
                HasErrors = false,
                JobHistory = null,
                LastRuntimeUTC = null,
                LogErrors = false,
                SourceProvider = 718,
                SourceConfiguration = "",
                NextScheduledRuntimeUTC = null,
                OverwriteFields = new ChoiceRef(overwriteFieldsChoiceId) {Name = overwriteFieldsChoiceName},
                ScheduleRule = String.Empty,
                Type = null,
                SecuredConfiguration = string.Empty
            };
        }

        [Test]
        public void ItShouldGetIntegrationPoint()
        {
            int artifactId = 884;
            var integrationPoint = CreateRdo(artifactId);

            _integrationPointLocalService.ReadIntegrationPoint(artifactId).Returns(integrationPoint);

            var result = _integrationPointRepository.GetIntegrationPoint(artifactId);

            _integrationPointLocalService.Received(1).ReadIntegrationPoint(artifactId);

            Assert.That(result.SourceProvider, Is.EqualTo(integrationPoint.SourceProvider));
            Assert.That(result.DestinationProvider, Is.EqualTo(integrationPoint.DestinationProvider));
            Assert.That(result.ArtifactId, Is.EqualTo(integrationPoint.ArtifactId));
            Assert.That(result.Name, Is.EqualTo(integrationPoint.Name));
        }

        [Test]
        [TestCase(null)]
        [TestCase(123)]
        public void ItShouldRunIntegrationPointWithUser(int? federatedInstanceArtifactId)
        {
            int workspaceId = 873;
            int artifactId = 797;
            int userId = 127;

            _userInfo.ArtifactID.Returns(userId);

            SetUpGetRdo(artifactId, 456, "choice456", federatedInstanceArtifactId);

            _integrationPointRepository.RunIntegrationPoint(workspaceId, artifactId);

            _integrationPointService.Received(1).RunIntegrationPoint(workspaceId, artifactId, userId);
        }

        [Test]
        public void ItShouldGetAllIntegrationPoints()
        {
            var integrationPoint1 = CreateRdo(263);
            var integrationPoint2 = CreateRdo(204);

            var expectedResult = new List<IntegrationPoint> {integrationPoint1, integrationPoint2};
            _integrationPointLocalService.GetAllRDOs().Returns(expectedResult);

            var result = _integrationPointRepository.GetAllIntegrationPoints();

            _integrationPointLocalService.Received(1).GetAllRDOs();

            Assert.That(result, Is.EquivalentTo(expectedResult).
                Using(new Func<IntegrationPointModel, IntegrationPoint, bool>(
                    (actual, expected) => (actual.Name == expected.Name) && (actual.SourceProvider == expected.SourceProvider.Value) && (actual.ArtifactId == expected.ArtifactId)
                                        && (actual.DestinationProvider == expected.DestinationProvider.Value))));
        }

        [Test]
        public void ItShouldGetIntegrationPointArtifactTypeId()
        {
            int expectedArtifactTypeId = 975;

            _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(Arg.Any<Guid>()).Returns(expectedArtifactTypeId);

            var actualResult = _integrationPointRepository.GetIntegrationPointArtifactTypeId();

            _objectTypeRepository.Received(1).RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));

            Assert.That(actualResult, Is.EqualTo(expectedArtifactTypeId));
        }


        [Test]
        public void ItShouldRetrieveAllOverwriteFieldChoices()
        {
            var expectedChoices = new List<ChoiceRef>
            {
                new ChoiceRef(756)
                {
                    Name = "name_653"
                },
                new ChoiceRef(897)
                {
                    Name = "name_466"
                }
            };

            _choiceQuery.GetChoicesOnField(_workspaceArtifactId, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(expectedChoices);

            var actualChoicesModels = _integrationPointRepository.GetOverwriteFieldChoices();

            Assert.That(actualChoicesModels,
                Is.EquivalentTo(expectedChoices).Using(new Func<OverwriteFieldsModel, ChoiceRef, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactID))));
        }

        [Test]
        [TestCase(null)]
        [TestCase(123)]
        public void ItShouldCreateIntegrationPointBasedOnProfile(int? federatedInstanceArtifactId)
        {
            int profileArtifactId = 565952;
            string integrationPointName = "ip_name_425";
            int artifactId = 131510;

            var integrationPoint = SetUpGetRdo(0, 789, "choice789", federatedInstanceArtifactId);

            var profile = new IntegrationPointProfile
            {
                OverwriteFields = new ChoiceRef(179935),
                SourceProvider = 237,
                DestinationConfiguration = _serializedDestinationConfiguration,
                SourceConfiguration = "391908",
                DestinationProvider = 363,
                Type = 840,
                EnableScheduler = false,
                ScheduleRule = string.Empty,
                EmailNotificationRecipients = "420590",
                LogErrors = false,
                NextScheduledRuntimeUTC = DateTime.MaxValue,
                FieldMappings = "266304",
                Name = "ip_159"
            };

            _integrationPointProfileService.ReadIntegrationPointProfile(profileArtifactId).Returns(profile);
            _integrationPointService.SaveIntegration(Arg.Any<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>()).Returns(artifactId);
            _integrationPointLocalService.ReadIntegrationPoint(artifactId).Returns(integrationPoint);

            _integrationPointRepository.CreateIntegrationPointFromProfile(profileArtifactId, integrationPointName);

            _integrationPointProfileService.Received(1).ReadIntegrationPointProfile(profileArtifactId);
            _integrationPointService.Received(1).SaveIntegration(Arg.Is<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>(x => x.Name == integrationPointName && x.ArtifactID == 0));
            _integrationPointLocalService.Received(1).ReadIntegrationPoint(artifactId);
        }
    }
}