using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
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
        private IntegrationPointProfileAccessor _integrationPointProfileAccessor;
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

            _integrationPointProfileAccessor = new IntegrationPointProfileAccessor(_backwardCompatibility, _integrationPointProfileService, _choiceQuery,
                _integrationPointService, _caseContext);

            _choiceQuery.GetChoicesOnField(_workspaceArtifactId, new Guid(IntegrationPointProfileFieldGuids.OverwriteFields)).Returns(new List<ChoiceRef>
            {
                new ChoiceRef(588)
                {
                    Name = "Append/Overlay"
                }
            });
        }

        [Test]
        public void ItShouldCreateIntegrationPoint()
        {
            var overwriteFieldsChoiceId = 588;
            var overwriteFieldsChoiceName = "Append/Overlay";
            var integrationPointProfileArtifactId = 219;

            var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointProfileArtifactId);

            _integrationPointProfileService.SaveProfile(Arg.Is<IntegrationPointProfileDto>(x => x.ArtifactId == 0)).Returns(integrationPointProfileArtifactId);

            _integrationPointProfileAccessor.CreateIntegrationPointProfile(createRequest);

            _backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
            _integrationPointProfileService.Received(1).Read(integrationPointProfileArtifactId);
        }

        [Test]
        public void ItShouldUpdateIntegrationPoint()
        {
            var overwriteFieldsChoiceId = 152;
            var overwriteFieldsChoiceName = "Append/Overlay";
            var integrationPointProfileArtifactId = 289;

            var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointProfileArtifactId);

            _integrationPointProfileService.SaveProfile(Arg.Is<IntegrationPointProfileDto>(x => x.ArtifactId == createRequest.IntegrationPoint.ArtifactId))
                .Returns(integrationPointProfileArtifactId);

            _integrationPointProfileAccessor.CreateIntegrationPointProfile(createRequest);

            _backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
            _integrationPointProfileService.Received(1).Read(integrationPointProfileArtifactId);
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
                    },
                    FieldMappings = new List<FieldMap>
                    {
                        new FieldMap
                        {
                            DestinationField = new FieldEntry
                            {
                                Type = "type",
                                DisplayName = "Name",
                                FieldType = FieldType.String,
                                FieldIdentifier = "identifier",
                                IsIdentifier = true,
                                IsRequired = true
                            },
                            SourceField = new FieldEntry
                            {
                                Type = "type",
                                DisplayName = "Name",
                                FieldType = FieldType.String,
                                FieldIdentifier = "identifier",
                                IsIdentifier = true,
                                IsRequired = true
                            },
                            FieldMapType = FieldMapType.Identifier
                        }
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
            _integrationPointProfileService.Read(integrationPointProfileArtifactId).Returns(new IntegrationPointProfileDto
            {
                ArtifactId = integrationPointProfileArtifactId,
                Name = "name_982",
                SourceProvider = 268,
                DestinationProvider = 288,
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
            });
            return createRequest;
        }

        [Test]
        public void ItShouldGetIntegrationPoint()
        {
            int artifactId = 924;
            var integrationPointProfile = new IntegrationPointProfileDto
            {
                ArtifactId = 235,
                Name = "ip_name_350",
                SourceProvider = 471,
                DestinationProvider = 817,
                SecuredConfiguration = string.Empty,
                Type = 3,
                PromoteEligible = false,
                EmailNotificationRecipients = string.Empty,
                LogErrors = true,
                Scheduler = new Scheduler
                {
                    EnableScheduler = false
                },
                NextRun = DateTime.Now,
                SelectedOverwrite = string.Empty,
                DestinationConfiguration = new DestinationConfiguration
                {
                    ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay
                },
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
                SourceConfiguration = string.Empty
            };

            _integrationPointProfileService.Read(artifactId).Returns(integrationPointProfile);

            var result = _integrationPointProfileAccessor.GetIntegrationPointProfile(artifactId);

            _integrationPointProfileService.Received(1).Read(artifactId);

            Assert.That(result.SourceProvider, Is.EqualTo(integrationPointProfile.SourceProvider));
            Assert.That(result.DestinationProvider, Is.EqualTo(integrationPointProfile.DestinationProvider));
            Assert.That(result.ArtifactId, Is.EqualTo(integrationPointProfile.ArtifactId));
            Assert.That(result.Name, Is.EqualTo(integrationPointProfile.Name));
        }

        [Test]
        public void ItShouldGetAllIntegrationPoints()
        {
            var integrationPointProfile1 = new IntegrationPointProfileDto
            {
                ArtifactId = 844,
                Name = "ip_name_234",
                SourceProvider = 871,
                DestinationProvider = 143,
                DestinationConfiguration = new DestinationConfiguration
                {
                    ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay
                },
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
                SourceConfiguration = string.Empty,
                Scheduler = new Scheduler
                {
                    EnableScheduler = false
                },
            };
            var integrationPointProfile2 = new IntegrationPointProfileDto
            {
                ArtifactId = 526,
                Name = "ip_name_617",
                SourceProvider = 723,
                DestinationProvider = 158,
                DestinationConfiguration = new DestinationConfiguration
                {
                    ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay
                },
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
                SourceConfiguration = string.Empty,
                Scheduler = new Scheduler
                {
                    EnableScheduler = false
                },
            };

            var expectedResult = new List<IntegrationPointProfileDto> { integrationPointProfile1, integrationPointProfile2 };
            _integrationPointProfileService.ReadAll().Returns(expectedResult);

            var result = _integrationPointProfileAccessor.GetAllIntegrationPointProfiles();

            _integrationPointProfileService.Received(1).ReadAll();

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreEqual(result[i].Name, expectedResult[i].Name);
                Assert.AreEqual(result[i].ArtifactId, expectedResult[i].ArtifactId);
                Assert.AreEqual(result[i].SourceProvider, expectedResult[i].SourceProvider);
                Assert.AreEqual(result[i].DestinationProvider, expectedResult[i].DestinationProvider);
            }
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

            var actualChoicesModels = _integrationPointProfileAccessor.GetOverwriteFieldChoices();

            Assert.That(actualChoicesModels,
                Is.EquivalentTo(expectedChoices).Using(new Func<OverwriteFieldsModel, ChoiceRef, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactID))));
        }

        [Test]
        public void ItShouldCreateProfileBasedOnIntegrationPoint()
        {
            int integrationPointArtifactId = 988586;
            string profileName = "profile_name_100";
            int artifactId = 984783;

            var integrationPoint = new IntegrationPointDto
            {
                SelectedOverwrite = "123",
                SourceProvider = 284,
                SourceConfiguration = "559417",
                DestinationProvider = 346,
                Type = 190,
                EmailNotificationRecipients = "432824",
                LogErrors = false,
                NextRun = DateTime.MaxValue,
                Name = "ip_346",
                SecuredConfiguration = "{}",
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
            var integrationPointProfile = new IntegrationPointProfileDto
            {
                Name = "profile_990",
                SourceProvider = 451,
                DestinationProvider = 443,
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

            _integrationPointService.Read(integrationPointArtifactId).Returns(integrationPoint);
            _integrationPointProfileService.SaveProfile(Arg.Any<IntegrationPointProfileDto>()).Returns(artifactId);
            _integrationPointProfileService.Read(artifactId).Returns(integrationPointProfile);

            _integrationPointProfileAccessor.CreateIntegrationPointProfileFromIntegrationPoint(integrationPointArtifactId, profileName);

            _integrationPointService.Received(1).Read(integrationPointArtifactId);
            _integrationPointProfileService.Received(1).SaveProfile(Arg.Is<IntegrationPointProfileDto>(x => x.Name == profileName && x.ArtifactId == 0));
            _integrationPointProfileService.Received(1).Read(artifactId);
        }
    }
}
