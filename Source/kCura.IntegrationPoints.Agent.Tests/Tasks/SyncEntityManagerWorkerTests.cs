using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Queries;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.EntityManager;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using ObjectTypeGuids = kCura.IntegrationPoints.Core.Contracts.Entity.ObjectTypeGuids;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
    [TestFixture, Category("Unit")]
    public class SyncEntityManagerWorkerTests : TestBase
    {
        private IntegrationPointDto _integrationPoint;
        private IDataSynchronizer _dataSynchronizer;
        private IHelper _helper;
        private IJobHistoryErrorService _jobHistoryErrorService;
        private IJobService _jobService;
        private IJobStopManager _jobStopManager;
        private IIntegrationPointService _integrationPointService;
        private ISerializer _jsonSerializer;
        private IRelativityObjectManager _relativityObjectManager;
        private Job _job;
        private JobHistory _jobHistory;
        private SyncEntityManagerWorker _instance;

        private const string _SOURCE_MANAGER_UNIQUE_ID = "source id";
        private const string _DESTINATION_MANAGER_UNIQUE_ID = "destination id";


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
            IQueueQueryManager queueQueryManager = Substitute.For<IQueueQueryManager>();
            JobStatisticsService statisticsService = Substitute.For<JobStatisticsService>();
            IManagerFactory managerFactory = Substitute.For<IManagerFactory>();
            _jobService = Substitute.For<IJobService>();
            IProviderTypeService providerTypeService = Substitute.For<IProviderTypeService>();
            _integrationPointService = Substitute.For<IIntegrationPointService>();

            _jobStopManager = Substitute.For<IJobStopManager>();
            _dataSynchronizer = Substitute.For<IDataSynchronizer>();

            IFieldQueryRepository fieldQueryRepository = Substitute.For<IFieldQueryRepository>();

            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();

            int workspaceArtifactId = 12345;

            _instance = new SyncEntityManagerWorker(caseServiceContext,
                dataProviderFactory,
                _helper,
                serializer,
                appDomainRdoSynchronizerFactory,
                jobHistoryService,
                _jobHistoryErrorService,
                jobManager,
                queueQueryManager,
                statisticsService,
                managerFactory,
                _jobService,
                repositoryFactory,
                _relativityObjectManager,
                providerTypeService,
                _integrationPointService,
                null);

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
            _integrationPoint = new IntegrationPointDto
            {
                SourceProvider = 654,
                DestinationProvider = 942,
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

            const int entityManagerFieldArtifactId = 9876;

            IDictionary<string, string> entityManagerMap = new Dictionary<string, string>
            {
                { "hello", "world" },
                { "merhaba", "dunya"}
            };

            TaskParameters taskParams = new TaskParameters
            {
                BatchInstance = Guid.NewGuid(),
                BatchParameters = new EntityManagerJobParameters
                {
                    EntityManagerMap = entityManagerMap,
                    EntityManagerFieldMap = new[]
                    {
                        new FieldMap
                        {
                            DestinationField = new FieldEntry
                            {
                                DisplayName = _DESTINATION_MANAGER_UNIQUE_ID,
                                FieldIdentifier = "123456"
                            },
                            FieldMapType = FieldMapTypeEnum.Identifier,
                            SourceField = new FieldEntry
                            {
                                DisplayName = _SOURCE_MANAGER_UNIQUE_ID,
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
                                DisplayName = _DESTINATION_MANAGER_UNIQUE_ID,
                                FieldIdentifier = entityManagerFieldArtifactId.ToString()
                            },
                            FieldMapType = FieldMapTypeEnum.Identifier,
                            SourceField = new FieldEntry
                            {
                                DisplayName = _SOURCE_MANAGER_UNIQUE_ID,
                                FieldIdentifier = "789456"
                            }
                        }
                    }
                }
            };

            _jobService.GetJob(_job.JobId).Returns(_job);

            var associatedJobs = new List<Job> { _job };
            _integrationPointService.Read(_job.RelatedObjectArtifactID).Returns(_integrationPoint);
            caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(_integrationPoint.SourceProvider).Returns(sourceProvider);
            caseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<DestinationProvider>(_integrationPoint.DestinationProvider).Returns(destinationProvider);
            serializer.Deserialize<TaskParameters>(_job.JobDetails).Returns(taskParams);
            jobHistoryService.CreateRdo(_integrationPoint, taskParams.BatchInstance,
                JobTypeChoices.JobHistoryRun, Arg.Any<DateTime>()).Returns(_jobHistory);
            queueQueryManager.CheckAllSyncWorkerBatchesAreFinished(_job.JobId).Returns(new ValueReturnQuery<bool>(true));
            managerFactory.CreateJobStopManager(_jobService, jobHistoryService, taskParams.BatchInstance, _job.JobId, true, Arg.Any<IDiagnosticLog>())
                .Returns(_jobStopManager);

            _relativityObjectManager.Query(Arg.Any<QueryRequest>()).Returns(new List<RelativityObject>());
            repositoryFactory.GetFieldQueryRepository(workspaceArtifactId).Returns(fieldQueryRepository);
            fieldQueryRepository.ReadArtifactID(Arg.Any<Guid>()).Returns(entityManagerFieldArtifactId);
            appDomainRdoSynchronizerFactory.CreateSynchronizer(new Guid(destinationProvider.Identifier),
                Arg.Any<string>()).Returns(_dataSynchronizer);
            _dataSynchronizer.TotalRowsProcessed.Returns(entityManagerMap.Count);
            jobManager.CheckBatchOnJobComplete(_job, taskParams.BatchInstance.ToString()).Returns(true);
            jobManager.GetJobsByBatchInstanceId(_integrationPoint.ArtifactId, taskParams.BatchInstance)
                .Returns(associatedJobs);
            jobManager.GetBatchesStatuses(_job, taskParams.BatchInstance.ToString())
                .Returns(new BatchStatusQueryResult {ProcessingCount = 1});
        }

        [Test]
        public void Execute_GoldFlow()
        {
            // act
            _instance.Execute(_job);

            // assert
            EnsureToSetJobHistoryErrorServiceProperties();
            _dataSynchronizer.Received(1).SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<IEnumerable<FieldMap>>(), Arg.Any<string>(), Arg.Any<IJobStopManager>(), null);
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
            _dataSynchronizer.Received(1).SyncData(Arg.Any<IEnumerable<IDictionary<FieldEntry, object>>>(), Arg.Any<IEnumerable<FieldMap>>(), Arg.Any<string>(), _jobStopManager, null);
            Assert.DoesNotThrow(_jobStopManager.Dispose);
            _jobService.Received().UpdateStopState(Arg.Is<IList<long>>(lst => lst.SequenceEqual(new[] { _job.JobId })), StopState.None);
        }

        [Test]
        public void GetParameters_Param1_CorrectValues()
        {
            //ARRANGE
            Job job = GetJob(_jsonParam1);
            SyncEntityManagerWorker task =
                new SyncEntityManagerWorker(null, null, _helper, _jsonSerializer, null, null, null, null, null, null, null, null, null, null, null, null, null);

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
                new SyncEntityManagerWorker(null, null, _helper, _jsonSerializer, null, null, null, null, null, null, null, null, null, null, null, null, null);
            _integrationPoint.DestinationConfiguration = _jsonParam2;
            task.GetType().GetProperty("IntegrationPointDto", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(task, _integrationPoint);

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

        [Test]
        public void ImportedManagerArtifactIdsShouldNotLogJobLevelErrorWhenDuplicationsFound()
        {
            // arrange
            List<RelativityObject> result = new List<RelativityObject>
            {
                new RelativityObject
                {
                    FieldValues = new List<FieldValuePair>
                    {
                        new FieldValuePair
                        {
                            Field = new Field { Name = _DESTINATION_MANAGER_UNIQUE_ID },
                            Value = "Sieben 1"
                        }
                    },
                    ArtifactID = 1
                },
                new RelativityObject
                {
                    FieldValues = new List<FieldValuePair>
                    {
                        new FieldValuePair
                        {
                            Field = new Field { Name = _DESTINATION_MANAGER_UNIQUE_ID },
                            Value = "Sieben 2"
                        }
                    },
                    ArtifactID = 2
                },
                new RelativityObject
                {
                    FieldValues = new List<FieldValuePair>
                    {
                        new FieldValuePair
                        {
                            Field = new Field { Name = _DESTINATION_MANAGER_UNIQUE_ID },
                            Value = "Sieben 1"
                        }
                    },
                    ArtifactID = 3
                },
                new RelativityObject
                {
                    FieldValues = new List<FieldValuePair>
                    {
                        new FieldValuePair
                        {
                            Field = new Field { Name = _DESTINATION_MANAGER_UNIQUE_ID },
                            Value = "Sieben 1"
                        }
                    },
                    ArtifactID = 4
                },
                new RelativityObject
                {
                    FieldValues = new List<FieldValuePair>
                    {
                        new FieldValuePair
                        {
                            Field = new Field { Name = _DESTINATION_MANAGER_UNIQUE_ID },
                            Value = "Sieben 2"
                        }
                    },
                    ArtifactID = 5
                },
                new RelativityObject
                {
                    FieldValues = new List<FieldValuePair>
                    {
                        new FieldValuePair
                        {
                            Field = new Field { Name = _DESTINATION_MANAGER_UNIQUE_ID },
                            Value = "Sieben 3"
                        }
                    },
                    ArtifactID = 6
                }
            };

            _relativityObjectManager.Query(Arg.Do<QueryRequest>(x => x.ObjectType.Guid = ObjectTypeGuids.Entity)).Returns(result);

            // act && assert
            Action action = () => _instance.Execute(_job);
            action.ShouldNotThrow();

            _jobHistoryErrorService.DidNotReceive().AddError(ErrorTypeChoices.JobHistoryErrorJob, Arg.Any<Exception>());

            _jobHistoryErrorService.Received(1).AddError(ErrorTypeChoices.JobHistoryErrorItem, result[2].ArtifactID.ToString(),
            $"Duplicated entity found for: {result[2].FieldValues.First().Value} with the following ArtifactID: {result[2].ArtifactID}",
            string.Empty);
            _jobHistoryErrorService.Received(1).AddError(ErrorTypeChoices.JobHistoryErrorItem, result[3].ArtifactID.ToString(),
            $"Duplicated entity found for: {result[3].FieldValues.First().Value} with the following ArtifactID: {result[3].ArtifactID}",
            string.Empty);
            _jobHistoryErrorService.Received(1).AddError(ErrorTypeChoices.JobHistoryErrorItem, result[4].ArtifactID.ToString(),
            $"Duplicated entity found for: {result[4].FieldValues.First().Value} with the following ArtifactID: {result[4].ArtifactID}",
            string.Empty);
        }

        private Job GetJob(string jobDetails)
        {
            return JobHelper.GetJob(1, null, null, 1, 1, 111, 222, TaskType.SyncEntityManagerWorker, new DateTime(), null, jobDetails,
                0, new DateTime(), 1, null, null);
        }

        private void EnsureToSetJobHistoryErrorServiceProperties()
        {
            _jobHistoryErrorService.Received(1).JobHistory = _jobHistory;
            _jobHistoryErrorService.Received(1).IntegrationPointDto = _integrationPoint;
            _jobHistoryErrorService.Received(1).SubscribeToBatchReporterEvents(_dataSynchronizer);
        }
    }
}
