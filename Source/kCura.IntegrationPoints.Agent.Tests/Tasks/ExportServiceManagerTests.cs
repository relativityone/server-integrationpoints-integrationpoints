﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.DataReaderClient;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Choice;
using static kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
    [TestFixture, Category("Unit")]
    [Description("IMPORTANT" +
                 "These existing tests will show that they cover majority of the code. " +
                 "But the tests below are only consist of the stopping scenarios and regular gold flow." +
                 "A lot more tests must be added !")]
    public class ExportServiceManagerTests : TestBase
    {
        private IntegrationPointDto _integrationPointDto;
        private ExportServiceManager _instance;
        private IAgentValidator _agentValidator;
        private IBatchStatus _exportServiceObserver;
        private IBatchStatus _sendingEmailNotification;
        private IBatchStatus _updateJobHistoryStatus;
        private ICaseServiceContext _caseContext;
        private IDataSynchronizer _synchronizer;
        private IDocumentRepository _documentRepository;
        private IEnumerable<IBatchStatus> _batchStatuses;
        private IExporterFactory _exporterFactory;
        private IExportServiceObserversFactory _exportServiceObserversFactory;
        private IExporterService _exporterService;
        private IHelper _helper;
        private IIntegrationPointService _integrationPointService;
        private IJobHistoryErrorManager _jobHistoryErrorManager;
        private IJobHistoryErrorRepository _jobHistoryErrorRepository;
        private IJobHistoryErrorService _jobHistoryErrorService;
        private IJobHistoryManager _historyManager;
        private IJobHistoryService _jobHistoryService;
        private IJobService _jobService;
        private IJobStopManager _jobStopManager;
        private IManagerFactory _managerFactory;
        private IRepositoryFactory _repositoryFactory;
        private ISavedSearchQueryRepository _savedSearchQueryRepository;
        private IScheduleRuleFactory _scheduleRuleFactory;
        private ISerializer _serializer;
        private IExportDataSanitizer _exportDataSanitizer;
        private IAPILog _logger;
        private Job _job;
        private JobHistory _jobHistory;
        private JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
        private JobStatisticsService _jobStatisticsService;
        private SourceConfiguration _configuration;
        private SourceProvider _sourceProvider;
        private TaskParameters _taskParameters;
        private ImportSettings _importSettings;
        private const int _EXPORT_DOC_COUNT = 0;
        private const int _RETRY_SAVEDSEARCHID = 312;
        private const string _IMPORTSETTINGS_WITH_USERID = "blah blah blah";

        [SetUp]
        public override void SetUp()
        {
            Job job = JobExtensions.CreateJob();
            SetUp(job);
        }

        private void SetUp(Job job)
        {
            _job = job;
            _helper = Substitute.For<IHelper>();
            _logger = Substitute.For<IAPILog>();
            ILogFactory logFactory = Substitute.For<ILogFactory>();
            logFactory.GetLogger().Returns(_logger);
            _helper.GetLoggerFactory().Returns(logFactory);
            _logger.ForContext<ServiceManagerBase>().Returns(_logger);
            _caseContext = Substitute.For<ICaseServiceContext>();
            ISynchronizerFactory synchronizerFactory = Substitute.For<ISynchronizerFactory>();
            _exporterFactory = Substitute.For<IExporterFactory>();
            _exportServiceObserversFactory = Substitute.For<IExportServiceObserversFactory>();
            ITagsCreator tagsCreator = Substitute.For<ITagsCreator>();
            ITagSavedSearchManager tagSavedSearchManager = Substitute.For<ITagSavedSearchManager>();
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _managerFactory = Substitute.For<IManagerFactory>();
            _integrationPointService = Substitute.For<IIntegrationPointService>();
            _documentRepository = Substitute.For<IDocumentRepository>();
            _exportDataSanitizer = Substitute.For<IExportDataSanitizer>();

            _sendingEmailNotification = Substitute.For<IBatchStatus>();
            _updateJobHistoryStatus = Substitute.For<IBatchStatus>();
            _batchStatuses = new List<IBatchStatus>() { _sendingEmailNotification, _updateJobHistoryStatus };
            _serializer = Substitute.For<ISerializer>();
            _jobService = Substitute.For<IJobService>();
            _scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
            _jobHistoryService = Substitute.For<IJobHistoryService>();
            _jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();

            _jobHistoryErrorManager = Substitute.For<IJobHistoryErrorManager>();
            _savedSearchQueryRepository = Substitute.For<ISavedSearchQueryRepository>();
            _jobStopManager = Substitute.For<IJobStopManager>();
            IDocumentRepository documentRepository = Substitute.For<IDocumentRepository>();
            IWorkspaceRepository workspaceRepository = Substitute.For<IWorkspaceRepository>();
            _exporterService = Substitute.For<IExporterService>();
            _repositoryFactory.GetWorkspaceRepository().Returns(workspaceRepository);
            _jobHistoryErrorRepository = Substitute.For<IJobHistoryErrorRepository>();
            _exportServiceObserver = Substitute.For<IBatchStatus>();
            _synchronizer = Substitute.For<IDataSynchronizer>();
            _historyManager = Substitute.For<IJobHistoryManager>();
            _agentValidator = Substitute.For<IAgentValidator>();
            _jobStatisticsService = Substitute.For<JobStatisticsService>();
            ISourceWorkspaceTagCreator sourceWorkspaceTagsCreator = Substitute.For<ISourceWorkspaceTagCreator>();

            var exportJobObservers = new List<IBatchStatus>
            {
                _exportServiceObserver
            };

            _exportServiceObserversFactory
                .InitializeExportServiceJobObservers(
                    Arg.Any<Job>(),
                    tagsCreator,
                    tagSavedSearchManager,
                    synchronizerFactory,
                    _serializer,
                    _jobHistoryErrorManager,
                    _jobStopManager,
                    sourceWorkspaceTagsCreator,
                    Arg.Any<FieldMap[]>(),
                    Arg.Any<SourceConfiguration>(),
                    Arg.Any<JobHistoryErrorDTO.UpdateStatusType>(),
                    Arg.Any<JobHistory>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(exportJobObservers);

            _integrationPointDto = new IntegrationPointDto()
            {
                SourceConfiguration = "source config",
                DestinationConfiguration = "destination config",
                SourceProvider = 741,
                SecuredConfiguration = "secured config",
                FieldMappings = new List<FieldMap>(),
            };
            _configuration = new SourceConfiguration()
            {
                SourceWorkspaceArtifactId = 8465,
                SavedSearchArtifactId = 987654,
                TypeOfExport = SourceConfiguration.ExportType.SavedSearch
            };

            _taskParameters = new TaskParameters() { BatchInstance = Guid.NewGuid() };
            _jobHistory = new JobHistory() { JobType = JobTypeChoices.JobHistoryRun, TotalItems = 0, Overwrite = OverwriteModeNames.AppendOnlyModeName };
            _sourceProvider = new SourceProvider();
            _updateStatusType = new JobHistoryErrorDTO.UpdateStatusType();

            _integrationPointService.Read(job.RelatedObjectArtifactID).Returns(_integrationPointDto);
            _serializer.Deserialize<SourceConfiguration>(_integrationPointDto.SourceConfiguration).Returns(_configuration);
            _serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(_taskParameters);
            _jobHistoryService.GetOrCreateScheduledRunHistoryRdo(_integrationPointDto, _taskParameters.BatchInstance, Arg.Any<DateTime>()).Returns(_jobHistory);
            _caseContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(_integrationPointDto.SourceProvider).Returns(_sourceProvider);
            _managerFactory.CreateJobHistoryErrorManager(_configuration.SourceWorkspaceArtifactId, GetUniqueJobId(job, _taskParameters.BatchInstance)).Returns(_jobHistoryErrorManager);
            _jobHistoryErrorManager.StageForUpdatingErrors(job, Arg.Is<ChoiceRef>(obj => obj.EqualsToChoice(JobTypeChoices.JobHistoryRun))).Returns(_updateStatusType);
            _repositoryFactory.GetSavedSearchQueryRepository(_configuration.SourceWorkspaceArtifactId).Returns(_savedSearchQueryRepository);
            _savedSearchQueryRepository.RetrieveSavedSearch(_configuration.SavedSearchArtifactId).Returns(new SavedSearchDTO());
            _repositoryFactory.GetJobHistoryErrorRepository(_configuration.SourceWorkspaceArtifactId).Returns(_jobHistoryErrorRepository);
            _jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job, _configuration.SavedSearchArtifactId).Returns(_RETRY_SAVEDSEARCHID);
            synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _integrationPointDto.DestinationConfiguration).Returns(_synchronizer);
            _managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _taskParameters.BatchInstance, job.JobId, Arg.Any<bool>(), Arg.Any<IDiagnosticLog>()).Returns(_jobStopManager);

            _importSettings = new ImportSettings();
            _serializer.Deserialize<ImportSettings>(_integrationPointDto.DestinationConfiguration).Returns(_importSettings);
            _serializer.Serialize(_importSettings).Returns(_IMPORTSETTINGS_WITH_USERID);

            _repositoryFactory.GetDocumentRepository(_configuration.SourceWorkspaceArtifactId).Returns(documentRepository);

            _exporterFactory.BuildExporter(
                    _jobStopManager,
                    Arg.Any<FieldMap[]>(),
                    _integrationPointDto.SourceConfiguration,
                    _configuration.SavedSearchArtifactId,
                    _IMPORTSETTINGS_WITH_USERID,
                    _documentRepository,
                    _exportDataSanitizer)
                .Returns(_exporterService);

            _exporterService.TotalRecordsFound.Returns(_EXPORT_DOC_COUNT);
            _serializer.Deserialize<TaskParameters>(job.JobDetails)
                .Returns(_taskParameters);
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Is<Guid>(guid => guid == _taskParameters.BatchInstance)).Returns(_jobHistory);

            _managerFactory.CreateTagsCreator().Returns(tagsCreator);
            _managerFactory.CreateTaggingSavedSearchManager().Returns(tagSavedSearchManager);
            _managerFactory.CreateSourceWorkspaceTagsCreator(Arg.Any<SourceConfiguration>()).Returns(sourceWorkspaceTagsCreator);

            _instance = new ExportServiceManager(
                _helper,
                _caseContext,
                synchronizerFactory,
                _exporterFactory,
                _exportServiceObserversFactory,
                _repositoryFactory,
                _managerFactory,
                _batchStatuses,
                _serializer,
                _jobService,
                _scheduleRuleFactory,
                _jobHistoryService,
                _jobHistoryErrorService,
                _jobStatisticsService,
                toggleProvider: null,
                agentValidator: _agentValidator,
                integrationPointService: _integrationPointService,
                documentRepository: _documentRepository,
                exportDataSanitizer: _exportDataSanitizer,
                diagnosticLog: new EmptyDiagnosticLog());
            _managerFactory.CreateJobHistoryManager().Returns(_historyManager);
        }

        [Test]
        public void Execute_FailToLoadIntegrationPointRDO()
        {
            // ARRANGE
            _integrationPointService.Read(_job.RelatedObjectArtifactID).Returns((IntegrationPointDto)null);

            // ACT
            _instance.Execute(_job);

            // ASSERT
            AssertJobHistoryErrorServiceReceivedException<ArgumentException>("Failed to retrieve corresponding Integration Point.");
            _jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable);
            _jobHistoryErrorService.Received().CommitErrors();
        }

        [Test]
        public void Execute_EnsureToSanatizeFieldMappings()
        {
            // ARRANGE
            _integrationPointDto.FieldMappings = new List<FieldMap>
            {
                new FieldMap()
                {
                    SourceField = new FieldEntry() { DisplayName = "source" },
                    DestinationField =  new FieldEntry() { DisplayName = "destination" },
                    FieldMapType = FieldMapTypeEnum.Identifier
                }
            };

            // ACT
            _instance.Execute(_job);

            // ASSERT
            Assert.IsTrue(_integrationPointDto.FieldMappings[0].SourceField.IsIdentifier);
        }

        [Test]
        [TestCase(OverwriteModeNames.AppendOnlyModeName, ImportOverwriteModeEnum.AppendOnly)]
        [TestCase(OverwriteModeNames.AppendOverlayModeName, ImportOverwriteModeEnum.AppendOverlay)]
        [TestCase(OverwriteModeNames.OverlayOnlyModeName, ImportOverwriteModeEnum.OverlayOnly)]
        public void Execute_EnsureToAssignCorrectOverwriteModeFromJobHistory(string initialOverwriteMode, ImportOverwriteModeEnum expectedOverwriteSetting)
        {
            // ARRANGE
            _jobHistory.Overwrite = initialOverwriteMode;

            // ACT
            _instance.Execute(_job);

            // ASSERT
            Assert.AreEqual(expectedOverwriteSetting, _importSettings.ImportOverwriteMode);
        }

        [Test]
        public void Execute_EnsureToCheckTheExistentOfSavedSearch()
        {
            // ARRANGE
            _savedSearchQueryRepository.RetrieveSavedSearch(_configuration.SavedSearchArtifactId).Returns((SavedSearchDTO)null);

            // ACT
            Assert.Throws<IntegrationPointsException>(() => _instance.Execute(_job));

            // ASSERT
            AssertJobHistoryErrorServiceReceivedException<Exception>(
                Core.Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NO_ACCESS);
            _jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable);
            _jobHistoryErrorService.Received().CommitErrors();
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void Execute_StopAtTheVeryBeginningOfTheJob()
        {
            // ARRANGE
            _jobStopManager.When(obj => obj.ThrowIfStopRequested())
                .Do(info => { throw new OperationCanceledException(); });

            // ACT
            _instance.Execute(_job);

            // ASSERT
            Assert.AreEqual(0, _jobHistory.TotalItems);
            AssertFinalizedJob(_job);
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void Execute_StopAfterAcquiringTheSynchronizer()
        {
            // ARRANGE
            _jobStopManager.When(obj => obj.ThrowIfStopRequested())
                .Do(Callback.First(x => { })
                .Then(info => { throw new OperationCanceledException(); }));

            // ACT
            _instance.Execute(_job);

            // ASSERT
            Assert.AreEqual(0, _jobHistory.TotalItems);
            AssertFinalizedJob(_job);
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void Execute_StopBeforeExecutePushingData()
        {
            // ARRANGE

            _jobStopManager.When(obj => obj.ThrowIfStopRequested())
                .Do(Callback.First(x => { })
                .Then(x => { })
                .Then(info => { throw new OperationCanceledException(); }));

            // ACT
            _instance.Execute(_job);

            // ASSERT
            Assert.AreEqual(0, _jobHistory.TotalItems);
            _jobHistoryErrorService.DidNotReceive().AddError(Arg.Any<ChoiceRef>(), Arg.Any<Exception>());
            _jobHistoryErrorService.DidNotReceive().AddError(Arg.Any<ChoiceRef>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            AssertFinalizedJob(_job);
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void Execute_NoStopRequested()
        {
            // ACT
            _instance.Execute(_job);

            // ASSERT
            EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();
            EnsureToUpdateTotalItemCount();
            ExporterServiceObjectIsFinalized();
            // do not tag any errors
            _historyManager.DidNotReceive().SetErrorStatusesToExpired(Arg.Any<int>(), Arg.Any<int>());
            AssertFinalizedJob(_job);
        }

        [Test]
        public void Execute_ExportServiceJobObserverFailToInitialize()
        {
            // ARRANGE
            const string exceptionMessage = "exception !";
            _exportServiceObserver.When(observer => observer.OnJobStart(_job)).Do(info => { throw new Exception(exceptionMessage); });

            // ACT
            _instance.Execute(_job);

            // ASSERT
            AssertJobHistoryErrorServiceReceivedAggregateException(exceptionMessage);
            _jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable);
            _jobHistoryErrorService.Received().CommitErrors();
        }

        [Test]
        public void Execute_ExportServiceJobObserverFailToFinalized()
        {
            // ARRANGE
            const string exceptionMessage = "exception !";
            _exportServiceObserver.When(observer => observer.OnJobComplete(_job)).Do(info => { throw new Exception(exceptionMessage); });

            // ACT
            _instance.Execute(_job);

            // ASSERT
            AssertFinalizedJob(_job);
            AssertJobHistoryErrorServiceReceivedAggregateException(exceptionMessage);
        }

        [Test]
        public void Execute_NewSavedSearchIsCreatedOnItemLevelErrorRetry()
        {
            // ARRANGE
            _updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;
            _updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;

            // ACT
            _instance.Execute(_job);

            // ASSERT
            AssertRetrySavedSearch(expectToCreate: true);
        }

        [Test]
        public void Execute_ItemLevelErrorSavedSearchIsNotDeletedWhenItFailsToCreateOne()
        {
            // ARRANGE
            _updateStatusType.ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;
            _updateStatusType.JobType = JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors;

            var exception = new Exception();
            _jobHistoryErrorManager
                .CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId)
                .Throws(exception);

            // ACT
            _instance.Execute(_job);

            // ASSERT
            _jobHistoryErrorManager.Received(1).CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId);
            _exporterFactory.Received(0).BuildExporter(
                _jobStopManager,
                Arg.Any<FieldMap[]>(),
                _integrationPointDto.SourceConfiguration,
                _RETRY_SAVEDSEARCHID,
                _IMPORTSETTINGS_WITH_USERID,
                _documentRepository,
                _exportDataSanitizer);
            _jobHistoryErrorRepository.Received(0).DeleteItemLevelErrorsSavedSearch(Arg.Any<int>());
            _logger.LogError(
                exception,
                "Failed to delete temp Saved Search {SavedSearchArtifactId}.",
                _configuration.SavedSearchArtifactId
            );
        }

        [TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run)]
        [TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run)]
        [TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run)]
        [TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.Run)]
        [TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.None, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)]
        [TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)]
        [TestCase(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobOnly, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors)]
        public void Execute_NoRetrySavedSearchCreated(JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices errorType, JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices jobType)
        {
            // ARRANGE
            _updateStatusType.ErrorTypes = errorType;
            _updateStatusType.JobType = jobType;

            // ACT
            _instance.Execute(_job);

            // ASSERT
            AssertRetrySavedSearch(expectToCreate: false);
        }

        [Test]
        [Description("This happens when GeneralWithEntityRdoSynchronizerFactory is passed in.")]
        public void Execute_CreateDestinationProvider_MakeSureToSetSourceProvider()
        {
            // ARRANGE
            IDiagnosticLog diagnosticLog = new EmptyDiagnosticLog();

            IWindsorContainer windsorContainer = Substitute.For<IWindsorContainer>();
            IObjectTypeRepository objectTypeRepository = Substitute.For<IObjectTypeRepository>();
            ISynchronizerFactory synchronizerFactory = Substitute.For<GeneralWithEntityRdoSynchronizerFactory>(windsorContainer, objectTypeRepository, diagnosticLog);

            // ACT
            ExportServiceManager instance = new ExportServiceManager(
                _helper,
                _caseContext,
                synchronizerFactory,
                _exporterFactory,
                _exportServiceObserversFactory,
                _repositoryFactory,
                _managerFactory,
                _batchStatuses,
                _serializer,
                _jobService,
                _scheduleRuleFactory,
                _jobHistoryService,
                _jobHistoryErrorService,
                _jobStatisticsService,
                null,
                _agentValidator,
                _integrationPointService,
                _documentRepository,
                _exportDataSanitizer,
                diagnosticLog);
            try
            {
                instance.Execute(_job);
            }
            catch (Exception)
            {
                // Ignore any errors - we want to check just the assertions below
            }

            // ASSERT
            var factory = synchronizerFactory as GeneralWithEntityRdoSynchronizerFactory;
            Assert.IsNotNull(factory);
            Assert.AreSame(factory.SourceProvider, _sourceProvider);
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void Execute_FailToSetJobStateAsUnstoppable_OnFinalizeExportServiceObservers()
        {
            // ARRANGE
            _jobService.When(service =>
               service.UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(_job.JobId)), StopState.Unstoppable))
                .Throw<Exception>();

            // ACT
            _instance.Execute(_job);

            // ASSERT
            _exportServiceObserver.Received(1).OnJobComplete(_job);
        }

        [Test]
        public void Execute_GoldFlow_DefaultBatchStatus()
        {
            // ACT
            _instance.Execute(_job);

            // ASSERT
            _sendingEmailNotification.Received(1).OnJobStart(_job);
            _sendingEmailNotification.Received(1).OnJobComplete(_job);

            _updateJobHistoryStatus.Received(1).OnJobStart(_job);
            _updateJobHistoryStatus.Received(1).OnJobComplete(_job);

        }

        [Test]
        public void Execute_DefaultBatchStatus_ErrorOnStart()
        {
            // ARRANGE
            _sendingEmailNotification.When(notifer => notifer.OnJobStart(_job)).Throw<Exception>();

            // ACT
            _instance.Execute(_job);

            // ASSERT
            _exporterFactory.DidNotReceive().BuildExporter(
                Arg.Any<IJobStopManager>(),
                Arg.Any<FieldMap[]>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                _IMPORTSETTINGS_WITH_USERID,
                Arg.Any<IDocumentRepository>(),
                Arg.Any<IExportDataSanitizer>());

            _sendingEmailNotification.Received(1).OnJobComplete(_job);
            _updateJobHistoryStatus.Received(1).OnJobStart(_job);
            _updateJobHistoryStatus.Received(1).OnJobComplete(_job);
        }

        [Test]
        public void Execute_DefaultBatchStatus_ErrorOnComplete()
        {
            // ARRANGE
            Exception exception = new Exception();
            _sendingEmailNotification.When(notifer => notifer.OnJobComplete(_job)).Do(info => { throw exception; });

            // ACT
            _instance.Execute(_job);

            // ASSERT
            _updateJobHistoryStatus.Received(1).OnJobComplete(_job);
            _jobHistoryErrorService.Received(1).AddError(Arg.Is<ChoiceRef>(type => type.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)), exception);
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void Execute_EnsureToMarkErrorStatusAsExpiredIfTheJobIsStopped()
        {
            // ARRAGE
            _jobStopManager.IsStopRequested().Returns(true);

            // ACT
            _instance.Execute(_job);

            // ASSERT
            EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();
            _historyManager.Received(1).SetErrorStatusesToExpired(_caseContext.WorkspaceID, _jobHistory.ArtifactId);
        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void Execute_FailMarkErrorStatusAsExpiredIfTheJobIsStopped_ExpectNoException()
        {
            // ARRAGE
            _jobStopManager.IsStopRequested().Returns(true);
            _historyManager.When(manager => manager.SetErrorStatusesToExpired(_caseContext.WorkspaceID, _jobHistory.ArtifactId))
                .Throw<Exception>();

            // ACT &  ASSERT
            _instance.Execute(_job);
            EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();

        }

        [Test]
        [Category(TestConstants.TestCategories.STOP_JOB)]
        public void Execute_MakeSureToUpdateJobStopStateToNoneOnScheduledJob()
        {
            // ARRANGE
            _job.ScheduleRule = "rules!";

            // ACT
            _instance.Execute(_job);

            // ASSERT
            EnsureToPassTheJobStopManagerToTheJobHistroyErrorService();
            _jobService.Received(1).UpdateStopState(Arg.Is<List<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None);
        }

        [Test]
        public void Execute_GoldFlow_CreateDataReaderAndPassItToSynchronizer()
        {
            // ARRANGE
            IDataTransferContext reader = Substitute.For<IDataTransferContext>();

            _exporterService.TotalRecordsFound.Returns(99);
            _exporterService.GetDataTransferContext(Arg.Any<IExporterTransferConfiguration>()).Returns(reader);

            // ACT
            _instance.Execute(_job);

            // ASSERT
            _synchronizer.Received(1).SyncData(Arg.Any<IDataTransferContext>(), Arg.Any<List<FieldMap>>(), Arg.Any<string>(), Arg.Any<IJobStopManager>(), Arg.Any<IDiagnosticLog>());
        }

        [Test]
        public void Execute_JobHasNoBatchId_ExpectNewBatchIdToBeGenerated()
        {
            // ARRANGE
            const string newConfig = "new config";
            Job job = new JobBuilder()
                .WithWorkspaceId(_configuration.SourceWorkspaceArtifactId)
                .WithRelatedObjectArtifactId(_integrationPointDto.ArtifactId)
                .WithJobDetails(string.Empty)
                .Build();
            SetUp(job);
            _serializer.Serialize(Arg.Any<TaskParameters>()).Returns(newConfig);
            _jobHistoryService.GetRdoWithoutDocuments(Arg.Any<Guid>()).Returns(_jobHistory);

            // ACT
            _instance.Execute(job);

            // ASSERT
            Assert.AreEqual(newConfig, _job.JobDetails);
        }

        [Test]
        public void Execute_EnsureToValidateJob()
        {
            // ARRANGE

            // ACT
            _instance.Execute(_job);

            // ASSERT
            _agentValidator.Received(1).Validate(_integrationPointDto, _job.SubmittedBy);

            _jobHistoryService.Received(1).UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(x => x == _jobHistory));
        }

        [Test]
        public void Execute_EnsureToHandleValidationErrorJob()
        {
            // ARRANGE
            _agentValidator.When(x => x.Validate(_integrationPointDto, _job.SubmittedBy)).Do(x =>
                {
                    throw new PermissionException();
                }
            );

            // ACT
            Action action = () => _instance.Execute(_job);

            // ASSERT
            action.ShouldThrow<PermissionException>();

            _agentValidator.Received(1).Validate(_integrationPointDto, _job.SubmittedBy);

            _jobHistoryService.Received(2).UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(x => x == _jobHistory));
        }

        [Test]
        public void Execute_ShouldThrowValidationException_WhenValidationFails()
        {
            // ARRANGE
            _agentValidator.When(x => x.Validate(_integrationPointDto, _job.SubmittedBy)).Do(x =>
                {
                    throw new IntegrationPointValidationException(new ValidationResult());
                }
            );

            // ACT
            Action action = () =>_instance.Execute(_job);

            // ASSERT
            action.ShouldThrow<IntegrationPointValidationException>();

            _agentValidator.Received(1).Validate(_integrationPointDto, _job.SubmittedBy);
            _jobHistoryService.Received(2).UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(x => x == _jobHistory));
        }

        private void AssertFinalizedJob(Job job)
        {
            // dispose jobStopManager
            _jobStopManager.Received().Dispose();

            // update stop state to unstoppable
            _jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(job.JobId)), StopState.Unstoppable);

            // commit error
            _jobHistoryErrorService.Received().CommitErrors();
        }

        private void AssertJobHistoryErrorServiceReceivedException<T>(string message) where T : Exception
        {
            _jobHistoryErrorService
                .Received(1)
                .AddError(
                    Arg.Is<ChoiceRef>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)),
                    Arg.Is<T>(ex => ex.Message == message));
        }

        private void AssertJobHistoryErrorServiceReceivedAggregateException(string innerMessage)
        {
            _jobHistoryErrorService
                .Received(1)
                .AddError(
                    Arg.Is<ChoiceRef>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)),
                    Arg.Is<AggregateException>(ex => ex.InnerExceptions[0].Message == innerMessage));
        }

        private void EnsureToUpdateTotalItemCount()
        {
            _jobHistoryService.Received().UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(rdo => rdo.TotalItems == _EXPORT_DOC_COUNT));
        }

        private void AssertRetrySavedSearch(bool expectToCreate)
        {
            if (expectToCreate)
            {
                _exporterFactory.Received(1).BuildExporter(
                    _jobStopManager,
                    Arg.Any<FieldMap[]>(),
                    _integrationPointDto.SourceConfiguration,
                    _RETRY_SAVEDSEARCHID,
                    _IMPORTSETTINGS_WITH_USERID,
                    _documentRepository,
                    _exportDataSanitizer);
                _jobHistoryErrorManager.Received(1).CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId);
                _jobHistoryErrorRepository.Received(1).DeleteItemLevelErrorsSavedSearch(_RETRY_SAVEDSEARCHID);
            }
            else
            {
                _exporterFactory.Received(1).BuildExporter(
                    _jobStopManager,
                    Arg.Any<FieldMap[]>(),
                    _integrationPointDto.SourceConfiguration,
                    _configuration.SavedSearchArtifactId,
                    _IMPORTSETTINGS_WITH_USERID,
                    _documentRepository,
                    _exportDataSanitizer);
                _jobHistoryErrorManager.DidNotReceive().CreateItemLevelErrorsSavedSearch(_job, _configuration.SavedSearchArtifactId);
                _jobHistoryErrorRepository.DidNotReceive().DeleteItemLevelErrorsSavedSearch(_RETRY_SAVEDSEARCHID);
            }
        }

        private void EnsureToPassTheJobStopManagerToTheJobHistroyErrorService()
        {
            _jobHistoryErrorService.Received(1).JobStopManager = _jobStopManager;
        }

        private void ExporterServiceObjectIsFinalized()
        {
            _exportServiceObserver.OnJobComplete(_job);
        }

        private string GetUniqueJobId(Job job, Guid identifier)
        {
            return job.JobId + "_" + identifier;
        }
    }
}
