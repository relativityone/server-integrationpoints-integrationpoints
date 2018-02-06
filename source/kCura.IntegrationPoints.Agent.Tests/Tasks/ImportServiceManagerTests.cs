using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Choice = kCura.Relativity.Client.DTOs.Choice;

using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	[Description("These tests were modeled after unit tests in ExportServiceManagerTests")]
	public class ImportServiceManagerTests : TestBase
	{
		private const string _IMPORTSETTINGS_WITH_USERID = "blah blah blah";
		private const string _IMPORTSETTINGS_FOR_IMAGE = "ImageImport";
		private const string _IMPORTSETTINGS_FOR_DOC = "DocumentImport";
		private const string _IMPORT_PROVIDER_SETTINGS_FOR_IMAGE = "ImageImport";
		private const string _IMPORT_PROVIDER_SETTINGS_FOR_DOC = "DocumentImport";
		private const string _ERROR_FILE_PATH = "ErrorFilePath";
		private const string _LOAD_FILE_PATH = "LoadFilePath";
		private const int _RECORD_COUNT = 42;

		private ImportServiceManager _instance;
		private IHelper _helper;
		private ICaseServiceContext _caseContext;
		private IContextContainerFactory _contextContainerFactory;
		private ISynchronizerFactory _synchronizerFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _claimPrincipleFactory;
		private ISourceWorkspaceManager _sourceWorkspaceManager;
		private ISourceJobManager _sourceJobManager;
		private IManagerFactory _managerFactory;
		private IEnumerable<IBatchStatus> _batchStatuses;
		private ISerializer _serializer;
		private IJobService _jobService;
		private IScheduleRuleFactory _scheduleRuleFactory;
		private IJobHistoryService _jobHistoryService;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IJobHistoryManager _jobHistoryManager;
		private IContextContainer _contextContainer;
		private IJobStopManager _jobStopManager;
		private IDataSynchronizer _synchronizer;
		private IDataReaderFactory _dataReaderFactory;
		private IImportFileLocationService _importFileLocationService;
		private IDataReader _opticonFileReader;
		private IDataReader _loadFileReader;

		private Job _job;
		private Data.IntegrationPoint _integrationPoint;
		private SourceConfiguration _configuration;
		private TaskParameters _taskParameters;
		private JobHistory _jobHistory;
		private SourceProvider _sourceProvider;
		private List<FieldMap> _mappings;
		private JobHistoryErrorDTO.UpdateStatusType _updateStatusType;
		private JobStatisticsService _jobStatisticsService;

		private object _lock;
		//not sure
		private IBatchStatus _updateJobHistoryStatus;
		private IBatchStatus _sendingEmailNotification;

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
			_caseContext = Substitute.For<ICaseServiceContext>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_synchronizerFactory = Substitute.For<ISynchronizerFactory>();
			_claimPrincipleFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();
			_sourceJobManager = Substitute.For<ISourceJobManager>();
			_managerFactory = Substitute.For<IManagerFactory>();

			_sendingEmailNotification = Substitute.For<IBatchStatus>();
			_updateJobHistoryStatus = Substitute.For<IBatchStatus>();
			_batchStatuses = new List<IBatchStatus>() {_sendingEmailNotification, _updateJobHistoryStatus};
			_serializer = Substitute.For<ISerializer>();
			_jobService = Substitute.For<IJobService>();
			
			_scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();

			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_contextContainer = Substitute.For<IContextContainer>();
			_jobStopManager = Substitute.For<IJobStopManager>();
			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);
			_synchronizer = Substitute.For<IDataSynchronizer>();
			_dataReaderFactory = Substitute.For<IDataReaderFactory>();
			_importFileLocationService = Substitute.For<IImportFileLocationService>();
			_loadFileReader = Substitute.For<IDataReader, IArtifactReader>();
			_opticonFileReader = Substitute.For<IDataReader, IOpticonDataReader>();
			((IArtifactReader)_loadFileReader).CountRecords().Returns(_RECORD_COUNT);
			((IOpticonDataReader)_opticonFileReader).CountRecords().Returns(_RECORD_COUNT);

			_jobStatisticsService = Substitute.For<JobStatisticsService>();

			_dataReaderFactory.GetDataReader(Arg.Any<FieldMap[]>(), _IMPORT_PROVIDER_SETTINGS_FOR_DOC).Returns(_loadFileReader);
			_dataReaderFactory.GetDataReader(Arg.Any<FieldMap[]>(), _IMPORT_PROVIDER_SETTINGS_FOR_IMAGE).Returns(_opticonFileReader);

			_importFileLocationService.LoadFileFullPath(Arg.Any<int>()).Returns(_LOAD_FILE_PATH);
			_importFileLocationService.ErrorFilePath(Arg.Any<int>()).Returns(_ERROR_FILE_PATH);

			_lock = new object();
			_integrationPoint = new Data.IntegrationPoint()
			{
				SourceConfiguration = "source config",
				DestinationConfiguration = "destination config",
				SourceProvider = 741,
				FieldMappings = "mapping",
				SecuredConfiguration = "secured config"
			};
			_configuration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = 8465,
				SavedSearchArtifactId = 987654
			};

			_taskParameters = new TaskParameters() {BatchInstance = Guid.NewGuid() };
			_jobHistory = new JobHistory() { JobType = JobTypeChoices.JobHistoryRun, TotalItems = 0};
			_sourceProvider = new SourceProvider();
			_mappings = new List<FieldMap>();
			_updateStatusType = new JobHistoryErrorDTO.UpdateStatusType();

			_caseContext.RsapiService.RelativityObjectManager.Read<Data.IntegrationPoint>(job.RelatedObjectArtifactID).Returns(_integrationPoint);
			_serializer.Deserialize<SourceConfiguration>(_integrationPoint.SourceConfiguration).Returns(_configuration);
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(_taskParameters);
			_jobHistoryService.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _taskParameters.BatchInstance, Arg.Any<DateTime>()).Returns(_jobHistory);
			_caseContext.RsapiService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider.Value).Returns(_sourceProvider);
			_serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(_mappings);
			_synchronizerFactory.CreateSynchronizer(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(_synchronizer);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _taskParameters.BatchInstance, job.JobId, true).Returns(_jobStopManager);

			ImportSettings imageSettings = new ImportSettings();
			imageSettings.ImageImport = true;
			ImportSettings documentSettings = new ImportSettings();
			imageSettings.ImageImport = false;
			_serializer.Deserialize<ImportSettings>(_IMPORTSETTINGS_FOR_DOC).Returns(documentSettings);
			_serializer.Deserialize<ImportSettings>(_IMPORTSETTINGS_FOR_IMAGE).Returns(imageSettings);
			_serializer.Serialize(documentSettings).Returns(_IMPORTSETTINGS_FOR_DOC);
			_serializer.Serialize(imageSettings).Returns(_IMPORTSETTINGS_FOR_IMAGE);
			ImportProviderSettings providerSettingsForDoc = new ImportProviderSettings();
			ImportProviderSettings providerSettingsForImage = new ImportProviderSettings();
			providerSettingsForDoc.LineNumber = "0";
			providerSettingsForImage.LineNumber = "0";
			_serializer.Deserialize<ImportProviderSettings>(_IMPORT_PROVIDER_SETTINGS_FOR_DOC).Returns(providerSettingsForDoc);
			_serializer.Serialize(providerSettingsForDoc).Returns(_IMPORT_PROVIDER_SETTINGS_FOR_DOC);
			_serializer.Deserialize<ImportProviderSettings>(_IMPORT_PROVIDER_SETTINGS_FOR_IMAGE).Returns(providerSettingsForImage);
			_serializer.Serialize(providerSettingsForImage).Returns(_IMPORT_PROVIDER_SETTINGS_FOR_IMAGE);

			_jobStopManager.SyncRoot.Returns(_lock);
			_serializer.Deserialize<TaskParameters>(job.JobDetails)
				.Returns(_taskParameters);
			_jobHistoryService.GetRdo(Arg.Is<Guid>( guid => guid == _taskParameters.BatchInstance)).Returns(_jobHistory);
			_instance = new ImportServiceManager(_helper,
				_caseContext, _contextContainerFactory,
				_synchronizerFactory,
				_claimPrincipleFactory,	_managerFactory, _batchStatuses, _serializer, _jobService, _scheduleRuleFactory, _jobHistoryService,
				_jobHistoryErrorService, _jobStatisticsService,
				_dataReaderFactory, _importFileLocationService);
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
	}
}
