using System;
using System.Collections.Generic;
using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.WinEDDS.Api;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using Relativity.AutomatedWorkflows.Services.Interfaces;
using Relativity.AutomatedWorkflows.Services.Interfaces.DataContracts.Triggers;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using kCura.IntegrationPoints.Core.Contracts.Import;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture, Category("Unit")]
	[Description("These tests were modeled after unit tests in ExportServiceManagerTests")]
	public class ImportServiceManagerTests : TestBase
	{
		private Data.IntegrationPoint _integrationPoint;
		private IDataSynchronizer _synchronizer;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IImportFileLocationService _importFileLocationService;

		private ImportServiceManager _instance;

		private Job _job;
		private LoadFileInfo _loadFile;
		private TaskParameters _taskParameters;
		private IHelper _helper;
		private IRetryHandler _retryHandler;
		private IRetryHandlerFactory _retryHandlerFactory;
		private IObjectManager _objectManager;
		private IAutomatedWorkflowsService _automatedWorkflowsService;
		private IJobStatusUpdater _jobStatusUpdater;
		private const int _RECORD_COUNT = 42;
		private const string _ERROR_FILE_PATH = "ErrorFilePath";
		private const string _IMPORT_PROVIDER_SETTINGS_FOR_DOC = "DocumentImport";
		private const string _IMPORT_PROVIDER_SETTINGS_FOR_IMAGE = "ImageImport";
		private const string _IMPORTSETTINGS_FOR_DOC = "DocumentImport";
		private const string _IMPORTSETTINGS_FOR_IMAGE = "ImageImport";
		
		private const string _LOAD_FILE_PATH = "LoadFilePath";
		private const long _LOAD_FILE_SIZE = 1000;
		private readonly DateTime _LOAD_FILE_MODIFIED_DATE = new DateTime(2020, 1, 1);

		private const int _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID = 1000014;
		private const string _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION = "'Name' == 'Automated Workflows'";


		[SetUp]
		public override void SetUp()
		{
			Job job = JobExtensions.CreateJob();
			SetUp(job);
		}

		private void SetUp(Job job)
		{
			_job = job;

			_loadFile = new LoadFileInfo
			{
				FullPath = _LOAD_FILE_PATH,
				Size = _LOAD_FILE_SIZE,
				LastModifiedDate = _LOAD_FILE_MODIFIED_DATE
			};

			_helper = Substitute.For<IHelper>();

			_retryHandler = Substitute.For<IRetryHandler>();
			_retryHandler.ExecuteWithRetriesAsync(Arg.Any<Func<Task>>(), Arg.Any<string>()).Returns(callInfo => ((Func<Task>)callInfo[0])());

			_retryHandlerFactory = Substitute.For<IRetryHandlerFactory>();
			_retryHandlerFactory.Create().ReturnsForAnyArgs(_retryHandler);

			_objectManager = Substitute.For<IObjectManager>();
			_helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System).Returns(_objectManager);

			_automatedWorkflowsService = Substitute.For<IAutomatedWorkflowsService>();
			_helper.GetServicesManager().CreateProxy<IAutomatedWorkflowsService>(ExecutionIdentity.System).Returns(_automatedWorkflowsService);

			_jobStatusUpdater = Substitute.For<IJobStatusUpdater>();

			ICaseServiceContext caseContext = Substitute.For<ICaseServiceContext>();
			ISynchronizerFactory synchronizerFactory = Substitute.For<ISynchronizerFactory>();
			IManagerFactory managerFactory = Substitute.For<IManagerFactory>();
			IIntegrationPointRepository integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

			IBatchStatus sendingEmailNotification = Substitute.For<IBatchStatus>();
			IBatchStatus updateJobHistoryStatus = Substitute.For<IBatchStatus>();
			IEnumerable<IBatchStatus> batchStatuses = new List<IBatchStatus>() { sendingEmailNotification, updateJobHistoryStatus };
			ISerializer serializer = Substitute.For<ISerializer>();
			IJobService jobService = Substitute.For<IJobService>();

			IScheduleRuleFactory scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();

			_importFileLocationService = Substitute.For<IImportFileLocationService>();
			_importFileLocationService.ErrorFilePath(Arg.Any<Data.IntegrationPoint>()).Returns(_ERROR_FILE_PATH);
			_importFileLocationService.LoadFileInfo(Arg.Any<Data.IntegrationPoint>()).Returns(_loadFile);

			IJobStopManager jobStopManager = Substitute.For<IJobStopManager>();
			_synchronizer = Substitute.For<IDataSynchronizer>();
			IDataReaderFactory dataReaderFactory = Substitute.For<IDataReaderFactory>();
			
			IDataReader loadFileReader = Substitute.For<IDataReader, IArtifactReader>();
			IDataReader opticonFileReader = Substitute.For<IDataReader, IOpticonDataReader>();
			IAgentValidator agentValidator = Substitute.For<IAgentValidator>();
			((IArtifactReader)loadFileReader).CountRecords().Returns(_RECORD_COUNT);
			((IOpticonDataReader)opticonFileReader).CountRecords().Returns(_RECORD_COUNT);

			JobStatisticsService jobStatisticsService = Substitute.For<JobStatisticsService>();

			dataReaderFactory.GetDataReader(Arg.Any<FieldMap[]>(), _IMPORT_PROVIDER_SETTINGS_FOR_DOC).Returns(loadFileReader);
			dataReaderFactory.GetDataReader(Arg.Any<FieldMap[]>(), _IMPORT_PROVIDER_SETTINGS_FOR_IMAGE).Returns(opticonFileReader);

			object syncRootLock = new object();
			_integrationPoint = new Data.IntegrationPoint()
			{
				SourceConfiguration = "source config",
				DestinationConfiguration = "destination config",
				SourceProvider = 741,
				FieldMappings = "mapping",
				SecuredConfiguration = "secured config"
			};
			SourceConfiguration configuration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = 8465,
				SavedSearchArtifactId = 987654
			};

			_taskParameters = new TaskParameters 
			{ 
				BatchInstance = Guid.NewGuid(),
				BatchParameters = new LoadFileTaskParameters
				{
					LastModifiedDate = _LOAD_FILE_MODIFIED_DATE,
					Size = _LOAD_FILE_SIZE
				}
			};
			JobHistory jobHistory = new JobHistory() { JobType = JobTypeChoices.JobHistoryRun, TotalItems = 0 };
			SourceProvider sourceProvider = new SourceProvider();
			List<FieldMap> mappings = new List<FieldMap>();

			integrationPointRepository.ReadWithFieldMappingAsync(job.RelatedObjectArtifactID).Returns(_integrationPoint);
			serializer.Deserialize<SourceConfiguration>(_integrationPoint.SourceConfiguration).Returns(configuration);
			serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(_taskParameters);
			jobHistoryService.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _taskParameters.BatchInstance, Arg.Any<DateTime>()).Returns(jobHistory);
			caseContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider.Value).Returns(sourceProvider);
			serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(mappings);
			synchronizerFactory.CreateSynchronizer(Arg.Any<Guid>(), Arg.Any<string>()).Returns(_synchronizer);
			managerFactory.CreateJobStopManager(jobService, jobHistoryService, _taskParameters.BatchInstance, job.JobId, Arg.Any<bool>()).Returns(jobStopManager);

			ImportSettings imageSettings = new ImportSettings();
			imageSettings.ImageImport = true;
			ImportSettings documentSettings = new ImportSettings();
			imageSettings.ImageImport = false;
			serializer.Deserialize<ImportSettings>(_IMPORTSETTINGS_FOR_DOC).Returns(documentSettings);
			serializer.Deserialize<ImportSettings>(_IMPORTSETTINGS_FOR_IMAGE).Returns(imageSettings);
			serializer.Serialize(documentSettings).Returns(_IMPORTSETTINGS_FOR_DOC);
			serializer.Serialize(imageSettings).Returns(_IMPORTSETTINGS_FOR_IMAGE);
			ImportProviderSettings providerSettingsForDoc = new ImportProviderSettings();
			ImportProviderSettings providerSettingsForImage = new ImportProviderSettings();
			providerSettingsForDoc.LineNumber = "0";
			providerSettingsForImage.LineNumber = "0";
			serializer.Deserialize<ImportProviderSettings>(_IMPORT_PROVIDER_SETTINGS_FOR_DOC).Returns(providerSettingsForDoc);
			serializer.Serialize(providerSettingsForDoc).Returns(_IMPORT_PROVIDER_SETTINGS_FOR_DOC);
			serializer.Deserialize<ImportProviderSettings>(_IMPORT_PROVIDER_SETTINGS_FOR_IMAGE).Returns(providerSettingsForImage);
			serializer.Serialize(providerSettingsForImage).Returns(_IMPORT_PROVIDER_SETTINGS_FOR_IMAGE);

			jobStopManager.SyncRoot.Returns(syncRootLock);
			serializer.Deserialize<TaskParameters>(job.JobDetails)
				.Returns(_taskParameters);
			jobHistoryService.GetRdo(Arg.Is<Guid>(guid => guid == _taskParameters.BatchInstance)).Returns(jobHistory);
			_instance = new ImportServiceManager(
				_helper,
				_retryHandlerFactory,
				caseContext,
				synchronizerFactory,
				managerFactory,
				batchStatuses,
				serializer,
				jobService,
				scheduleRuleFactory,
				jobHistoryService,
				_jobHistoryErrorService,
				jobStatisticsService,
				dataReaderFactory,
				_importFileLocationService,
				agentValidator,
				integrationPointRepository,
				_jobStatusUpdater);
		}

		[Test]
		public void Execute_GoldFlow_CreateDataReaderAndPassItToSynchronizer()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_synchronizer.Received(1).SyncData(Arg.Any<IDataTransferContext>(), Arg.Any<List<FieldMap>>(), Arg.Any<string>());
		}

		[Test]
		public void Execute_JobHistoryErrorServiceSubscriptionIsSetup()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			// ACT
			_instance.Execute(_job);

			// ASSERT
			_jobHistoryErrorService.Received(1).SubscribeToBatchReporterEvents(_synchronizer);
		}

		[Test]
		public void Execute_ShouldTriggerRawAsCompleted_WhenRunCompletedAndAutomatedWorkflowsIsInstalled()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryCompleted);
			_objectManager.QuerySlimAsync(
				Arg.Any<int>(),
				Arg.Is<QueryRequest>(qr => qr.ObjectType.ArtifactTypeID == _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID && qr.Condition == _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION),
				Arg.Is(0),
				Arg.Is(0)
			).Returns(Task.FromResult(new QueryResultSlim { TotalCount = 1 }));

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.Received().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Is<SendTriggerBody>(stb => stb.State == ImportServiceManager.RAW_STATE_COMPLETE));
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunCompletedAndAutomatedWorkflowsIsNotInstalled()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryCompleted);
			_objectManager.QuerySlimAsync(
				Arg.Any<int>(),
				Arg.Is<QueryRequest>(qr => qr.ObjectType.ArtifactTypeID == _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID && qr.Condition == _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION),
				Arg.Is(0),
				Arg.Is(0)
			).Returns(Task.FromResult(new QueryResultSlim { TotalCount = 0 }));

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}


		[Test]
		public void Execute_ShouldTriggerRawAsCompletedWithErrors_WhenRunCompletedWithErrorsAndAutomatedWorkflowsIsInstalled()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryCompletedWithErrors);
			_objectManager.QuerySlimAsync(
				Arg.Any<int>(),
				Arg.Is<QueryRequest>(qr => qr.ObjectType.ArtifactTypeID == _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID && qr.Condition == _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION),
				Arg.Is(0),
				Arg.Is(0)
			).Returns(Task.FromResult(new QueryResultSlim { TotalCount = 1 }));

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.Received().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Is<SendTriggerBody>(stb => stb.State == ImportServiceManager.RAW_STATE_COMPLETE_WITH_ERRORS));
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunCompletedWithErrorsAndAutomatedWorkflowsIsNotInstalled()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryCompletedWithErrors);
			_objectManager.QuerySlimAsync(
				Arg.Any<int>(),
				Arg.Is<QueryRequest>(qr => qr.ObjectType.ArtifactTypeID == _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID && qr.Condition == _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION),
				Arg.Is(0),
				Arg.Is(0)
			).Returns(Task.FromResult(new QueryResultSlim { TotalCount = 0 }));

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunFailed()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryErrorJobFailed);

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunPending()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryPending);

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunProcessing()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryProcessing);

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunStopped()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryStopped);

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunStopping()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryStopping);

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunValidating()
		{
			// ARRANGE
			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryValidating);

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}

		[Test]
		public void Execute_ShouldNotTriggerRaw_WhenRunValidationFailed()
		{
			// ARRANGE
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			_jobStatusUpdater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryValidationFailed);

			// ACT
			_instance.Execute(_job);

			_automatedWorkflowsService.DidNotReceive().SendTriggerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<SendTriggerBody>());
		}

		[Test]
		public void Execute_ShouldThrowValidationFailed_WhenLoadFileSizeHasChanged()
		{
			// Arrange
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			const int sizeChanged = 10;

			_importFileLocationService.LoadFileInfo(Arg.Any<Data.IntegrationPoint>())
				.Returns(new LoadFileInfo
				{
					FullPath = _LOAD_FILE_PATH,
					Size = _LOAD_FILE_SIZE + sizeChanged
				});

			// Act & Assert
			Assert.Throws<IntegrationPointValidationException>(() => _instance.Execute(_job));
		}

		[Test]
		public void Execute_ShouldThrowValidationFailed_WhenLoadFileModifiedDateHasChanged()
		{
			// Arrange
			_integrationPoint.DestinationConfiguration = _IMPORTSETTINGS_FOR_DOC;
			_integrationPoint.SourceConfiguration = _IMPORT_PROVIDER_SETTINGS_FOR_DOC;

			const int modifiedMinutesLater = 10;

			_importFileLocationService.LoadFileInfo(Arg.Any<Data.IntegrationPoint>())
				.Returns(new LoadFileInfo
				{
					FullPath = _LOAD_FILE_PATH,
					Size = _LOAD_FILE_SIZE,
					LastModifiedDate = _LOAD_FILE_MODIFIED_DATE.AddMinutes(modifiedMinutesLater)
				});

			// Act & Assert
			Assert.Throws<IntegrationPointValidationException>(() => _instance.Execute(_job));
		}
	}
}
