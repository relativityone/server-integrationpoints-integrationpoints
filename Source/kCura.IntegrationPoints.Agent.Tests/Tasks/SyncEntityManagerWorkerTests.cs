﻿using System;
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
using kCura.IntegrationPoints.Core.Services.EntityManager;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Field = kCura.Relativity.Client.DTOs.Field;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	public class SyncEntityManagerWorkerTests : TestBase
	{
		private Data.IntegrationPoint _integrationPoint;
		private IDataSynchronizer _dataSynchronizer;
		private IHelper _helper;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IJobService _jobService;
		private IJobStopManager _jobStopManager;
		private IIntegrationPointRepository _integrationPointRepository;
		private ISerializer _jsonSerializer;
		private Job _job;
		private JobHistory _jobHistory;
		private SyncEntityManagerWorker _instance;

		private readonly string _jsonParam1 =
			"{\"BatchInstance\":\"2b7bda1b-11c9-4349-b446-ae5c8ca2c408\",\"BatchParameters\":{\"EntityManagerMap\":{\"9E6D57BEE28D8D4CA9A64765AE9510FB\":\"CN=Middle Manager,OU=Nested,OU=Testing - Users,DC=testing,DC=corp\",\"779561316F4CE44191B150453DE9A745\":\"CN=Top Manager,OU=Testing - Users,DC=testing,DC=corp\",\"2845DA5813991740BA2D6CC6C9765799\":\"CN=Bottom Manager,OU=NestedAgain,OU=Nested,OU=Testing - Users,DC=testing,DC=corp\"},\"EntityManagerFieldMap\":[{\"SourceField\":{\"DisplayName\":\"CustodianIdentifier\",\"FieldIdentifier\":\"objectguid\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"ManagerIdentidier\",\"FieldIdentifier\":\"distinguishedname\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":1}],\"ManagerFieldIdIsBinary\":false,\"ManagerFieldMap\":[{\"SourceField\":{\"DisplayName\":\"mail\",\"FieldIdentifier\":\"mail\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Email\",\"FieldIdentifier\":\"1040539\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"givenname\",\"FieldIdentifier\":\"givenname\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"First Name\",\"FieldIdentifier\":\"1040546\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":true},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"sn\",\"FieldIdentifier\":\"sn\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Last Name\",\"FieldIdentifier\":\"1040547\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":true},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"manager\",\"FieldIdentifier\":\"manager\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Manager\",\"FieldIdentifier\":\"1040548\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"objectguid\",\"FieldIdentifier\":\"objectguid\",\"FieldType\":0,\"IsIdentifier\":true,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"UniqueID\",\"FieldIdentifier\":\"1040555\",\"FieldType\":0,\"IsIdentifier\":true,\"IsRequired\":false},\"FieldMapType\":1}]}}";

		private readonly string _jsonParam2 =
			"{\"artifactTypeID\":1000051,\"ImportOverwriteMode\":\"AppendOverlay\",\"CaseArtifactId\":1019127,\"EntityManagerFieldContainsLink\":\"true\"}";

		[OneTimeSetUp]
		public override void FixtureSetUp()
		{
			base.FixtureSetUp();
			_jsonSerializer = new JSONSerializer();
		}

		[SetUp]
		public override void SetUp()
		{
			IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
			ICaseServiceContext caseServiceContext = Substitute.For<ICaseServiceContext>();
			IDataProviderFactory dataProviderFactory = Substitute.For<IDataProviderFactory>();
			_helper = Substitute.For<IHelper>();
			ISerializer serializer = Substitute.For<ISerializer>();
			ISynchronizerFactory appDomainRdoSynchronizerFactory = Substitute.For<ISynchronizerFactory>();
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			IJobManager jobManager = Substitute.For<IJobManager>();
			IManagerQueueService managerQueueService = Substitute.For<IManagerQueueService>();
			JobStatisticsService statisticsService = Substitute.For<JobStatisticsService>();
			IManagerFactory managerFactory = Substitute.For<IManagerFactory>();
			_jobService = Substitute.For<IJobService>();
			IProviderTypeService providerTypeService = Substitute.For<IProviderTypeService>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

			_jobStopManager = Substitute.For<IJobStopManager>();
			_dataSynchronizer = Substitute.For<IDataSynchronizer>();

			IFieldQueryRepository fieldQueryRepository = Substitute.For<IFieldQueryRepository>();

			IRelativityObjectManager relativityObjectManager = Substitute.For<IRelativityObjectManager>();

			int workspaceArtifactId = 12345;

			_instance = new SyncEntityManagerWorker(caseServiceContext,
				dataProviderFactory,
				_helper,
				serializer,
				appDomainRdoSynchronizerFactory,
				jobHistoryService,
				_jobHistoryErrorService,
				jobManager,
				managerQueueService,
				statisticsService,
				managerFactory,
				_jobService,
				repositoryFactory,
				relativityObjectManager,
				providerTypeService,
				_integrationPointRepository);

			_job = JobHelper.GetJob(
				1,
				null,
				null,
				1,
				1,
				workspaceArtifactId,
				222,
				TaskType.SyncEntityManagerWorker,
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
				DestinationConfiguration = "{ \"artifactTypeID\": 1000036 }",
				SecuredConfiguration = "sec conf"
			};
			SourceProvider sourceProvider = new SourceProvider
			{
				Identifier = Guid.NewGuid().ToString(),
				ApplicationIdentifier = Guid.NewGuid().ToString()
			};
			DestinationProvider destinationProvider = new DestinationProvider
			{
				Identifier = Guid.NewGuid().ToString()
			};
			_jobHistory = new JobHistory
			{
				ArtifactId = 9876546
			};
			TaskParameters taskParams = new TaskParameters
			{
				BatchInstance = Guid.NewGuid(),
				BatchParameters = new EntityManagerJobParameters
				{
					EntityManagerMap = new Dictionary<string, string>
					{
						{ "hello", "world" },
						{ "merhaba", "dunya"}
					},
					EntityManagerFieldMap = new[]
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

			List<EntityManagerMap> custodianManagerMaps = new List<EntityManagerMap>
			{
				new EntityManagerMap
				{
					EntityID = "213",
					ManagerArtifactID = 3423,
					NewManagerID = "453",
					OldManagerID = "67"
				}
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

			var associatedJobs = new List<Job> { _job };
			var fieldsMap = new List<FieldMap>();
			_integrationPointRepository.ReadWithFieldMappingAsync(_job.RelatedObjectArtifactID).Returns(_integrationPoint);
			_integrationPointRepository.GetSecuredConfiguration(_job.RelatedObjectArtifactID).Returns(_integrationPoint.SecuredConfiguration);
			caseServiceContext.RsapiService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider.Value).Returns(sourceProvider);
			caseServiceContext.RsapiService.RelativityObjectManager.Read<DestinationProvider>(_integrationPoint.DestinationProvider.Value).Returns(destinationProvider);
			serializer.Deserialize<TaskParameters>(_job.JobDetails).Returns(taskParams);
			jobHistoryService.CreateRdo(_integrationPoint, taskParams.BatchInstance,
				JobTypeChoices.JobHistoryRun, Arg.Any<DateTime>()).Returns(_jobHistory);
			managerQueueService.AreAllTasksOfTheBatchDone(_job, Arg.Any<string[]>()).Returns(true);
			managerQueueService.GetEntityManagerLinksToProcess(_job, Arg.Any<Guid>(), Arg.Any<List<EntityManagerMap>>())
				.Returns(custodianManagerMaps);
			managerFactory.CreateJobStopManager(_jobService, jobHistoryService, taskParams.BatchInstance, _job.JobId, true)
				.Returns(_jobStopManager);
			serializer.Deserialize<List<FieldMap>>(_integrationPoint.FieldMappings).Returns(fieldsMap);

			relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns(new List<RelativityObject>());
			repositoryFactory.GetFieldQueryRepository(workspaceArtifactId).Returns(fieldQueryRepository);
			fieldQueryRepository.Read(Arg.Any<Field>()).Returns(fieldResultSet);
			appDomainRdoSynchronizerFactory.CreateSynchronizer(new Guid(destinationProvider.Identifier),
				Arg.Any<string>()).Returns(_dataSynchronizer);
			jobManager.CheckBatchOnJobComplete(_job, taskParams.BatchInstance.ToString()).Returns(true);
			jobManager.GetJobsByBatchInstanceId(_integrationPoint.ArtifactId, taskParams.BatchInstance)
				.Returns(associatedJobs);
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
			Job job = GetJob(_jsonParam1);
			SyncEntityManagerWorker task =
				new SyncEntityManagerWorker(null, null, _helper, _jsonSerializer, null, null, null, null, null, null, null, null, null, null, null, null);

			//ACT
			MethodInfo dynMethod = task.GetType().GetMethod("GetParameters",
				BindingFlags.NonPublic | BindingFlags.Instance);
			dynMethod.Invoke(task, new object[] { job });

			//ASSERT
			Assert.AreEqual(new Guid("2b7bda1b-11c9-4349-b446-ae5c8ca2c408"), task.GetType().GetProperty("BatchInstance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task));

			List<EntityManagerMap> custodianManagerMap = (List<EntityManagerMap>)task.GetType().GetField("_entityManagerMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(3, custodianManagerMap.Count);
			Assert.AreEqual("779561316F4CE44191B150453DE9A745", custodianManagerMap[1].EntityID);
			Assert.AreEqual("CN=Bottom Manager,OU=NestedAgain,OU=Nested,OU=Testing - Users,DC=testing,DC=corp", custodianManagerMap[2].OldManagerID);

			List<FieldMap> custodianManagerFieldMap = (List<FieldMap>)task.GetType().GetField("_entityManagerFieldMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(1, custodianManagerFieldMap.Count);
			Assert.AreEqual(FieldMapTypeEnum.Identifier, custodianManagerFieldMap[0].FieldMapType);
			Assert.AreEqual("objectguid", custodianManagerFieldMap[0].SourceField.FieldIdentifier);
			Assert.AreEqual("distinguishedname", custodianManagerFieldMap[0].DestinationField.FieldIdentifier);

			List<FieldMap> managerFieldMap = (List<FieldMap>)task.GetType().GetField("_managerFieldMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(5, managerFieldMap.Count);

			bool managerFieldIdIsBinary = (bool)task.GetType().GetField("_managerFieldIdIsBinary", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(false, managerFieldIdIsBinary);
		}

		[Test]
		public void ReconfigureDestinationSettings_Param2_CorrectValues()
		{
			//ARRANGE
			SyncEntityManagerWorker task =
				new SyncEntityManagerWorker(null, null, _helper, _jsonSerializer, null, null, null, null, null, null, null, null, null, null, null, null);
			_integrationPoint.DestinationConfiguration = _jsonParam2;
			task.GetType().GetProperty("IntegrationPoint", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(task, _integrationPoint);

			//ACT
			MethodInfo dynMethod = task.GetType().GetMethod("ReconfigureImportAPISettings",
				BindingFlags.NonPublic | BindingFlags.Instance);
			object newDestinationConfiguration = dynMethod.Invoke(task, new object[] { 1014321 });

			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(newDestinationConfiguration.ToString());

			//ASSERT
			Assert.AreEqual(1014321, importSettings.ObjectFieldIdListContainsArtifactId[0]);
			Assert.AreEqual(ImportOverwriteModeEnum.OverlayOnly, importSettings.ImportOverwriteMode);
			Assert.AreEqual(false, importSettings.EntityManagerFieldContainsLink);
			Assert.AreEqual(1000051, importSettings.ArtifactTypeId);
			Assert.AreEqual(1019127, importSettings.CaseArtifactId);
		}

		private Job GetJob(string jobDetails)
		{
			return JobHelper.GetJob(1, null, null, 1, 1, 111, 222, TaskType.SyncEntityManagerWorker, new DateTime(), null, jobDetails,
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