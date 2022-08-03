using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
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
using Console = kCura.EventHandler.Console;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints
{
    [TestFixture, Category("Unit")]
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

        [TestCase(false, true, false, false, true)]
        [TestCase(false, true, true, false, true)]
        [TestCase(true, true, true, false, true)]
        [TestCase(false, false, false, false, true)]
        [TestCase(false, false, true, false, true)]
        [TestCase(true, false, true, false, true)]
        [TestCase(true, true, false, true, true)]
        [TestCase(true, true, true, true, true)]
        [TestCase(true, true, true, true, true)]
        [TestCase(true, false, false, true, true)]
        [TestCase(true, false, true, true, true)]
        [TestCase(true, false, true, true, true)]
        [TestCase(true, false, true, true, false)]
        public void GetConsole_RelativityProvider_GoldFlow(bool hasJobsExecutingOrInQueue, bool hasRunPermissions, bool hasViewErrorsPermissions, bool hasStoppableJobs,
            bool hasProfileAddPermission)
        {
            // ARRANGE
            var sourceConfiguration = new SourceConfiguration()
            {
                TypeOfExport = SourceConfiguration.ExportType.SavedSearch
            };
            var importSettings = new ImportSettings { ImageImport = false };
            var integrationPoint = new Data.IntegrationPoint
            {
                HasErrors = true,
                SourceProvider = 8392,
                DestinationProvider = 437,
                SourceConfiguration = _serializer.Serialize(sourceConfiguration),
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

            StoppableJobHistoryCollection stoppableJobCollection = null;

            if (hasStoppableJobs)
            {
                stoppableJobCollection = new StoppableJobHistoryCollection
                {
                    PendingJobHistory = new[] { new JobHistory { ArtifactId = 1232 } },
                    ProcessingJobHistory = new[] { new JobHistory { ArtifactId = 9403 } }
                };
            }
            else
            {
                stoppableJobCollection = new StoppableJobHistoryCollection();
            }

            _jobHistoryManager.GetStoppableJobHistory(_APPLICATION_ID, _ARTIFACT_ID).Returns(stoppableJobCollection);

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
                    sourceConfiguration.TypeOfExport,
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


            _jobHistoryManager.Received(1).GetStoppableJobHistory(_APPLICATION_ID, _ARTIFACT_ID);

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
        [TestCase(true, true, ProviderType.FTP)]
        [TestCase(true, true, ProviderType.LDAP)]
        [TestCase(true, true, ProviderType.LoadFile)]
        [TestCase(true, true, ProviderType.Other)]
        [TestCase(true, false, ProviderType.FTP)]
        [TestCase(true, false, ProviderType.LDAP)]
        [TestCase(true, false, ProviderType.LoadFile)]
        [TestCase(true, false, ProviderType.Other)]
        [TestCase(false, false, ProviderType.FTP)]
        [TestCase(false, false, ProviderType.LDAP)]
        [TestCase(false, false, ProviderType.LoadFile)]
        [TestCase(false, false, ProviderType.Other)]
        public void GetConsole_NonRelativityProvider_GoldFlow(bool hasJobsExecutingOrInQueue, bool hasStoppableJobs, ProviderType providerType)
        {
            // ARRANGE
            var sourceConfiguration = new SourceConfiguration()
            {
                TypeOfExport = SourceConfiguration.ExportType.SavedSearch
            };
            var importSettings = new ImportSettings { ImageImport = false };
            var integrationPoint = new Data.IntegrationPoint
            {
                HasErrors = true,
                SourceProvider = 8392,
                DestinationProvider = 243,
                SourceConfiguration = _serializer.Serialize(sourceConfiguration),
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

            StoppableJobHistoryCollection stoppableJobCollection = null;

            if (hasStoppableJobs)
            {
                stoppableJobCollection = new StoppableJobHistoryCollection
                {
                    PendingJobHistory = new[] { new JobHistory { ArtifactId = 1232 } },
                    ProcessingJobHistory = new[] { new JobHistory { ArtifactId = 9403 } }
                };
            }
            else
            {
                stoppableJobCollection = new StoppableJobHistoryCollection();
            }

            _jobHistoryManager.GetStoppableJobHistory(_APPLICATION_ID, _ARTIFACT_ID).Returns(stoppableJobCollection);
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

            _stateManager
                .GetButtonState(sourceConfiguration.TypeOfExport, providerType, hasJobsExecutingOrInQueue, true, true, providerType == ProviderType.LoadFile && hasStoppableJobs, true)
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