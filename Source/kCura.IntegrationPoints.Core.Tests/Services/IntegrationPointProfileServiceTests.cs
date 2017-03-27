using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Toggles;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
	[TestFixture]
	public class IntegrationPointProfileServiceTests : TestBase
	{
		private readonly int _sourceWorkspaceArtifactId = 1234;
		private readonly int _targetWorkspaceArtifactId = 9954;
		private readonly int _integrationPointProfileArtifactId = 741;
		private readonly int _savedSearchArtifactId = 93032;
		private readonly int _sourceProviderId = 321;
		private readonly int _destinationProviderId = 424;

		private IHelper _helper;
		private IHelper _targetHelper;
		private ICaseServiceContext _caseServiceContext;
		private IContextContainer _contextContainer;
		private IContextContainerFactory _contextContainerFactory;
		private IIntegrationPointSerializer _serializer;
		private IManagerFactory _managerFactory;
		private IntegrationPointProfile _integrationPointProfile;
		private SourceProvider _sourceProvider;
		private IIntegrationPointProviderValidator _integrationModelValidator;
		private IIntegrationPointPermissionValidator _permissionValidator;
		private IntegrationPointProfileService _instance;
		private IChoiceQuery _choiceQuery;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_targetHelper = Substitute.For<IHelper>();
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_contextContainer = Substitute.For<IContextContainer>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_serializer = Substitute.For<IIntegrationPointSerializer>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_choiceQuery = Substitute.For<IChoiceQuery>();
			_integrationModelValidator = Substitute.For<IIntegrationPointProviderValidator>();
			_permissionValidator = Substitute.For<IIntegrationPointPermissionValidator>();
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);

			_integrationModelValidator.Validate(Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>(), Arg.Any<IntegrationPointType>(), Arg.Any<string>()).Returns(new ValidationResult());

			_instance = Substitute.ForPartsOf<IntegrationPointProfileService>(
				_helper,
				_caseServiceContext,
				_contextContainerFactory,
				_serializer,
				_choiceQuery,
				_managerFactory,
				_integrationModelValidator,
				_permissionValidator
			);

			_caseServiceContext.RsapiService = Substitute.For<IRSAPIService>();
			_caseServiceContext.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Returns(Substitute.For<IGenericLibrary<IntegrationPointProfile>>());
			_caseServiceContext.RsapiService.SourceProviderLibrary.Returns(Substitute.For<IGenericLibrary<SourceProvider>>());
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

			_caseServiceContext.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Read(_integrationPointProfileArtifactId)
				.Returns(_integrationPointProfile);
			_caseServiceContext.RsapiService.SourceProviderLibrary.Read(_sourceProviderId).Returns(_sourceProvider);

			_integrationModelValidator.Validate(
				Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>(),
				Arg.Any<IntegrationPointType>(), Arg.Any<string>()).Returns(new ValidationResult());

			_permissionValidator.Validate(
				Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>(),
				Arg.Any<IntegrationPointType>(), Arg.Any<string>()).Returns(new ValidationResult());

			_permissionValidator.ValidateSave(
				Arg.Any<IntegrationPointModelBase>(), Arg.Any<SourceProvider>(), Arg.Any<DestinationProvider>(),
				Arg.Any<IntegrationPointType>(), Arg.Any<string>()).Returns(new ValidationResult());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void SaveIntegrationPointProfile_NoSchedule_GoldFlow(bool isRelativityProvider)
		{
			// Arrange
			int targetWorkspaceArtifactId = 6543;
			var model = new IntegrationPointProfileModel()
			{
				ArtifactID = 0,
				SourceProvider = 9999,
				DestinationProvider = 7553,
				SourceConfiguration = JsonConvert.SerializeObject(new { TargetWorkspaceArtifactId = targetWorkspaceArtifactId }),
				SelectedOverwrite = "SelectedOverwrite",
				Scheduler = new Scheduler() { EnableScheduler = false },
				Destination = JsonConvert.SerializeObject(new { DestinationProviderType = "" })
			};

			_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields))
				.Returns(new List<Choice>()
				{
					new Choice(5555)
					{
						Name = model.SelectedOverwrite
					}
				});

			const int newIpProfileId = 389234;
			_caseServiceContext.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Create(
					Arg.Is<IntegrationPointProfile>(x => x.ArtifactId == 0))
				.Returns(newIpProfileId);

			_caseServiceContext.EddsUserID = 1232;

			// Act
			int result = _instance.SaveIntegration(model);

			// Assert
			Assert.AreEqual(newIpProfileId, result, "The resulting artifact id should match.");
			_caseServiceContext.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Received(1)
				.Create(Arg.Is<IntegrationPointProfile>(x => x.ArtifactId == newIpProfileId));
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

			_instance.ReadIntegrationPointProfile(Arg.Is(model.ArtifactID)).Returns(existingModel);
			_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields))
				.Returns(new List<Choice>()
				{
					new Choice(5555)
					{
						Name = model.SelectedOverwrite
					}
				});

			_caseServiceContext.EddsUserID = 1232;

			_caseServiceContext.RsapiService.SourceProviderLibrary.Read(Arg.Is(model.SourceProvider))
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
			_caseServiceContext.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Received(1)
				.Update(Arg.Is<IntegrationPointProfile>(x => x.ArtifactId == model.ArtifactID));

			_caseServiceContext.RsapiService.SourceProviderLibrary
				.Received(1)
				.Read(Arg.Is(model.SourceProvider));
		}

		[Test]
		public void GetRdo_ArtifactIdExists_ReturnsRdo_Test()
		{
			//Act
			IntegrationPointProfile integrationPointProfile = _instance.GetRdo(_integrationPointProfileArtifactId);

			//Assert
			_caseServiceContext.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Received(1).Read(_integrationPointProfileArtifactId);
			Assert.IsNotNull(integrationPointProfile);
		}

		[Test]
		public void GetRdo_ArtifactIdDoesNotExist_ExceptionThrown_Test()
		{
			//Arrange
			_caseServiceContext.RsapiService.GetGenericLibrary<IntegrationPointProfile>().Read(_integrationPointProfileArtifactId).Throws<Exception>();

			//Act
			Assert.Throws<Exception>(() => _instance.GetRdo(_integrationPointProfileArtifactId), "Unable to retrieve Integration Point.");
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

			_instance.ReadIntegrationPointProfile(Arg.Is(model.ArtifactID))
				.Throws(new Exception(String.Empty));

			// Act
			Assert.Throws<Exception>(() => _instance.SaveIntegration(model), "Unable to save Integration Point: Unable to retrieve Integration Point");
		}
	}
}