using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.Services.Choice;

namespace Relativity.IntegrationPoints.Services.Tests.Repositories
{
    [TestFixture]
    [Category("Unit")]
    public class IntegrationPointRepositoryTests : TestBase
    {
        private IObjectTypeRepository _objectTypeRepository;
        private IUserInfo _userInfo;
        private IChoiceQuery _choiceQuery;
        private IBackwardCompatibility _backwardCompatibility;
        private IIntegrationPointService _integrationPointService;
        private IIntegrationPointProfileService _integrationPointProfileService;
        private Services.Repositories.IIntegrationPointAccessor _integrationPointAccessor;
        private DestinationConfiguration _destinationConfiguration;
        private ICaseServiceContext _caseContext;
        private int _workspaceArtifactId = 100;

        public override void SetUp()
        {
            _integrationPointService = Substitute.For<IIntegrationPointService>();
            _integrationPointProfileService = Substitute.For<IIntegrationPointProfileService>();
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();
            _userInfo = Substitute.For<IUserInfo>();
            _choiceQuery = Substitute.For<IChoiceQuery>();
            _backwardCompatibility = Substitute.For<IBackwardCompatibility>();
            _caseContext = Substitute.For<ICaseServiceContext>();
            _caseContext.WorkspaceID.Returns(_workspaceArtifactId);

            _choiceQuery.GetChoicesOnField(_workspaceArtifactId, IntegrationPointFieldGuids.OverwriteFieldsGuid).Returns(new List<ChoiceRef>
            {
                new ChoiceRef(588)
                {
                    Name = "Append/Overlay"
                }
            });

            _integrationPointAccessor = new IntegrationPointAccessor(
                _objectTypeRepository,
                _userInfo,
                _choiceQuery,
                _backwardCompatibility,
                _integrationPointService,
                _integrationPointProfileService,
                _caseContext);
        }

        [Test]
        public void ItShouldCreateIntegrationPoint()
        {
            var overwriteFieldsChoiceId = 934;
            var overwriteFieldsChoiceName = "Append/Overlay";
            var integrationPointArtifactId = 134;

            var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointArtifactId);

            _integrationPointService.SaveIntegrationPoint(Arg.Is<IntegrationPointDto>(x => x.ArtifactId == 0))
                .Returns(integrationPointArtifactId);

            _integrationPointAccessor.CreateIntegrationPoint(createRequest);

            _backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
            _integrationPointService.Received(1).Read(integrationPointArtifactId);
        }

        [Test]
        public void ItShouldUpdateIntegrationPoint()
        {
            var overwriteFieldsChoiceId = 215;
            var overwriteFieldsChoiceName = "Append/Overlay";
            var integrationPointArtifactId = 902;

            var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointArtifactId);

            _integrationPointService.SaveIntegrationPoint(
                Arg.Is<IntegrationPointDto>(x => x.ArtifactId == createRequest.IntegrationPoint.ArtifactId))
                .Returns(integrationPointArtifactId);

            _integrationPointAccessor.CreateIntegrationPoint(createRequest);

            _backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
            _integrationPointService.Received(1).Read(integrationPointArtifactId);
        }

        private UpdateIntegrationPointRequest SetUpCreateOrUpdateTest(int overwriteFieldsChoiceId, string overwriteFieldsChoiceName, int integrationPointArtifactId)
        {
            SetUpGetDto(integrationPointArtifactId, overwriteFieldsChoiceId, overwriteFieldsChoiceName);

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
                    SecuredConfiguration = "{}",

                }
            };

            return createRequest;
        }

        private IntegrationPointDto SetUpGetDto(int integrationPointArtifactId, int overwriteFieldsChoiceId = 123, string overwriteFieldsChoiceName = "AppendOverlay")
        {
            _destinationConfiguration = new DestinationConfiguration
            {
                ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay
            };

            var integrationPointDto = CreateIntegrationPoint(integrationPointArtifactId, overwriteFieldsChoiceId, overwriteFieldsChoiceName);

            _integrationPointService.Read(integrationPointArtifactId).Returns(integrationPointDto);

            _choiceQuery.GetChoicesOnField(_workspaceArtifactId, new Guid(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<ChoiceRef>
            {
                new ChoiceRef(overwriteFieldsChoiceId)
                {
                    Name = overwriteFieldsChoiceName
                }
            });

            return integrationPointDto;
        }

        private IntegrationPointDto CreateIntegrationPoint(int integrationPointArtifactId, int overwriteFieldsChoiceId = 123, string overwriteFieldsChoiceName = "choice123")
        {
            return new IntegrationPointDto
            {
                ArtifactId = integrationPointArtifactId,
                Name = "name_762",
                DestinationProvider = 715,
                EmailNotificationRecipients = "emails",
                HasErrors = false,
                JobHistory = null,
                LastRun = null,
                LogErrors = false,
                SourceProvider = 718,
                SelectedOverwrite = overwriteFieldsChoiceName,
                Type = 0,
                SecuredConfiguration = string.Empty,
                FieldMappings = new List<FieldsMapping.Models.FieldMap>
                {
                    new FieldsMapping.Models.FieldMap
                    {
                        DestinationField = new Contracts.Models.FieldEntry()
                        {
                            Type = "type",
                            DisplayName = "Name",
                            FieldType = Contracts.Models.FieldType.String,
                            FieldIdentifier = "identifier",
                            IsIdentifier = true,
                            IsRequired = true
                        },
                        SourceField = new Contracts.Models.FieldEntry()
                        {
                            Type = "type",
                            DisplayName = "Name",
                            FieldType = Contracts.Models.FieldType.String,
                            FieldIdentifier = "identifier",
                            IsIdentifier = true,
                            IsRequired = true
                        },
                        FieldMapType = FieldMapTypeEnum.Identifier
                    }
                },
                DestinationConfiguration = new DestinationConfiguration
                {
                    ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay
                },
                Scheduler = new Scheduler()
            };
        }

        [Test]
        public void ItShouldGetIntegrationPoint()
        {
            int artifactId = 884;
            var integrationPoint = CreateIntegrationPoint(artifactId);

            _integrationPointService.Read(artifactId).Returns(integrationPoint);

            var result = _integrationPointAccessor.GetIntegrationPoint(artifactId);

            _integrationPointService.Received(1).Read(artifactId);

            Assert.That(result.SourceProvider, Is.EqualTo(integrationPoint.SourceProvider));
            Assert.That(result.DestinationProvider, Is.EqualTo(integrationPoint.DestinationProvider));
            Assert.That(result.ArtifactId, Is.EqualTo(integrationPoint.ArtifactId));
            Assert.That(result.Name, Is.EqualTo(integrationPoint.Name));
        }

        [Test]
        public void ItShouldRunIntegrationPointWithUser()
        {
            int workspaceId = 873;
            int artifactId = 797;
            int userId = 127;

            _userInfo.ArtifactID.Returns(userId);

            SetUpGetDto(artifactId, 456, "choice456");

            _integrationPointAccessor.RunIntegrationPoint(workspaceId, artifactId);

            _integrationPointService.Received(1).RunIntegrationPoint(workspaceId, artifactId, userId);
        }

        [Test]
        public void ItShouldGetAllIntegrationPoints()
        {
            var integrationPoint1 = CreateIntegrationPoint(263);
            var integrationPoint2 = CreateIntegrationPoint(204);

            var expectedResult = new List<IntegrationPointDto> { integrationPoint1, integrationPoint2 };
            _integrationPointService.ReadAll().Returns(expectedResult);

            var result = _integrationPointAccessor.GetAllIntegrationPoints();

            _integrationPointService.Received(1).ReadAll();

            Assert.That(result, Is.EquivalentTo(expectedResult).
                Using(new Func<IntegrationPointModel, IntegrationPointDto, bool>(
                    (actual, expected) => (actual.Name == expected.Name) && (actual.SourceProvider == expected.SourceProvider) && (actual.ArtifactId == expected.ArtifactId)
                                        && (actual.DestinationProvider == expected.DestinationProvider))));
        }

        [Test]
        public void ItShouldGetIntegrationPointArtifactTypeId()
        {
            int expectedArtifactTypeId = 975;

            _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(Arg.Any<Guid>()).Returns(expectedArtifactTypeId);

            var actualResult = _integrationPointAccessor.GetIntegrationPointArtifactTypeId();

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

            var actualChoicesModels = _integrationPointAccessor.GetOverwriteFieldChoices();

            Assert.That(actualChoicesModels,
                Is.EquivalentTo(expectedChoices).Using(new Func<OverwriteFieldsModel, ChoiceRef, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactID))));
        }

        [Test]
        public void ItShouldCreateIntegrationPointBasedOnProfile()
        {
            int profileArtifactId = 565952;
            string integrationPointName = "ip_name_425";
            int artifactId = 131510;

            var integrationPoint = SetUpGetDto(0, 789);

            var profile = new IntegrationPointProfileDto
            {
                SelectedOverwrite = "No",
                SourceProvider = 237,
                DestinationConfiguration = _destinationConfiguration,
                SourceConfiguration = "391908",
                DestinationProvider = 363,
                Type = 840,
                EmailNotificationRecipients = "420590",
                LogErrors = false,
                NextRun = DateTime.MaxValue,
                Name = "ip_159"
            };

            _integrationPointProfileService.Read(profileArtifactId).Returns(profile);
            _integrationPointService.SaveIntegrationPoint(Arg.Any<IntegrationPointDto>()).Returns(artifactId);
            _integrationPointService.Read(artifactId).Returns(integrationPoint);

            _integrationPointAccessor.CreateIntegrationPointFromProfile(profileArtifactId, integrationPointName);

            _integrationPointProfileService.Received(1).Read(profileArtifactId);
            _integrationPointService.Received(1).SaveIntegrationPoint(Arg.Is<IntegrationPointDto>(x => x.Name == integrationPointName && x.ArtifactId == 0));
            _integrationPointService.Received(1).Read(artifactId);
        }
    }
}
