using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    public class CustomProviderTaskTests
    {
        private Mock<IIntegrationPointService> _integrationPointService;
        private Mock<ISourceProviderService> _sourceProviderService;
        private Mock<IIdFilesBuilder> _idFilesBuilder;
        private Mock<IRelativityStorageService> _relativityStorageService;
        private Mock<IStorageAccess<string>> _storageAccess;
        private Mock<IJobService> _jobService;

        [SetUp]
        public void SetUp()
        {
            _integrationPointService = new Mock<IIntegrationPointService>();
            _sourceProviderService = new Mock<ISourceProviderService>();
            _idFilesBuilder = new Mock<IIdFilesBuilder>();

            _relativityStorageService = new Mock<IRelativityStorageService>();
            _relativityStorageService
                .Setup(x => x.GetWorkspaceDirectoryPathAsync(It.IsAny<int>()))
                .ReturnsAsync("test-directory");

            _storageAccess = new Mock<IStorageAccess<string>>();
            _relativityStorageService
                .Setup(x => x.GetStorageAccessAsync())
                .ReturnsAsync(_storageAccess.Object);

            _jobService = new Mock<IJobService>();

            IntegrationPointDto dto = new IntegrationPointDto();
            _integrationPointService.Setup(x => x.Read(It.IsAny<int>())).Returns(dto);
        }

        [Test]
        public void Execute_GoldFlow()
        {
            // Arrange
            const int numberOfBatches = 3;

            List<CustomProviderBatch> batches = Enumerable.Range(1, numberOfBatches).Select(x => new CustomProviderBatch()
            {
                BatchID = x
            }).ToList();

            _idFilesBuilder
                .Setup(x => x.BuildIdFilesAsync(It.IsAny<IDataSourceProvider>(), It.IsAny<IntegrationPointDto>(), It.IsAny<string>()))
                .ReturnsAsync(batches);

            Job job = new Job();

            CustomProviderTask sut = PrepareSut();

            // Act
            sut.Execute(job);

            // Assert
            _jobService.Verify(x => x.UpdateJobDetails(It.Is<Job>(storedJob => VerifyJob(storedJob, numberOfBatches))));
            _storageAccess
                .Verify(x => x.DeleteDirectoryAsync(It.IsAny<string>(), It.IsAny<DeleteDirectoryOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void Execute_ShouldCleanupImportDirectory_WhenExceptionIsThrown()
        {
            // Arrange
            _integrationPointService
                .Setup(x => x.Read(It.IsAny<int>()))
                .Throws<InvalidOperationException>();

            Job job = new Job();

            CustomProviderTask sut = PrepareSut();

            // Act
            sut.Execute(job);

            // Assert
            _storageAccess
                .Verify(x => x.DeleteDirectoryAsync(It.IsAny<string>(), It.IsAny<DeleteDirectoryOptions>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        private bool VerifyJob(Job job, int numberOfBatches)
        {
            CustomProviderJobDetails jobDetails = new JSONSerializer().Deserialize<CustomProviderJobDetails>(job.JobDetails);
            jobDetails.ImportJobID.Should().NotBe(Guid.Empty);
            jobDetails.Batches.Count.Should().Be(numberOfBatches);
            return true;
        }

        private CustomProviderTask PrepareSut()
        {
            return new CustomProviderTask(
                _integrationPointService.Object, _sourceProviderService.Object,
                _idFilesBuilder.Object, _relativityStorageService.Object, new JSONSerializer(),
                _jobService.Object, Mock.Of<IAPILog>());
        }
    }
}
