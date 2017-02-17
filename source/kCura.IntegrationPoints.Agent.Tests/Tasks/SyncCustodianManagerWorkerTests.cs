using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.CustodianManager;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.CustodianManager;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Field = kCura.Relativity.Client.DTOs.Field;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	public class SyncCustodianManagerWorkerTests : TestBase
	{
		private IRepositoryFactory _repositoryFactory;
		private ICaseServiceContext _caseServiceContext;
		private IHelper _helper;
		private IDataProviderFactory _dataProviderFactory;
		private ISerializer _serializer;
		private ISynchronizerFactory _appDomainRdoSynchronizerFactory;
		private IJobHistoryService _jobHistoryService;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IManagerFactory _managerFactory;
		private IManagerQueueService _managerQueueService;
		private JobStatisticsService _statisticsService;
		private IRSAPIClient _workspaceRsapiClient;
		private IContextContainerFactory _contextContainerFactory;
		private IJobService _jobService;
		private IJobManager _jobManager;
		private IDataReaderWrapperFactory _dataReaderWrapperFactory;
		private IProviderTypeService _providerTypeService;

		private SyncCustodianManagerWorker _instance;

		private Job _job;
		private Data.IntegrationPoint _integrationPoint;
		private SourceProvider _sourceProvider;
		private DestinationProvider _destinationProvider;
		private JobHistory _jobHistory;
		private TaskParameters _taskParams;
		private List<FieldMap> _fieldsMap;
		private List<Job> _associatedJobs;

		private IJobStopManager _jobStopManager;
		private IDataSynchronizer _dataSynchronizer;

		private string jsonParam1 =
			"{\"BatchInstance\":\"2b7bda1b-11c9-4349-b446-ae5c8ca2c408\",\"BatchParameters\":{\"CustodianManagerMap\":{\"9E6D57BEE28D8D4CA9A64765AE9510FB\":\"CN=Middle Manager,OU=Nested,OU=Testing - Users,DC=testing,DC=corp\",\"779561316F4CE44191B150453DE9A745\":\"CN=Top Manager,OU=Testing - Users,DC=testing,DC=corp\",\"2845DA5813991740BA2D6CC6C9765799\":\"CN=Bottom Manager,OU=NestedAgain,OU=Nested,OU=Testing - Users,DC=testing,DC=corp\"},\"CustodianManagerFieldMap\":[{\"SourceField\":{\"DisplayName\":\"CustodianIdentifier\",\"FieldIdentifier\":\"objectguid\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"ManagerIdentidier\",\"FieldIdentifier\":\"distinguishedName\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":1}],\"ManagerFieldIdIsBinary\":false,\"ManagerFieldMap\":[{\"SourceField\":{\"DisplayName\":\"mail\",\"FieldIdentifier\":\"mail\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Email\",\"FieldIdentifier\":\"1040539\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"givenname\",\"FieldIdentifier\":\"givenname\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"First Name\",\"FieldIdentifier\":\"1040546\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":true},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"sn\",\"FieldIdentifier\":\"sn\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Last Name\",\"FieldIdentifier\":\"1040547\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":true},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"manager\",\"FieldIdentifier\":\"manager\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Manager\",\"FieldIdentifier\":\"1040548\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"objectguid\",\"FieldIdentifier\":\"objectguid\",\"FieldType\":0,\"IsIdentifier\":true,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"UniqueID\",\"FieldIdentifier\":\"1040555\",\"FieldType\":0,\"IsIdentifier\":true,\"IsRequired\":false},\"FieldMapType\":1}]}}";

		private string jsonParam2 =
			"{\"artifactTypeID\":1000051,\"ImportOverwriteMode\":\"AppendOverlay\",\"CaseArtifactId\":1019127,\"CustodianManagerFieldContainsLink\":\"true\"}";

		private ISerializer _jsonSerializer;
		private int _workspaceArtifactId;
		private IRdoRepository _rdoRepository;
		private IFieldRepository _fieldRepository;

		[OneTimeSetUp]
		public override void FixtureSetUp()
		{
			base.FixtureSetUp();
			_jsonSerializer = new JSONSerializer();
		}

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_dataProviderFactory = Substitute.For<IDataProviderFactory>();
			_helper = Substitute.For<IHelper>();
			_serializer = Substitute.For<ISerializer>();
			_appDomainRdoSynchronizerFactory = Substitute.For<ISynchronizerFactory>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			_jobManager = Substitute.For<IJobManager>();
			_workspaceRsapiClient = Substitute.For<IRSAPIClient>();
			_managerQueueService = Substitute.For<IManagerQueueService>();
			_statisticsService = Substitute.For<JobStatisticsService>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_jobService = Substitute.For<IJobService>();
			_dataReaderWrapperFactory = Substitute.For<IDataReaderWrapperFactory>();

			_jobStopManager = Substitute.For<IJobStopManager>();
			_dataSynchronizer = Substitute.For<IDataSynchronizer>();

			_rdoRepository = Substitute.For<IRdoRepository>();
			_fieldRepository = Substitute.For<IFieldRepository>();
			_providerTypeService = Substitute.For<IProviderTypeService>();

			_workspaceArtifactId = 12345;

			_instance = new SyncCustodianManagerWorker(_caseServiceContext,
				_dataProviderFactory,
				_helper,
				_serializer,
				_appDomainRdoSynchronizerFactory,
				_jobHistoryService,
				_jobHistoryErrorService,
				_jobManager,
				_managerQueueService,
				_statisticsService,
				_managerFactory,
				_dataReaderWrapperFactory,
				_contextContainerFactory,
				_jobService,
				_repositoryFactory,
				_providerTypeService
				);

			_job = JobHelper.GetJob(
				1,
				null,
				null,
				1,
				1,
				_workspaceArtifactId,
				222,
				TaskType.SyncCustodianManagerWorker,
				new DateTime(),
				null,
				"detail",
				0,
				new DateTime(),
				1,
				null,
				null);
			_integrationPoint = new Data.IntegrationPoint
			{
				SourceProvider = 654,
				DestinationProvider = 942,
				FieldMappings = "fields",
				SourceConfiguration = "source config",
				DestinationConfiguration = "{ \"artifactTypeID\": 1000036 }"
			};
			_sourceProvider = new SourceProvider
			{
				Identifier = Guid.NewGuid().ToString(),
				ApplicationIdentifier = Guid.NewGuid().ToString()
			};
			_destinationProvider = new DestinationProvider
			{
				Identifier = Guid.NewGuid().ToString()
			};
			_jobHistory = new JobHistory
			{
				ArtifactId = 9876546
			};
			_taskParams = new TaskParameters
			{
				BatchInstance = Guid.NewGuid(),
				BatchParameters = new CustodianManagerJobParameters
				{
					CustodianManagerMap = new Dictionary<string, string>
					{
						{ "hello", "world" },
						{ "merhaba", "dunya"}
					},
					CustodianManagerFieldMap = new[]
					{
						new FieldMap
						{
							DestinationField = new FieldEntry
							{
								DisplayName = "destination id",
								FieldIdentifier = "123456"
							},
							FieldMapType = FieldMapTypeEnum.Identifier,
							SourceField = new FieldEntry
							{
								DisplayName = "source id",
								FieldIdentifier = "789456"
							}
						}
					},
					ManagerFieldIdIsBinary = false,
					ManagerFieldMap = new[]
					{
						new FieldMap
						{
							DestinationField = new FieldEntry
							{
								DisplayName = "destination id",
								FieldIdentifier = "0"
							},
							FieldMapType = FieldMapTypeEnum.Identifier,
							SourceField = new FieldEntry
							{
								DisplayName = "source id",
								FieldIdentifier = "789456"
							}
						}
					}
				}
			};

			List<CustodianManagerMap> custodianManagerMaps = new List<CustodianManagerMap>
			{
				new CustodianManagerMap
				{
					CustodianID = "213",
					ManagerArtifactID = 3423,
					NewManagerID = "453",
					OldManagerID = "67"
				}
			};

			QueryResultSet<RDO> rdoQueryResultSet = new QueryResultSet<RDO>
			{
				Success = true
			};

			ResultSet<Field> fieldResultSet = new ResultSet<Field>
			{
				Success = true,
				Results = new List<Result<Field>> {
					new Result<Field>
					{
						Success = true,
						Artifact = new Field()
					}
				}
			};
			
			_associatedJobs = new List<Job> { _job };
			_fieldsMap = new List<FieldMap>();
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Read(_job.RelatedObjectArtifactID).Returns(_integrationPoint);
			_caseServiceContext.RsapiService.SourceProviderLibrary.Read(_integrationPoint.SourceProvider.Value).Returns(_sourceProvider);
			_caseServiceContext.RsapiService.DestinationProviderLibrary.Read(_integrationPoint.DestinationProvider.Value).Returns(_destinationProvider);
			_serializer.Deserialize<TaskParameters>(_job.JobDetails).Returns(_taskParams);
			_jobHistoryService.CreateRdo(_integrationPoint, _taskParams.BatchInstance,
				JobTypeChoices.JobHistoryRun, Arg.Any<DateTime>()).Returns(_jobHistory);
			_managerQueueService.AreAllTasksOfTheBatchDone(_job, Arg.Any<string[]>()).Returns(true);
			_managerQueueService.GetCustodianManagerLinksToProcess(_job, Arg.Any<Guid>(), Arg.Any<List<CustodianManagerMap>>())
				.Returns(custodianManagerMaps);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, _taskParams.BatchInstance, _job.JobId, true)
				.Returns(_jobStopManager);
			_serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(_fieldsMap);
			_repositoryFactory.GetRdoRepository(_workspaceArtifactId).Returns(_rdoRepository);
			_rdoRepository.Query(Arg.Any<Query<RDO>>()).Returns(rdoQueryResultSet);
			_repositoryFactory.GetFieldRepository(_workspaceArtifactId).Returns(_fieldRepository);
			_fieldRepository.Read(Arg.Any<Field>()).Returns(fieldResultSet);
			_appDomainRdoSynchronizerFactory.CreateSynchronizer(new Guid(_destinationProvider.Identifier),
				Arg.Any<string>()).Returns(_dataSynchronizer);
			_jobManager.CheckBatchOnJobComplete(_job, _taskParams.BatchInstance.ToString()).Returns(true);
			_jobManager.GetJobsByBatchInstanceId(_integrationPoint.ArtifactId, _taskParams.BatchInstance)
				.Returns(_associatedJobs);
		}

		[Test]
		public void Execute_GoldFlow()
		{
			// act
			_instance.Execute(_job);

			// assert
			EnsureToSetJobHistoryErrorServiceProperties();
			_dataSynchronizer.Received(1).SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<IEnumerable<FieldMap>>(), Arg.Any<string>());
			_jobHistoryErrorService.Received().CommitErrors();
			Assert.DoesNotThrow(_jobStopManager.Dispose);
			_jobService.Received().UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None);
		}

		[Test]
		public void Execute_StopRequested_DoesNotStop()
		{
			// arrange
			_jobStopManager
				.When(manager => manager.ThrowIfStopRequested())
				.Do(Callback.First(x => { })
					.Then(x => { }).Then(x => { }).Then(x => { })
					.Then(info => { throw new OperationCanceledException(); }));
			_jobStopManager.IsStopRequested().Returns(true);

			// act
			_instance.Execute(_job);

			// assert
			EnsureToSetJobHistoryErrorServiceProperties();
			_dataSynchronizer.Received(1).SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<IEnumerable<FieldMap>>(), Arg.Any<string>());
			Assert.DoesNotThrow(_jobStopManager.Dispose);
			_jobService.Received().UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None);
		}

		[Test]
		public void GetParameters_Param1_CorrectValues()
		{
			//ARRANGE
			Job job = GetJob(jsonParam1);
			SyncCustodianManagerWorker task =
				new SyncCustodianManagerWorker(null, null, _helper, _jsonSerializer, null, null, null, null, null, null, null, null, null, null, null, null);

			//ACT
			MethodInfo dynMethod = task.GetType().GetMethod("GetParameters",
				BindingFlags.NonPublic | BindingFlags.Instance);
			dynMethod.Invoke(task, new object[] { job });

			//ASSERT
			Assert.AreEqual(new Guid("2b7bda1b-11c9-4349-b446-ae5c8ca2c408"), task.GetType().GetProperty("BatchInstance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task));

			List<CustodianManagerMap> _custodianManagerMap = (List<CustodianManagerMap>)task.GetType().GetField("_custodianManagerMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(3, _custodianManagerMap.Count);
			Assert.AreEqual("779561316F4CE44191B150453DE9A745", _custodianManagerMap[1].CustodianID);
			Assert.AreEqual("CN=Bottom Manager,OU=NestedAgain,OU=Nested,OU=Testing - Users,DC=testing,DC=corp", _custodianManagerMap[2].OldManagerID);

			List<FieldMap> _custodianManagerFieldMap = (List<FieldMap>)task.GetType().GetField("_custodianManagerFieldMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(1, _custodianManagerFieldMap.Count);
			Assert.AreEqual(FieldMapTypeEnum.Identifier, _custodianManagerFieldMap[0].FieldMapType);
			Assert.AreEqual("objectguid", _custodianManagerFieldMap[0].SourceField.FieldIdentifier);
			Assert.AreEqual("distinguishedName", _custodianManagerFieldMap[0].DestinationField.FieldIdentifier);

			List<FieldMap> _managerFieldMap = (List<FieldMap>)task.GetType().GetField("_managerFieldMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(5, _managerFieldMap.Count);

			bool _managerFieldIdIsBinary = (bool)task.GetType().GetField("_managerFieldIdIsBinary", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(false, _managerFieldIdIsBinary);
		}

		[Test]
		public void ReconfigureDestinationSettings_Param2_CorrectValues()
		{
			//ARRANGE
			SyncCustodianManagerWorker task =
				new SyncCustodianManagerWorker(null, null, _helper, _jsonSerializer, null, null, null, null, null, null, null, null, null, null, null, null);
			_integrationPoint.DestinationConfiguration = jsonParam2;
			task.GetType().GetProperty("IntegrationPoint", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(task, _integrationPoint);
			
			//ACT
			MethodInfo dynMethod = task.GetType().GetMethod("ReconfigureImportAPISettings",
				BindingFlags.NonPublic | BindingFlags.Instance);
			object newDestinationConfiguration = dynMethod.Invoke(task, new object[] { 1014321 });

			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(newDestinationConfiguration.ToString());

			//ASSERT
			Assert.AreEqual(1014321, importSettings.ObjectFieldIdListContainsArtifactId[0]);
			Assert.AreEqual(ImportOverwriteModeEnum.OverlayOnly, importSettings.ImportOverwriteMode);
			Assert.AreEqual(false, importSettings.CustodianManagerFieldContainsLink);
			Assert.AreEqual(1000051, importSettings.ArtifactTypeId);
			Assert.AreEqual(1019127, importSettings.CaseArtifactId);
		}

		private Job GetJob(string jobDetails)
		{
			return JobHelper.GetJob(1, null, null, 1, 1, 111, 222, TaskType.SyncCustodianManagerWorker, new DateTime(), null, jobDetails,
				0, new DateTime(), 1, null, null);
		}

		private void EnsureToSetJobHistoryErrorServiceProperties()
		{
			_jobHistoryErrorService.Received(1).JobHistory = _jobHistory;
			_jobHistoryErrorService.Received(1).IntegrationPoint = _integrationPoint;
			_jobHistoryErrorService.Received(1).SubscribeToBatchReporterEvents(_dataSynchronizer);
		}
	}
}