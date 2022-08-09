using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
    [TestFixture, Category("Unit")]
    public class ExportManagerTest : TestBase
    {
        #region Private Fields

        private ExportManager _instanceToTest;

        private IJobManager _jobManagerMock;
        private IHelper _helperMock;
        private ISerializer _serializerMock;
        private IExportInitProcessService _exportInitProcessService;
        private IIntegrationPointService _integrationPointService;
        private IIntegrationPointRepository _integrationPointRepositoryMock;

        private readonly Job _job = JobHelper.GetJob(1, 2, 3, 4, 5, 6, 7, TaskType.ExportWorker,
                DateTime.MinValue, DateTime.MinValue, null, 1, DateTime.MinValue, 2, "", null);

        #endregion //Private Fields

        [SetUp]
        public override void SetUp()
        {
            _jobManagerMock = Substitute.For<IJobManager>();
            ICaseServiceContext caseServiceContextMock = Substitute.For<ICaseServiceContext>();
            _helperMock = Substitute.For<IHelper>();
            IManagerFactory managerFactoryMock = Substitute.For<IManagerFactory>();
            _serializerMock = Substitute.For<ISerializer>();
            _exportInitProcessService = Substitute.For<IExportInitProcessService>();
            IAgentValidator agentValidator = Substitute.For<IAgentValidator>();
            _integrationPointService = Substitute.For<IIntegrationPointService>();
            _integrationPointRepositoryMock = Substitute.For<IIntegrationPointRepository>();

            _instanceToTest = new ExportManager(
                caseServiceContextMock,
                Substitute.For<IDataProviderFactory>(),
                _jobManagerMock,
                Substitute.For<IJobService>(),
                _helperMock,
                _integrationPointService,
                _serializerMock,
                Substitute.For<IGuidService>(),
                Substitute.For<IJobHistoryService>(),
                Substitute.For<JobHistoryErrorService>(
                caseServiceContextMock,
                _helperMock,
                _integrationPointRepositoryMock),
                Substitute.For<IScheduleRuleFactory>(),
                managerFactoryMock,
                new List<IBatchStatus>(),
                _exportInitProcessService,
                agentValidator,
                Substitute.For<IDiagnosticLog>());
        }

        [TestCase(10)]
        [TestCase(0)]
        public void ItShouldReturnTotalExportDocsCount(int totalSavedSearchCount)
        {
            // Arrange
            const int artifactTypeId = 1;
            Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = "Source Configuration",
                DestinationConfiguration = $"{{ArtifactTypeId: {artifactTypeId}}}"
            };
            ExportUsingSavedSearchSettings sourceConfiguration = new ExportUsingSavedSearchSettings()
            {
                SavedSearchArtifactId = 1,
            };

            _integrationPointService.ReadIntegrationPoint(_job.RelatedObjectArtifactID).Returns(integrationPoint);
            _exportInitProcessService.CalculateDocumentCountToTransfer(sourceConfiguration, artifactTypeId).Returns(totalSavedSearchCount);

            _serializerMock.Deserialize<ExportUsingSavedSearchSettings>(integrationPoint.SourceConfiguration)
                .Returns(sourceConfiguration);

            // Act
            long retTotalCount = _instanceToTest.BatchTask(_job, null);

            // Assert
            Assert.That(retTotalCount, Is.EqualTo(totalSavedSearchCount));

            _jobManagerMock.Received(totalSavedSearchCount > 0 ? 1 : 0).CreateJobWithTracker(_job, Arg.Any<TaskParameters>(), TaskType.ExportWorker, Arg.Any<string>());
            Assert.That(_instanceToTest.BatchJobCount, Is.EqualTo(totalSavedSearchCount > 0 ? 1 : 0));
        }

        [Test]
        public void ItShouldReturnExportWorker()
        {
            _instanceToTest.CreateBatchJob(_job, new List<string>());

            _jobManagerMock.Received().CreateJobWithTracker(_job, Arg.Any<TaskParameters>(), TaskType.ExportWorker, Arg.Any<string>());
        }

        [Test]
        public void ItShouldReturnNullBatch()
        {
            var unbatchedIDs = _instanceToTest.GetUnbatchedIDs(_job);
            Assert.That(!unbatchedIDs.Any());
        }
    }
}