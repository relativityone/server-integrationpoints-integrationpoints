using System;
using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointProfileServiceTests : TestBase
    {
        private readonly int _sourceWorkspaceArtifactId = 1234;
        private readonly int _targetWorkspaceArtifactId = 9954;
        private readonly int _integrationPointProfileArtifactId = 741;
        private readonly int _savedSearchArtifactId = 93032;
        private readonly int _sourceProviderId = 321;
        private readonly int _destinationProviderId = 424;

        private IHelper _helper;
        private ICaseServiceContext _caseServiceContext;
        private IRelativityObjectManager _objectManager;
        private IIntegrationPointSerializer _serializer;
        private IManagerFactory _managerFactory;
        private IntegrationPointProfile _integrationPointProfile;
        private SourceProvider _sourceProvider;
        private IIntegrationPointProviderValidator _integrationModelValidator;
        private IIntegrationPointPermissionValidator _permissionValidator;
        private IntegrationPointProfileService _instance;
        private IChoiceQuery _choiceQuery;
        private IValidationExecutor _validationExecutor;

        [SetUp]
        public override void SetUp()
        {
            _helper = Substitute.For<IHelper>();
            _caseServiceContext = Substitute.For<ICaseServiceContext>();
            _objectManager = Substitute.For<IRelativityObjectManager>();
            _serializer = Substitute.For<IIntegrationPointSerializer>();
            _managerFactory = Substitute.For<IManagerFactory>();
            _choiceQuery = Substitute.For<IChoiceQuery>();
            _integrationModelValidator = Substitute.For<IIntegrationPointProviderValidator>();
            _permissionValidator = Substitute.For<IIntegrationPointPermissionValidator>();

            _integrationModelValidator.Validate(
                Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>(),
                Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>()).Returns(new ValidationResult());

            _validationExecutor = Substitute.For<IValidationExecutor>();

            _instance = Substitute.ForPartsOf<IntegrationPointProfileService>(
                _helper,
                _caseServiceContext,
                _serializer,
                _choiceQuery,
                _managerFactory,
                _validationExecutor,
                _objectManager
            );

            _caseServiceContext.RelativityObjectManagerService = Substitute.For<IRelativityObjectManagerService>();
            _caseServiceContext.WorkspaceID = _sourceWorkspaceArtifactId;

            _sourceProvider = new SourceProvider();

            _integrationPointProfile = new IntegrationPointProfile()
            {
                ArtifactId = _integrationPointProfileArtifactId,
                Name = "Integration Point Profile",
                DestinationConfiguration =
                    $"{{ DestinationProviderType : \"{Core.Services.Synchronizer.RdoSynchronizerProvider.RDO_SYNC_TYPE_GUID}\" }}",
                DestinationProvider = _destinationProviderId,
                EmailNotificationRecipients = "emails",
                EnableScheduler = false,
                FieldMappings = "",
                LogErrors = false,
                SourceProvider = _sourceProviderId,
                SourceConfiguration =
                    $"{{ TargetWorkspaceArtifactId : {_targetWorkspaceArtifactId}, SourceWorkspaceArtifactId : {_sourceWorkspaceArtifactId}, SavedSearchArtifactId: {_savedSearchArtifactId} }}",
                NextScheduledRuntimeUTC = null,
                ScheduleRule = string.Empty
            };

            _objectManager.Read<IntegrationPointProfile>(_integrationPointProfileArtifactId)
                .Returns(_integrationPointProfile);
            _objectManager.StreamUnicodeLongText(_integrationPointProfileArtifactId, Arg.Is<FieldRef>(x => x.Guid == IntegrationPointProfileFieldGuids.DestinationConfigurationGuid),
                    Arg.Any<ExecutionIdentity>())
                .Returns(new MemoryStream());
            _objectManager.StreamUnicodeLongText(_integrationPointProfileArtifactId, Arg.Is<FieldRef>(x => x.Guid == IntegrationPointProfileFieldGuids.SourceConfigurationGuid),
                    Arg.Any<ExecutionIdentity>())
                .Returns(new MemoryStream());
            _objectManager.StreamUnicodeLongText(_integrationPointProfileArtifactId, Arg.Is<FieldRef>(x => x.Guid == IntegrationPointProfileFieldGuids.FieldMappingsGuid),
                    Arg.Any<ExecutionIdentity>())
                .Returns(new MemoryStream());
            _objectManager.Read<SourceProvider>(_sourceProviderId).Returns(_sourceProvider);

            _integrationModelValidator.Validate(
                Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>(),
                Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>()).Returns(new ValidationResult());

            _permissionValidator.Validate(
                Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>(),
                Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>()).Returns(new ValidationResult());

            _permissionValidator.ValidateSave(
                Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>(),
                Arg.Any<IntegrationPointType>(), Arg.Any<Guid>(), Arg.Any<int>()).Returns(new ValidationResult());
        }

        [Test]
        public void SaveIntegrationPointProfile_NoSchedule_GoldFlow()
        {
            // Arrange
            const int newIpProfileID = 389234;
            IntegrationPointProfileModel model = PrepareValidProfileModel(newIpProfileID);

            // Act
            int result = _instance.SaveIntegration(model);

            // Assert
            Assert.AreEqual(newIpProfileID, result, "The resulting artifact id should match.");
            _objectManager.Received(1)
                .Create(Arg.Is<IntegrationPointProfile>(x => x.ArtifactId == newIpProfileID));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UpdateIntegrationPointProfile_GoldFlow(bool isRelativityProvider)
        {
            // Arrange
            int targetWorkspaceArtifactId = 6543;
            var model = new IntegrationPointProfileModel()
            {
                ArtifactID = 741,
                SourceProvider = 9999,
                DestinationProvider = 7553,
                SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId }),
                SelectedOverwrite = "SelectedOverwrite",
                Scheduler = new Scheduler() { EnableScheduler = false },
                Destination = JsonConvert.SerializeObject(new { DestinationProviderType = "" })
            };

            var existingModel = new IntegrationPointProfileModel()
            {
                ArtifactID = model.ArtifactID,
                SourceProvider = model.SourceProvider,
                SourceConfiguration = model.SourceConfiguration,
                DestinationProvider = model.DestinationProvider,
                SelectedOverwrite = model.SelectedOverwrite,
                Scheduler = model.Scheduler,
                Destination = JsonConvert.SerializeObject(new { DestinationProviderType = "" })
            };

            _instance.ReadIntegrationPointProfileModel(Arg.Is(model.ArtifactID)).Returns(existingModel);
            _choiceQuery.GetChoicesOnField(_sourceWorkspaceArtifactId, Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields))
                .Returns(new List<ChoiceRef>()
                {
                    new ChoiceRef(5555)
                    {
                        Name = model.SelectedOverwrite
                    }
                });

            _caseServiceContext.EddsUserID = 1232;

            _objectManager.Read<SourceProvider>(Arg.Is(model.SourceProvider))
                .Returns(new SourceProvider()
                {
                    Identifier =
                        isRelativityProvider
                            ? Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID
                            : Guid.NewGuid().ToString()
                });

            // Act
            int result = _instance.SaveIntegration(model);

            // Assert
            Assert.AreEqual(model.ArtifactID, result, "The resulting artifact id should match.");
            _objectManager.Received(1)
                .Update(Arg.Is<IntegrationPointProfile>(x => x.ArtifactId == model.ArtifactID));

            _objectManager
                .Received(1)
                .Read<SourceProvider>(Arg.Is(model.SourceProvider));
        }

        [Test]
        public void GetRdo_ArtifactIdExists_ReturnsRdo_Test()
        {
            //Act
            IntegrationPointProfile integrationPointProfile = _instance.ReadIntegrationPointProfile(_integrationPointProfileArtifactId);

            //Assert
            _objectManager.Received(1).Read<IntegrationPointProfile>(_integrationPointProfileArtifactId);
            Assert.IsNotNull(integrationPointProfile);
        }

        [Test]
        public void GetRdo_ArtifactIdDoesNotExist_ExceptionThrown_Test()
        {
            //Arrange
            _objectManager.Read<IntegrationPointProfile>(_integrationPointProfileArtifactId).Throws<Exception>();

            //Act
            Assert.Throws<Exception>(() => _instance.ReadIntegrationPointProfile(_integrationPointProfileArtifactId), "Unable to retrieve Integration Point.");
        }

        [Test]
        public void UpdateIntegrationPointProfile_ProfileReadFail()
        {
            int targetWorkspaceArtifactId = 9302;
            var model = new IntegrationPointProfileModel()
            {
                ArtifactID = 123,
                SourceProvider = 9830,
                SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId })
            };

            _instance.ReadIntegrationPointProfileModel(Arg.Is(model.ArtifactID))
                .Throws(new Exception(String.Empty));

            // Act
            Assert.Throws<Exception>(() => _instance.SaveIntegration(model), "Unable to save Integration Point: Unable to retrieve Integration Point");
        }

        [Test]
        public void ReadIntegrationPointProfile_ShouldReturnIntegrationPointProfile_WhenRepositoryReturnsIntegrationPoint()
        {
            // arrange
            _objectManager
                .Read<IntegrationPointProfile>(_integrationPointProfileArtifactId)
                .Returns(_integrationPointProfile);

            // act
            IntegrationPointProfile result = _instance.ReadIntegrationPointProfile(_integrationPointProfileArtifactId);

            // assert
            _objectManager
                .Received(1)
                .Read<IntegrationPointProfile>(_integrationPointProfileArtifactId);
            Assert.AreEqual(_integrationPointProfile, result);
        }

        [Test]
        public void ReadIntegrationPointProfile_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // arrange
            _objectManager
                .Read<IntegrationPointProfile>(_integrationPointProfileArtifactId)
                .Throws<Exception>();

            // act
            Assert.Throws<Exception>(() => _instance.ReadIntegrationPointProfile(_integrationPointProfileArtifactId));

            // assert
            _objectManager
                .Received(1)
                .Read<IntegrationPointProfile>(_integrationPointProfileArtifactId);
        }

        [Test]
        public void SaveIntegration_ShouldThrowException_WhenValidationFails()
        {
            // arrange
            const int newIPProfileID = 389234;
            IntegrationPointProfileModel model = PrepareValidProfileModel(newIPProfileID);

            _validationExecutor
                .When(m => m.ValidateOnSave(Arg.Any<ValidationContext>()))
                .Do(x => throw new IntegrationPointValidationException(new ValidationResult(false)));

            // act
            Assert.Throws<IntegrationPointValidationException>(() => _instance.SaveIntegration(model));
        }

        private IntegrationPointProfileModel PrepareValidProfileModel(int newIPprofileID)
        {
            const int targetWorkspaceArtifactID = 6543;
            var model = new IntegrationPointProfileModel()
            {
                ArtifactID = 0,
                SourceProvider = 9999,
                DestinationProvider = 7553,
                SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactID }),
                SelectedOverwrite = "SelectedOverwrite",
                Scheduler = new Scheduler() { EnableScheduler = false },
                Destination = JsonConvert.SerializeObject(new { DestinationProviderType = "" })
            };

            _choiceQuery.GetChoicesOnField(_sourceWorkspaceArtifactId, Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields))
                .Returns(new List<ChoiceRef>()
                {
                    new ChoiceRef(5555)
                    {
                        Name = model.SelectedOverwrite
                    }
                });

            _objectManager.Create(
                    Arg.Is<IntegrationPointProfile>(x => x.ArtifactId == 0))
                .Returns(newIPprofileID);

            _caseServiceContext.EddsUserID = 1232;

            return model;
        }
    }
}