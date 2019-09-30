using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Testing.Identification;
using Console = kCura.EventHandler.Console;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.IntegrationPoints
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ConsoleEventHandlerTests
	{
		[SetUp]
		public void SetUp()
		{
			_managerFactory = Substitute.For<IManagerFactory>();
			_providerTypeService = Substitute.For<IProviderTypeService>();
			_helperClassFactory = Substitute.For<IHelperClassFactory>();
			_helper = Substitute.For<IEHHelper>();
			_helper.GetActiveCaseID().Returns(_APPLICATION_ID);
			_stateManager = Substitute.For<IStateManager>();
			_queueManager = Substitute.For<IQueueManager>();
			_onClickEventHelper = Substitute.For<IOnClickEventConstructor>();
			_errorManager = Substitute.For<IErrorManager>();
			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_permissionRepository = Substitute.For<IPermissionRepository>();
			_permissionValidator = Substitute.For<IIntegrationPointPermissionValidator>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_serializer = new JSONSerializer();

			var activeArtifact = new Artifact(_ARTIFACT_ID, null, 0, "", false, new FieldCollection
			{
				new Field(1, "Name", "Name", 1, 1, 1, false, false, new FieldValue(_ARTIFACT_NAME), null)
			});
			var application = new EventHandler.Application(_APPLICATION_ID, "", "");

			_instance =
				new EventHandlers.IntegrationPoints.ConsoleEventHandler(
					new ButtonStateBuilder(_providerTypeService, _queueManager, _jobHistoryManager, _stateManager, _permissionRepository, _permissionValidator, _integrationPointRepository),
					_onClickEventHelper, new ConsoleBuilder())
				{
					ActiveArtifact = activeArtifact,
					Application = application,
					Helper = _helper
				};
		}

		private const int _ARTIFACT_ID = 100300;
		private const int _APPLICATION_ID = 100101;
		private const string _ARTIFACT_NAME = "artifact_name";
		private const string _RUN = "Run";
		private const string _STOP = "Stop";
		private const string _RETRY_ERRORS = "Retry Errors";
		private const string _VIEW_ERRORS = "View Errors";
		private const string _RUN_ENDPOINT = "IP.importNow";
		private const string _RETRY_ENDPOINT = "IP.retryJob";
		private const string _STOP_ENDPOINT = "IP.stopJob";

		private IIntegrationPointRepository _integrationPointRepository;
		private IManagerFactory _managerFactory;
		private IProviderTypeService _providerTypeService;
		private IHelperClassFactory _helperClassFactory;
		private IEHHelper _helper;
		private IStateManager _stateManager;
		private IQueueManager _queueManager;
		private IOnClickEventConstructor _onClickEventHelper;
		private IErrorManager _errorManager;
		private IJobHistoryManager _jobHistoryManager;
		private IPermissionRepository _permissionRepository;
		private IIntegrationPointPermissionValidator _permissionValidator;
		private ISerializer _serializer;

		private ConsoleEventHandler _instance;

		[IdentifiedTestCase("46fdea0d-fa60-4d1c-9c14-00c6cb2aef83", false, true, false, false, true)]
		[IdentifiedTestCase("25b453c0-0db7-4b48-9dad-2332b7c118df", false, true, true, false, true)]
		[IdentifiedTestCase("7a69b913-3ed5-4318-b34d-bfca01742c85", true, true, true, false, true)]
		[IdentifiedTestCase("3c3766ab-d802-4f3f-8c11-d27e1f6c143c", false, false, false, false, true)]
		[IdentifiedTestCase("59dfc6b1-3a55-4a6b-a9e1-47d81bebb5df", false, false, true, false, true)]
		[IdentifiedTestCase("1ad0c7f0-cc37-40ef-bf2b-eca09f30f760", true, false, true, false, true)]
		[IdentifiedTestCase("3fce1a1d-8acd-4bf1-810f-273ba5591bb9", true, true, false, true, true)]
		[IdentifiedTestCase("30d700e2-41cd-4976-8cb6-2aa5710b7928", true, true, true, true, true)]
		[IdentifiedTestCase("7700d508-ac18-4c71-985d-1c92e771e3c5", true, true, true, true, true)]
		[IdentifiedTestCase("afeccd33-9585-4308-8811-b11679ef5581", true, false, false, true, true)]
		[IdentifiedTestCase("613e7afd-2866-4f0b-9ff0-b3d1acf568e6", true, false, true, true, true)]
		[IdentifiedTestCase("de27f6e3-90e8-4cce-82b3-343320508f0e", true, false, true, true, true)]
		[IdentifiedTestCase("70d79854-3194-4bff-9a23-02ffe7167aa7", true, false, true, true, false)]
		public void GetConsole_RelativityProvider_GoldFlow(bool hasJobsExecutingOrInQueue, bool hasRunPermissions, bool hasViewErrorsPermissions, bool hasStoppableJobs,
			bool hasProfileAddPermission)
		{
			// ARRANGE
			var importSettings = new ImportSettings { ImageImport = false };
			var integrationPoint = new Data.IntegrationPoint
			{
				HasErrors = true,
				SourceProvider = 8392,
				DestinationProvider = 437,
				DestinationConfiguration = _serializer.Serialize(importSettings)
			};

			string[] viewErrorMessages = {Constants.IntegrationPoints.PermissionErrors.JOB_HISTORY_NO_VIEW};
			ProviderType providerType = ProviderType.Relativity;
			_managerFactory.CreateStateManager().Returns(_stateManager);
			_managerFactory.CreateQueueManager().Returns(_queueManager);
			_managerFactory.CreateJobHistoryManager().Returns(_jobHistoryManager);

			_permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(hasProfileAddPermission);

			_helperClassFactory.CreateOnClickEventHelper(_managerFactory).Returns(_onClickEventHelper);

			_integrationPointRepository.ReadWithFieldMappingAsync(_ARTIFACT_ID).Returns(integrationPoint);
			_providerTypeService.GetProviderType(integrationPoint.SourceProvider.Value, integrationPoint.DestinationProvider.Value).Returns(providerType);

			StoppableJobCollection stoppableJobCollection = null;

			if (hasStoppableJobs)
			{
				stoppableJobCollection = new StoppableJobCollection
				{
					PendingJobArtifactIds = new[] {1232},
					ProcessingJobArtifactIds = new[] {9403}
				};
			}
			else
			{
				stoppableJobCollection = new StoppableJobCollection();
			}

			_jobHistoryManager.GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID).Returns(stoppableJobCollection);

			if (!hasRunPermissions || !hasViewErrorsPermissions)
			{
				_managerFactory.CreateErrorManager().Returns(_errorManager);
			}

			ButtonStateDTO buttonStates = null;
			OnClickEventDTO onClickEvents = null;

			buttonStates = new ButtonStateDTO
			{
				RunButtonEnabled = !hasJobsExecutingOrInQueue,
				RetryErrorsButtonEnabled = !hasJobsExecutingOrInQueue,
				ViewErrorsLinkEnabled = hasViewErrorsPermissions,
				StopButtonEnabled = hasStoppableJobs,
				RetryErrorsButtonVisible = true,
				ViewErrorsLinkVisible = hasViewErrorsPermissions
			};

			_permissionValidator.ValidateViewErrors(_APPLICATION_ID).Returns(
				new ValidationResult(hasViewErrorsPermissions ? null : viewErrorMessages)
				{
					IsValid = hasViewErrorsPermissions
				});

			_queueManager.HasJobsExecutingOrInQueue(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasJobsExecutingOrInQueue);

			_stateManager.GetButtonState(
					ProviderType.Relativity, 
					hasJobsExecutingOrInQueue,
					integrationPoint.HasErrors.Value,
					hasViewErrorsPermissions,
					hasStoppableJobs,
					hasProfileAddPermission)
				.Returns(buttonStates);

			string actionButtonOnClickEvent;
			if (!hasJobsExecutingOrInQueue)
			{
				actionButtonOnClickEvent = $"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})";
			}
			else if (hasStoppableJobs)
			{
				actionButtonOnClickEvent = $"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})";
			}
			else
			{
				actionButtonOnClickEvent = string.Empty;
			}
			onClickEvents = new OnClickEventDTO
			{
				RunOnClickEvent = actionButtonOnClickEvent,
				RetryErrorsOnClickEvent = integrationPoint.HasErrors.Value && !hasJobsExecutingOrInQueue ? $"{_RETRY_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})" : string.Empty,
				ViewErrorsOnClickEvent =
					integrationPoint.HasErrors.Value && hasViewErrorsPermissions ? "Really long string" : string.Empty,
				StopOnClickEvent = actionButtonOnClickEvent
			};

			_onClickEventHelper.GetOnClickEvents(_APPLICATION_ID, _ARTIFACT_ID, _ARTIFACT_NAME, buttonStates)
				.Returns(onClickEvents);

			// ACT
			Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			_permissionValidator.Received(1).ValidateViewErrors(_APPLICATION_ID);

			Assert.IsNotNull(console);
			if (hasViewErrorsPermissions)
			{
				int buttonCount = 3;
				Assert.AreEqual(buttonCount, console.Items.Count, $"There should be {buttonCount} buttons on the console");
			}
			else
			{
				int buttonCount = 2;
				Assert.AreEqual(buttonCount, console.Items.Count, $"There should be {buttonCount} buttons on the console");
			}

			int buttonIndex = 0;
			ConsoleButton runButton = (ConsoleButton) console.Items[buttonIndex++];


			_jobHistoryManager.Received(1).GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID);

			if (!hasJobsExecutingOrInQueue)
			{
				Assert.AreEqual(_RUN, runButton.DisplayText);
				Assert.AreEqual($"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", runButton.OnClickEvent);
				Assert.AreEqual(buttonStates.RunButtonEnabled, runButton.Enabled);
				Assert.AreEqual(false, runButton.RaisesPostBack);
			}
			else if (hasStoppableJobs)
			{
				Assert.AreEqual(_STOP, runButton.DisplayText);
				Assert.AreEqual($"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", runButton.OnClickEvent);
				Assert.AreEqual(buttonStates.StopButtonEnabled, runButton.Enabled);
				Assert.AreEqual(false, runButton.RaisesPostBack);
			}
			else
			{
				Assert.AreEqual(_STOP, runButton.DisplayText);
				Assert.AreEqual(string.Empty, runButton.OnClickEvent);
				Assert.AreEqual(buttonStates.RunButtonEnabled, runButton.Enabled);
				Assert.AreEqual(false, runButton.RaisesPostBack);
			}


			ConsoleButton retryErrorsButton = (ConsoleButton) console.Items[buttonIndex++];
			Assert.AreEqual(_RETRY_ERRORS, retryErrorsButton.DisplayText);
			Assert.AreEqual(buttonStates.RetryErrorsButtonEnabled, retryErrorsButton.Enabled);
			Assert.AreEqual(false, retryErrorsButton.RaisesPostBack);
			Assert.AreEqual(!hasJobsExecutingOrInQueue && integrationPoint.HasErrors.Value ? $"{_RETRY_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})" : string.Empty,
				retryErrorsButton.OnClickEvent);

			if (hasViewErrorsPermissions)
			{
				ConsoleButton viewErrorsButtonLink = (ConsoleButton) console.Items[buttonIndex++];
				Assert.AreEqual(_VIEW_ERRORS, viewErrorsButtonLink.DisplayText);
				Assert.AreEqual(buttonStates.ViewErrorsLinkEnabled, viewErrorsButtonLink.Enabled);
				Assert.AreEqual(false, viewErrorsButtonLink.RaisesPostBack);
				Assert.AreEqual("Really long string", viewErrorsButtonLink.OnClickEvent);
			}
		}

		[SmokeTest]
		[IdentifiedTestCase("4f3c01c6-e3bd-4e43-9295-0a0fb7cdaa70", true, true, ProviderType.FTP)]
		[IdentifiedTestCase("2610629f-3df2-4567-8cd0-ad892b5382c6", true, true, ProviderType.LDAP)]
		[IdentifiedTestCase("2e409a1b-3a3f-428a-88c6-97e56ca600ba", true, true, ProviderType.LoadFile)]
		[IdentifiedTestCase("751f865c-1468-4f96-83e6-f42553d16f5c", true, true, ProviderType.Other)]
		[IdentifiedTestCase("75328608-278d-4e5c-b8f8-e918fac9e301", true, false, ProviderType.FTP)]
		[IdentifiedTestCase("d703ff11-8577-4073-b511-28acd839febd", true, false, ProviderType.LDAP)]
		[IdentifiedTestCase("2e2a5eeb-74e7-4f01-85ea-c0dbabf33705", true, false, ProviderType.LoadFile)]
		[IdentifiedTestCase("6fd60aa8-7ff1-418d-82c1-d523b0791906", true, false, ProviderType.Other)]
		[IdentifiedTestCase("3c78d3ab-77eb-4aa0-bebd-9829c69337f2", false, false, ProviderType.FTP)]
		[IdentifiedTestCase("57d50bcf-f60e-4300-a54c-2549236ca4af", false, false, ProviderType.LDAP)]
		[IdentifiedTestCase("d78e80fc-3677-4a93-900f-128f0b0bc92f", false, false, ProviderType.LoadFile)]
		[IdentifiedTestCase("70cb002f-dbe5-499f-a578-34fad96a2f4b", false, false, ProviderType.Other)]
		public void GetConsole_NonRelativityProvider_GoldFlow(bool hasJobsExecutingOrInQueue, bool hasStoppableJobs, ProviderType providerType)
		{
			// ARRANGE
			var importSettings = new ImportSettings { ImageImport = false };
			var integrationPoint = new Data.IntegrationPoint
			{
				HasErrors = true,
				SourceProvider = 8392,
				DestinationProvider = 243,
				DestinationConfiguration = _serializer.Serialize(importSettings)
			};
			
			_managerFactory.CreateStateManager().Returns(_stateManager);
			_managerFactory.CreateQueueManager().Returns(_queueManager);
			_managerFactory.CreateJobHistoryManager().Returns(_jobHistoryManager);

			_helperClassFactory.CreateOnClickEventHelper(_managerFactory).Returns(_onClickEventHelper);

			_permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create).Returns(true);

			_integrationPointRepository.ReadWithFieldMappingAsync(_ARTIFACT_ID).Returns(integrationPoint);
			_providerTypeService.GetProviderType(integrationPoint.SourceProvider.Value, integrationPoint.DestinationProvider.Value).Returns(providerType);

			_permissionValidator.ValidateViewErrors(_APPLICATION_ID).Returns(new ValidationResult());

			StoppableJobCollection stoppableJobCollection = null;

			if (hasStoppableJobs)
			{
				stoppableJobCollection = new StoppableJobCollection
				{
					PendingJobArtifactIds = new[] {1232},
					ProcessingJobArtifactIds = new[] {9403}
				};
			}
			else
			{
				stoppableJobCollection = new StoppableJobCollection();
			}

			_jobHistoryManager.GetStoppableJobCollection(_APPLICATION_ID, _ARTIFACT_ID).Returns(stoppableJobCollection);
			_queueManager.HasJobsExecutingOrInQueue(_APPLICATION_ID, _ARTIFACT_ID).Returns(hasJobsExecutingOrInQueue);

			ButtonStateDTO buttonStates = null;
			OnClickEventDTO onClickEvents = null;

			buttonStates = new ButtonStateDTO
			{
				RunButtonEnabled = !hasJobsExecutingOrInQueue,
				StopButtonEnabled = hasStoppableJobs,
				ViewErrorsLinkVisible = false,
				RetryErrorsButtonVisible = false,
				ViewErrorsLinkEnabled = false,
				RetryErrorsButtonEnabled = false
			};

			_stateManager.GetButtonState(providerType, hasJobsExecutingOrInQueue, true, true, providerType == ProviderType.LoadFile && hasStoppableJobs, true).Returns(buttonStates);

			string actionButtonOnClickEvent;
			if (!hasJobsExecutingOrInQueue)
			{
				actionButtonOnClickEvent = $"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})";
			}
			else if (hasStoppableJobs)
			{
				actionButtonOnClickEvent = $"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})";
			}
			else
			{
				actionButtonOnClickEvent = string.Empty;
			}
			onClickEvents = new OnClickEventDTO
			{
				RunOnClickEvent = actionButtonOnClickEvent,
				StopOnClickEvent = actionButtonOnClickEvent
			};

			_onClickEventHelper.GetOnClickEvents(_APPLICATION_ID, _ARTIFACT_ID, _ARTIFACT_NAME, buttonStates)
				.Returns(onClickEvents);

			// ACT
			Console console = _instance.GetConsole(ConsoleEventHandler.PageEvent.Load);

			// ASSERT
			Assert.IsNotNull(console);

			int buttonCount = 1;
			Assert.AreEqual(buttonCount, console.Items.Count, $"There should be {buttonCount} buttons on the console");

			ConsoleButton actionButton = (ConsoleButton) console.Items[0];

			if (!hasJobsExecutingOrInQueue)
			{
				Assert.AreEqual(_RUN, actionButton.DisplayText);
				Assert.AreEqual(false, actionButton.RaisesPostBack);
				Assert.IsTrue(actionButton.Enabled);
				Assert.AreEqual($"{_RUN_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", actionButton.OnClickEvent);
			}
			else if (hasStoppableJobs)
			{
				Assert.AreEqual(_STOP, actionButton.DisplayText);
				Assert.AreEqual(false, actionButton.RaisesPostBack);
				Assert.IsTrue(actionButton.Enabled);
				Assert.AreEqual($"{_STOP_ENDPOINT}({_ARTIFACT_ID},{_APPLICATION_ID})", actionButton.OnClickEvent);
			}
			else
			{
				Assert.AreEqual(_STOP, actionButton.DisplayText);
				Assert.AreEqual(false, actionButton.RaisesPostBack);
				Assert.IsFalse(actionButton.Enabled);
				Assert.AreEqual(string.Empty, actionButton.OnClickEvent);
			}
		}
	}
}