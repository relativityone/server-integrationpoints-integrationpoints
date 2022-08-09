using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.Services.Objects.DataContracts;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
    [TestFixture, Category("Unit")]
    public class JobHistoryServiceTests : TestBase
    {
        private IRelativityObjectManager _relativityObjectManager;
        private IWorkspaceManager _workspaceManager;
        private IFederatedInstanceManager _federatedInstanceManager;
        private IAPILog _logger;

        private JobHistoryService _instance;
        private IIntegrationPointSerializer _serializer;
        private Data.IntegrationPoint _integrationPoint;
        private ImportSettings _settings;
        private WorkspaceDTO _workspace;
        private int _jobHistoryArtifactId;
        private Guid _batchGuid;

        [SetUp]
        public override void SetUp()
        {
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();
            _workspaceManager = Substitute.For<IWorkspaceManager>();
            _federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
            _logger = Substitute.For<IAPILog>();
            _serializer = Substitute.For<IIntegrationPointSerializer>();

            _integrationPoint = new Data.IntegrationPoint()
            {
                ArtifactId = 98475,
                Name = "RIP RIP",
                DestinationConfiguration = "dest config",
                OverwriteFields = OverwriteFieldsChoices.IntegrationPointAppendOnly,
                JobHistory = new[] { 4543, 443 },
                SourceProvider = 0,
                DestinationProvider = 0
            };
            _settings = new ImportSettings()
            {
                CaseArtifactId = 987
            };
            _workspace = new WorkspaceDTO()
            {
                Name = "lol"
            };
            _batchGuid = Guid.NewGuid();
            _jobHistoryArtifactId = 987465;
            _instance = new JobHistoryService(
                _relativityObjectManager,
                _federatedInstanceManager,
                _workspaceManager,
                _logger,
                _serializer);
        }

        [Test]
        public void GetRdo_Succeeds_Test()
        {
            // Arrange
            Guid batchInstance = new Guid();
            var jobHistory = new Data.JobHistory
            {
                ArtifactId = 100
            };

            _relativityObjectManager
                .Query<Data.JobHistory>(Arg.Is<QueryRequest>(x => !string.IsNullOrEmpty(x.Condition)))
                .Returns(new List<Data.JobHistory>(1) { jobHistory });

            // Act
            Data.JobHistory actual = _instance.GetRdo(batchInstance);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.ArtifactId, jobHistory.ArtifactId);

            _relativityObjectManager
                .Received(1)
                .Query<Data.JobHistory>(Arg.Is<QueryRequest>(x =>
                    x.Condition.Contains(batchInstance.ToString())));
        }

        [Test]
        public void GetRdoWithoutDocuments_Succeeds_Test()
        {
            // Arrange
            Guid batchInstance = new Guid();
            var jobHistory = new Data.JobHistory
            {
                ArtifactId = 100,
                Name = "Job Name 1",
                JobID = "10"
            };

            _relativityObjectManager
                .Query<Data.JobHistory>(Arg.Is<QueryRequest>(x => !string.IsNullOrEmpty(x.Condition)))
                .Returns(new List<Data.JobHistory>(1) { jobHistory });

            // Act
            Data.JobHistory actual = _instance.GetRdoWithoutDocuments(batchInstance);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.ArtifactId, jobHistory.ArtifactId);
            Assert.AreEqual(actual.Name, jobHistory.Name);
            Assert.AreEqual(actual.JobID, jobHistory.JobID);

            _relativityObjectManager
                .Received(1)
                .Query<Data.JobHistory>(Arg.Is<QueryRequest>(x =>
                    x.Condition.Contains(batchInstance.ToString())));
        }

        [Test]
        public void GetJobHistory_Succeeds_Test()
        {
            // Arrange
            IList<int> jobHistoryArtifactIds = new[] { 123, 456, 789 };
            string conditionValue = string.Join(",", jobHistoryArtifactIds);

            _relativityObjectManager
                .Query<Data.JobHistory>(Arg.Is<QueryRequest>(x => x.Condition.Contains(conditionValue)))
                .Returns(new List<Data.JobHistory>(1) { new Data.JobHistory() });

            // Act
            IList<Data.JobHistory> actual = _instance.GetJobHistory(jobHistoryArtifactIds);

            // Assert
            Assert.IsNotNull(actual);

            _relativityObjectManager
                .Received(1)
                .Query<Data.JobHistory>(Arg.Is<QueryRequest>(x =>
                    !string.IsNullOrEmpty(x.Condition) &&
                        x.Condition.Contains(conditionValue)));
        }

        [Test]
        public void UpdateRdo_Succeeds_Test()
        {
            // Arrange
            Data.JobHistory jobHistory = new Data.JobHistory
            {
                ArtifactId = 456,
                BatchInstance = new Guid().ToString()
            };

            // Act
            _instance.UpdateRdo(jobHistory);

            // Assert
            _relativityObjectManager
                .Received(1)
                .Update(jobHistory);
        }

        [Test]
        public void UpdateRdoWithoutDocuments_Succeeds_Test()
        {
            // Arrange
            int artifactId = 456;
            string name = "Job Name 1";
            string jobID = "10";
            Data.JobHistory jobHistory = new Data.JobHistory
            {
                ArtifactId = artifactId,
                Name = name,
                JobID = jobID
            };

            // Act
            _instance.UpdateRdoWithoutDocuments(jobHistory);

            // Assert
            _relativityObjectManager
                .Received(1)
                .Update(
                    Arg.Is<int>(actualArtifactId => actualArtifactId == artifactId),
                    Arg.Is<List<FieldRefValuePair>>(actualFieldRefValuePairs => actualFieldRefValuePairs.Count == 2
                        && actualFieldRefValuePairs.Count(x => x.Field.Guid.ToString() == JobHistoryFieldGuids.Name
                                                               && (string)x.Value == name) == 1
                        && actualFieldRefValuePairs.Count(x => x.Field.Guid.ToString() == JobHistoryFieldGuids.JobID
                                                               && (string)x.Value == jobID) == 1));
        }

        [Test]
        public void DeleteJobHistory_Succeeds()
        {
            //Arrange
            int jobHistoryId = 12345;

            //Act
            _instance.DeleteRdo(jobHistoryId);

            //Assert
            _relativityObjectManager.Received(1).Delete(jobHistoryId);
        }

        [Test]
        public void CreateRdo_GoldFlow()
        {
            // ARRANGE
            _relativityObjectManager.Query<Data.JobHistory>(Arg.Any<QueryRequest>()).Returns(new List<Data.JobHistory>());
            _serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(_settings);
            _workspaceManager.RetrieveWorkspace(_settings.CaseArtifactId).Returns(_workspace);
            _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(_settings.FederatedInstanceArtifactId).Returns(new FederatedInstanceDto());
            _relativityObjectManager.Create(Arg.Any<Data.JobHistory>()).Returns(_jobHistoryArtifactId);

            // ACT
            Data.JobHistory jobHistory = _instance.CreateRdo(_integrationPoint, _batchGuid, JobTypeChoices.JobHistoryRun, DateTime.Now);

            // ASSERT
            ValidateJobHistory(jobHistory, JobTypeChoices.JobHistoryRun);
        }

        [Test]
        public void CreateRdo_WhenGetRdoThrowsException_NewJobHistoryCreated()
        {
            // ARRANGE
            _relativityObjectManager.Query<Data.JobHistory>(Arg.Any<QueryRequest>()).Throws(new Exception("blah blah"));
            _serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(_settings);
            _workspaceManager.RetrieveWorkspace(_settings.CaseArtifactId).Returns(_workspace);
            _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(_settings.FederatedInstanceArtifactId)
                .Returns(new FederatedInstanceDto());
            _relativityObjectManager.Create(Arg.Any<Data.JobHistory>()).Returns(_jobHistoryArtifactId);

            // ACT
            Data.JobHistory jobHistory = _instance.CreateRdo(_integrationPoint, _batchGuid, JobTypeChoices.JobHistoryRun, DateTime.Now);

            // ASSERT
            ValidateJobHistory(jobHistory, JobTypeChoices.JobHistoryRun);
        }

        [Test]
        public void GetOrCreateScheduleRunHistoryRdo_FoundRdo()
        {
            // ARRANGE
            Data.JobHistory history = new Data.JobHistory();
            List<Data.JobHistory> jobHistories = new List<Data.JobHistory>() { history };
            _relativityObjectManager.Query<Data.JobHistory>(Arg.Any<QueryRequest>()).Returns(jobHistories);

            // ACT
            Data.JobHistory returnedJobHistory = _instance.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _batchGuid, DateTime.Now);

            // ASSERT
            Assert.AreSame(history, returnedJobHistory);
        }

        [Test]
        public void GetOrCreateScheduleRunHistoryRdo_NoExistingRdo()
        {
            // ARRANGE
            _relativityObjectManager.Query<Data.JobHistory>(Arg.Any<QueryRequest>()).Returns(new List<Data.JobHistory>());
            _serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(_settings);
            _workspaceManager.RetrieveWorkspace(_settings.CaseArtifactId).Returns(_workspace);
            _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(_settings.FederatedInstanceArtifactId)
                .Returns(new FederatedInstanceDto());
            _relativityObjectManager.Create(Arg.Any<Data.JobHistory>()).Returns(_jobHistoryArtifactId);

            // ACT
            Data.JobHistory returnedJobHistory = _instance.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _batchGuid, DateTime.Now);

            // ASSERT
            ValidateJobHistory(returnedJobHistory, JobTypeChoices.JobHistoryScheduledRun);
        }

        [Test]
        public void GetOrCreateScheduleRunHistoryRdo_ErrorOnGetRdo()
        {
            // ARRANGE
            _relativityObjectManager.Query<Data.JobHistory>(Arg.Any<QueryRequest>()).Throws(new Exception("blah blah"));
            _serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(_settings);
            _workspaceManager.RetrieveWorkspace(_settings.CaseArtifactId).Returns(_workspace);
            _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(_settings.FederatedInstanceArtifactId)
                .Returns(new FederatedInstanceDto());
            _relativityObjectManager.Create(Arg.Any<Data.JobHistory>()).Returns(_jobHistoryArtifactId);

            // ACT
            Data.JobHistory returnedJobHistory = _instance.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _batchGuid, DateTime.Now);

            // ASSERT
            ValidateJobHistory(returnedJobHistory, JobTypeChoices.JobHistoryScheduledRun);
        }

        private void ValidateJobHistory(Data.JobHistory jobHistory, ChoiceRef jobType)
        {
            Assert.IsNotNull(jobHistory);
            Assert.AreEqual(_jobHistoryArtifactId, jobHistory.ArtifactId);
            Assert.IsTrue(jobHistory.IntegrationPoint.SequenceEqual(new[] { _integrationPoint.ArtifactId }));
            Assert.AreEqual(_batchGuid.ToString(), jobHistory.BatchInstance);
            Assert.IsTrue(jobHistory.JobType.EqualsToChoice(jobType));
            Assert.IsTrue(jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending));
            Assert.AreEqual(0, jobHistory.ItemsTransferred);
            Assert.AreEqual(0, jobHistory.ItemsWithErrors);
        }
    }
}