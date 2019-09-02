﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Microsoft.VisualBasic.Devices;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NSubstitute;
using NUnit.Framework;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.Testing.Identification;
using WorkspaceService = kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers.WorkspaceService;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration
{
	[TestFixture]
	public class ImportServiceManagerTests
	{
		private Data.IntegrationPoint _ip;
		private IImportFileLocationService _importFileLocationService;
		private ImportServiceManager _instanceUnderTest;

		private int _workspaceId;
		private ISerializer _serializer;
		private string _testDataDirectory;
		private static WindsorContainer _windsorContainer;
		private const string _INPUT_FOLDER_KEY = "InputFolder";
		private const string _TEST_DATA_PATH = "TestDataForImport";


		[OneTimeSetUp]
		public void Init()
		{
			_testDataDirectory = CopyTestData();

			_workspaceId = WorkspaceService.CreateWorkspace(DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));

			//Substitutes

			IHelper helper = Substitute.For<IHelper>();
			ICaseServiceContext caseServiceContext = Substitute.For<ICaseServiceContext>();
			IContextContainerFactory contextContainerFactory = Substitute.For<IContextContainerFactory>();
			ISynchronizerFactory synchronizerFactory = Substitute.For<ISynchronizerFactory>();
			IManagerFactory managerFactory = Substitute.For<IManagerFactory>();
			IEnumerable<IBatchStatus> statuses = Substitute.For<IEnumerable<IBatchStatus>>();
			IJobService jobService = Substitute.For<IJobService>();
			IScheduleRuleFactory scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();
			IJobHistoryErrorService jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			_importFileLocationService = Substitute.For<IImportFileLocationService>();
			IAgentValidator agentValidator = Substitute.For<IAgentValidator>();
			IIntegrationPointRepository integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

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
				helper, SharedVariables.RelativityWebApiUrl, true, true);
			synchronizerFactory.CreateSynchronizer(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(synchronizer);

			//RSAPI

			IRSAPIService rsapiServiceMock = Substitute.For<IRSAPIService>();
			caseServiceContext.RsapiService.Returns(rsapiServiceMock);

			//Source library

			IRelativityObjectManager objectManager = Substitute.For<IRelativityObjectManager>();
			rsapiServiceMock.RelativityObjectManager.Returns(objectManager);


			SourceProvider p = new SourceProvider();
			p.Configuration = "{}";
			p.Config.AlwaysImportNativeFiles = true;
			p.Config.AlwaysImportNativeFileNames = true;
			p.Config.OnlyMapIdentifierToIdentifier = true;

			objectManager.Read<SourceProvider>(Arg.Any<int>()).Returns(p);

			//IpLibrary


			_ip = new Data.IntegrationPoint();
			_ip.SecuredConfiguration = "";
			_ip.SourceProvider = -1;

			integrationPointRepository.ReadWithFieldMappingAsync(Arg.Any<int>()).Returns(_ip);

			//JobStopManager

			IJobStopManager stopManager = Substitute.For<IJobStopManager>();
			object syncRoot = new object();
			stopManager.SyncRoot.Returns(syncRoot);
			managerFactory.CreateJobStopManager(Arg.Any<IJobService>(),
				Arg.Any<IJobHistoryService>(), Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<bool>()).Returns(stopManager);

			//Job History Service

			JobHistory jobHistoryDto = new JobHistory();
			jobHistoryService.GetOrCreateScheduledRunHistoryRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<DateTime>())
				.Returns(jobHistoryDto);
			jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(jobHistoryDto);

			//Logging

			IAPILog logger = Substitute.For<IAPILog>();
			ILogFactory loggerFactory = Substitute.For<ILogFactory>();
			helper.GetLoggerFactory().Returns(loggerFactory);
			loggerFactory.GetLogger().Returns(logger);
			logger.ForContext<ImportService>().Returns(logger);

			//Resolve concrete implementations

			_serializer = _windsorContainer.Resolve<ISerializer>();
			IDataReaderFactory dataReaderFactory = _windsorContainer.Resolve<IDataReaderFactory>();

			_instanceUnderTest = new ImportServiceManager(helper,
				caseServiceContext,
				contextContainerFactory,
				synchronizerFactory,
				managerFactory,
				statuses,
				_serializer,
				jobService,
				scheduleRuleFactory,
				jobHistoryService,
				jobHistoryErrorService,
				null,
				dataReaderFactory,
				_importFileLocationService,
				agentValidator,
				integrationPointRepository);
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

		[SmokeTest]
		[TestCaseSource(nameof(ImportTestCaseSource))]
		public void RunStableTestCase(IImportTestCase testCase)
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
				.ResolveAll<IImportTestCase>();
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