using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.Devices;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NSubstitute;
using NUnit.Framework;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.TestCases;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using WorkspaceService = kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers.WorkspaceService;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration
{
	[TestFixture]
	public class ImportServiceManagerTests
	{
		private const string _TEST_DATA_PATH = "TestDataForImport";
		private const string _INPUT_FOLDER_KEY = "InputFolder";

		private int _workspaceId;
		private string _testDataDirectory;

		private static WindsorContainer _windsorContainer;

		private ImportServiceManager _instanceUnderTest;

		private Data.IntegrationPoint _ip;
		private IHelper _helper;
		private ICaseServiceContext _caseServiceContext;
		private IContextContainerFactory _contextContainerFactory;
		private ISynchronizerFactory _synchronizerFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _onBehalfOfUserClaimsPrincipalFactory;
		private IManagerFactory _managerFactory;
		private IEnumerable<IBatchStatus> _statuses;
		private ISerializer _serializer;
		private IJobService _jobService;
		private IScheduleRuleFactory _scheduleRuleFactory;
		private IJobHistoryService _jobHistoryService;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IDataReaderFactory _dataReaderFactory;
		private IImportFileLocationService _importFileLocationService;
		private IAgentValidator _agentValidator;

		[OneTimeSetUp]
		public void Init()
		{
			_testDataDirectory = CopyTestData();

			_workspaceId = WorkspaceService.CreateWorkspace(DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));

			//Subsitutes

			_helper = Substitute.For<IHelper>();
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_synchronizerFactory = Substitute.For<ISynchronizerFactory>();
			_onBehalfOfUserClaimsPrincipalFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_statuses = Substitute.For<IEnumerable<IBatchStatus>>();
			_jobService = Substitute.For<IJobService>();
			_scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			_importFileLocationService = Substitute.For<IImportFileLocationService>();
			_agentValidator = Substitute.For<IAgentValidator>();

			//Data Transfer Location

			IDataTransferLocationServiceFactory lsFactory = Substitute.For<IDataTransferLocationServiceFactory>();
			IDataTransferLocationService ls = Substitute.For<IDataTransferLocationService>();
			ls.GetWorkspaceFileLocationRootPath(Arg.Any<int>()).Returns(_testDataDirectory);
			lsFactory.CreateService(Arg.Any<int>()).Returns(ls);
			_windsorContainer.Register(Component.For<IDataTransferLocationServiceFactory>().Instance(lsFactory));

			//TestRdoSynchronizer

			TestRdoSynchronizer synchronizer = new TestRdoSynchronizer(_windsorContainer.Resolve<IRelativityFieldQuery>(),
				_windsorContainer.Resolve<IImportApiFactory>(),
				_windsorContainer.Resolve<IImportJobFactory>(),
				_helper, SharedVariables.RelativityWebApiUrl, true, true);
			_synchronizerFactory.CreateSynchronizer(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(synchronizer);

			//RSAPI

			IRSAPIService rsapiServiceMock = Substitute.For<IRSAPIService>();
			_caseServiceContext.RsapiService.Returns(rsapiServiceMock);

			//Source library

			IRelativityObjectManager _objectManager = Substitute.For<IRelativityObjectManager>();
			rsapiServiceMock.RelativityObjectManager.Returns(_objectManager);


			SourceProvider p = new SourceProvider();
			p.Configuration = "{}";
			p.Config.AlwaysImportNativeFiles = true;
			p.Config.AlwaysImportNativeFileNames = true;
			p.Config.OnlyMapIdentifierToIdentifier = true;

			_objectManager.Read<SourceProvider>(Arg.Any<int>()).Returns(p);

			//IpLibrary


			_ip = new Data.IntegrationPoint();
			_ip.SecuredConfiguration = "";
			_ip.SourceProvider = -1;

			_objectManager.Read<Data.IntegrationPoint>(Arg.Any<int>()).Returns(_ip);

			//JobStopManager

			IJobStopManager stopManager = Substitute.For<IJobStopManager>();
			object syncRoot = new object();
			stopManager.SyncRoot.Returns(syncRoot);
			_managerFactory.CreateJobStopManager(Arg.Any<IJobService>(),
				Arg.Any<IJobHistoryService>(), Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<bool>()).Returns(stopManager);

			//Job History Service

			JobHistory jobHistoryDto = new JobHistory();
			_jobHistoryService.GetOrCreateScheduledRunHistoryRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<DateTime>())
				.Returns(jobHistoryDto);
			_jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(jobHistoryDto);

			//Logging

			IAPILog logger = Substitute.For<IAPILog>();
			ILogFactory loggerFactory = Substitute.For<ILogFactory>();
			_helper.GetLoggerFactory().Returns(loggerFactory);
			loggerFactory.GetLogger().Returns(logger);
			logger.ForContext<ImportService>().Returns(logger);

			//Resolve concrete implementations

			_serializer = _windsorContainer.Resolve<ISerializer>();
			_dataReaderFactory = _windsorContainer.Resolve<IDataReaderFactory>();

			_instanceUnderTest = new ImportServiceManager(_helper,
				_caseServiceContext,
				_contextContainerFactory,
				_synchronizerFactory,
				_onBehalfOfUserClaimsPrincipalFactory,
				_managerFactory,
				_statuses,
				_serializer,
				_jobService,
				_scheduleRuleFactory,
				_jobHistoryService,
				_jobHistoryErrorService,
				null,
				_dataReaderFactory,
				_importFileLocationService,
				_agentValidator);
		}

		[OneTimeTearDown]
		public void CleanUp()
		{
			WorkspaceService.DeleteWorkspace(_workspaceId);
		}

		[TearDown]
		public void TearDown()
		{
			DocumentService.DeleteAllDocuments(_workspaceId);
		}

        [Test]
		[SmokeTest]
		[TestCaseSource(nameof(ImportTestCaseSource))]
		public void RunStableTestCase(IImportTestCase testCase)
		{
			RunTestCase(testCase);
		}

        [Test]
		[SmokeTest]
		[TestInQuarantine(TestQuarantineState.FailsContinuously, 
						@"REL-225244 TODO: Broken test needs to be fixed!
						Ignore tests until verification mechanism will be fixed.
						DocumentService.GetNativeMD5String(workspaceId, docResult)
						needs to be reimplemented.")]
		[TestCaseSource(nameof(ImportFlakyTestCaseSource))]
		public void RunFlakyTestCase(IImportTestCase testCase)
		{
			RunTestCase(testCase);
		}

		private void RunTestCase(IImportTestCase testCase)
		{
			SettingsObjects settingsObjects = testCase.Prepare(_workspaceId);

			settingsObjects.ImportSettings.RelativityUsername = SharedVariables.RelativityUserName;
			settingsObjects.ImportSettings.RelativityPassword = SharedVariables.RelativityPassword;
			_ip.SourceConfiguration = _serializer.Serialize(settingsObjects.ImportProviderSettings);
			_ip.DestinationConfiguration = _serializer.Serialize(settingsObjects.ImportSettings);
			_ip.FieldMappings = _serializer.Serialize(settingsObjects.FieldMaps);

			_importFileLocationService.LoadFileFullPath(Arg.Any<int>()).Returns(Path.Combine(_testDataDirectory, settingsObjects.ImportProviderSettings.LoadFile));

			_instanceUnderTest.Execute(JobExtensions.CreateJob());

			testCase.Verify(_workspaceId);
		}

		private static IEnumerable<IImportTestCase> ImportTestCaseSource()
		{
			InitContainer();
			return _windsorContainer
				.ResolveAll<IImportTestCase>()
				.Where(x => x.GetType() != typeof(ItShouldLoadNativesFromPaths));
		}

		private static IEnumerable<IImportTestCase> ImportFlakyTestCaseSource()
		{
			InitContainer();
			return new[]
			{
				_windsorContainer.Resolve<ItShouldLoadNativesFromPaths>()
			};
		}

		private static void InitContainer()
		{
			if (_windsorContainer == null)
			{
				_windsorContainer = ContainerInstaller.CreateContainer();
			}
		}

		private static string CopyTestData()
		{
			string inputPath = ConfigurationManager.AppSettings[_INPUT_FOLDER_KEY];

			if (!Directory.Exists(inputPath))
			{
				Directory.CreateDirectory(inputPath);
			}
			else
			{
				Directory.Delete(inputPath, true);
			}

			(new Computer()).FileSystem.CopyDirectory(Path.Combine(TestContext.CurrentContext.TestDirectory, _TEST_DATA_PATH), inputPath);

			return inputPath;
		}
	}
}