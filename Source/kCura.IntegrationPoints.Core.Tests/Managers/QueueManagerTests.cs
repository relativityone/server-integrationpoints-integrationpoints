using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class QueueManagerTests : TestBase
    {
        private IQueueRepository _queueRepository;
        private IRepositoryFactory _repositoryFactory;
        private IQueueManager _instance;
        private int _workspaceId = 12345;
        private int _integrationPointId = 98765;
        private DateTime _runTime = new DateTime(2016, 05, 10);
        private long _jobId = 4141;

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _queueRepository = Substitute.For<IQueueRepository>();

            _repositoryFactory.GetQueueRepository().Returns(_queueRepository);

            _instance = new QueueManager(_repositoryFactory);

            //verify
            _repositoryFactory.Received().GetQueueRepository();
        }

        [Test]
        public void HasJobsExecutingOrInQueue_JobAlreadyExecuting_True()
        {
            //Arrange
            _queueRepository.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId).Returns(2);

            //Act
            bool hasJobs = _instance.HasJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

            //Assert
            Assert.IsTrue(hasJobs);
            _queueRepository.Received(1).GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);
        }

        [Test]
        public void HasJobsExecutingOrInQueue_NoJobsExecuting_False()
        {
            //Arrange
            _queueRepository.GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId).Returns(0);

            //Act
            bool hasJobs = _instance.HasJobsExecutingOrInQueue(_workspaceId, _integrationPointId);

            //Assert
            Assert.IsFalse(hasJobs);
            _queueRepository.Received(1).GetNumberOfJobsExecutingOrInQueue(_workspaceId, _integrationPointId);
        }

        [Test]
        public void HasJobsExecuting_JobAlreadyExecuting_True()
        {
            //Arrange
            _queueRepository.GetNumberOfJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime).Returns(41);

            //Act
            bool hasJobs = _instance.HasJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime);

            //Assert
            Assert.IsTrue(hasJobs);
            _queueRepository.Received(1).GetNumberOfJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime);
        }

        [Test]
        public void HasJobsExecuting_NoJobsExecuting_False()
        {
            //Arrange
            _queueRepository.GetNumberOfJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime).Returns(0);

            //Act
            bool hasJobs = _instance.HasJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime);

            //Assert
            Assert.IsFalse(hasJobs);
            _queueRepository.Received(1).GetNumberOfJobsExecuting(_workspaceId, _integrationPointId, _jobId, _runTime);
        }
    }
}
