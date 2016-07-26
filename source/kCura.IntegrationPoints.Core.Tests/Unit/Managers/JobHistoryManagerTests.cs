using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class JobHistoryManagerTests
	{
		private IJobHistoryManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryRepository _jobHistoryRepository;

		private const int _WORKSPACE_ID = 100532;

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_testInstance = new JobHistoryManager(_repositoryFactory);

			_repositoryFactory.GetJobHistoryRepository(_WORKSPACE_ID).Returns(_jobHistoryRepository);
		}

		[Test]
		public void GetLastJobHistoryArtifactId_GoldFlow()
		{
			// ARRANGE
			int integrationPointArtifactId = 1322131;
			int expectedLastTwoJobHistoryIds = 234242;
			_jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId).Returns(expectedLastTwoJobHistoryIds);

			// ACT
			int result = _testInstance.GetLastJobHistoryArtifactId(_WORKSPACE_ID, integrationPointArtifactId);

			// ASSERT
			Assert.AreEqual(expectedLastTwoJobHistoryIds, result);
		}

		[Test]
		public void GetStoppableJobCollection_GoldFlow()
		{
			// ARRANGE
			int integrationPointArtifactId = 1322131;
			int[] pendingJobHistoryIds = {234323, 980934};
			int[] processingJobHistoryIds = {323, 9893};
			IDictionary<Guid, int[]> artifactIdsByStatus = new Dictionary<Guid, int[]>()
			{
				{JobStatusChoices.JobHistoryPending.ArtifactGuids.First(), pendingJobHistoryIds},
				{JobStatusChoices.JobHistoryProcessing.ArtifactGuids.First(), processingJobHistoryIds},
			};

			_jobHistoryRepository.GetStoppableJobHistoryArtifactIdsByStatus(integrationPointArtifactId).Returns(artifactIdsByStatus);

			// ACT
			StoppableJobCollection result = _testInstance.GetStoppableJobCollection(_WORKSPACE_ID, integrationPointArtifactId);

			// ASSERT
			Assert.IsTrue(pendingJobHistoryIds.SequenceEqual(result.PendingJobArtifactIds),
				"The PendingJobArtifactIds should be correct");
			Assert.IsTrue(processingJobHistoryIds.SequenceEqual(result.ProcessingJobArtifactIds),
				"The ProcessingJobArtifactIds should be correct");
		}

		[Test]
		public void GetStoppableJobCollection_NoResults_ReturnsEmptyArrays()
		{
			// ARRANGE
			int integrationPointArtifactId = 1322131;
			IDictionary<Guid, int[]> artifactIdsByStatus = new Dictionary<Guid, int[]>()
			{
			};

			_jobHistoryRepository.GetStoppableJobHistoryArtifactIdsByStatus(integrationPointArtifactId).Returns(artifactIdsByStatus);

			// ACT
			StoppableJobCollection result = _testInstance.GetStoppableJobCollection(_WORKSPACE_ID, integrationPointArtifactId);

			// ASSERT
			Assert.IsNotNull(result.PendingJobArtifactIds, $"The {nameof(StoppableJobCollection.PendingJobArtifactIds)} should not be null.");
			Assert.IsNotNull(result.ProcessingJobArtifactIds, $"The {nameof(StoppableJobCollection.ProcessingJobArtifactIds)} should not be null.");
			Assert.IsTrue(result.PendingJobArtifactIds.Length == 0, "There should be no results.");
			Assert.IsTrue(result.ProcessingJobArtifactIds.Length == 0, "There should be no results.");
		}
	}
}
