using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class JobHistoryManagerTests : TestBase
	{
		private IJobHistoryManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryRepository _jobHistoryRepository;
		private IObjectTypeRepository _objectTypeRepo;
		private IArtifactGuidRepository _artifactGuidRepo;
		private IJobHistoryErrorRepository _jobHistoryErrorRepo;
		private IScratchTableRepository _itemLevelScratchTable;
		private IScratchTableRepository _jobLevelScratchTable;

		private const int _WORKSPACE_ID = 100532;

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_objectTypeRepo = Substitute.For<IObjectTypeRepository>();
			_artifactGuidRepo = Substitute.For<IArtifactGuidRepository>();
			_jobHistoryErrorRepo = Substitute.For<IJobHistoryErrorRepository>();

			var helper = Substitute.For<IHelper>();
			_testInstance = new JobHistoryManager(_repositoryFactory, helper);
			_itemLevelScratchTable = Substitute.For<IScratchTableRepository>();
			_jobLevelScratchTable = Substitute.For<IScratchTableRepository>();
			_jobLevelScratchTable.GetTempTableName().Returns("Job Level errors");
			_itemLevelScratchTable.GetTempTableName().Returns("Item Level errors");

			_repositoryFactory.GetJobHistoryRepository(_WORKSPACE_ID).Returns(_jobHistoryRepository);
			_repositoryFactory.GetJobHistoryErrorRepository(_WORKSPACE_ID).Returns(_jobHistoryErrorRepo);
			_repositoryFactory.GetScratchTableRepository(_WORKSPACE_ID, "StoppingRIPJob_", Arg.Any<String>()).Returns(_itemLevelScratchTable, _jobLevelScratchTable);
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
				{JobStatusChoices.JobHistoryPending.Guids.First(), pendingJobHistoryIds},
				{JobStatusChoices.JobHistoryProcessing.Guids.First(), processingJobHistoryIds},
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

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void SetErrorStatusesToExpired_GoldFlow()
		{
			// ARRANGE
			const int jobHistoryTypeId = 78;
			const int errorChoiceArtifactId = 98756;
			int[] itemLevelErrors = new[] {2, 3, 4};
			Dictionary<Guid, int> guids = new Dictionary<Guid, int>()
			{
				{ErrorStatusChoices.JobHistoryErrorExpired.Guids[0], errorChoiceArtifactId}
			};
			_repositoryFactory.GetObjectTypeRepository(_WORKSPACE_ID).Returns(_objectTypeRepo);
			_repositoryFactory.GetArtifactGuidRepository(_WORKSPACE_ID).Returns(_artifactGuidRepo);

			_objectTypeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(Arg.Is<Guid>(guid => guid.Equals(new Guid(ObjectTypeGuids.JobHistoryError)))).Returns(jobHistoryTypeId);
			_artifactGuidRepo.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorExpired.Guids).Returns(guids);

			_jobHistoryErrorRepo.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(itemLevelErrors);
			_jobHistoryErrorRepo.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(new int[] { });
			
			// ACT
			_testInstance.SetErrorStatusesToExpired(_WORKSPACE_ID, jobHistoryTypeId);

			// ASSERT
			_itemLevelScratchTable.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<ICollection<int>>(collection => collection.SequenceEqual(itemLevelErrors)));
			_jobLevelScratchTable.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<ICollection<int>>(collection => collection.Count == 0));
			_jobHistoryErrorRepo.Received(1).UpdateErrorStatuses(Arg.Any<ClaimsPrincipal>(), itemLevelErrors.Length, jobHistoryTypeId, errorChoiceArtifactId, _itemLevelScratchTable.GetTempTableName());
			_jobHistoryErrorRepo.Received(1).UpdateErrorStatuses(Arg.Any<ClaimsPrincipal>(), 0, jobHistoryTypeId, errorChoiceArtifactId, _jobLevelScratchTable.GetTempTableName());
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.STOPJOB_FEATURE)]
		public void SetErrorStatusesToExpired_UpdatesFail()
		{
			// ARRANGE
			const int jobHistoryTypeId = 78;
			const int errorChoiceArtifactId = 98756;
			int[] itemLevelErrors = new[] { 2, 3, 4 };
			Dictionary<Guid, int> guids = new Dictionary<Guid, int>()
			{
				{ErrorStatusChoices.JobHistoryErrorExpired.Guids[0], errorChoiceArtifactId}
			};
			_repositoryFactory.GetObjectTypeRepository(_WORKSPACE_ID).Returns(_objectTypeRepo);
			_repositoryFactory.GetArtifactGuidRepository(_WORKSPACE_ID).Returns(_artifactGuidRepo);

			_objectTypeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(Arg.Is<Guid>(guid => guid.Equals(new Guid(ObjectTypeGuids.JobHistoryError)))).Returns(jobHistoryTypeId);
			_artifactGuidRepo.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorExpired.Guids).Returns(guids);

			_jobHistoryErrorRepo.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(itemLevelErrors);
			_jobHistoryErrorRepo.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(new int[] { });

			_jobHistoryErrorRepo
				.When(repo => repo.UpdateErrorStatuses(Arg.Any<ClaimsPrincipal>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()))
				.Do(info => { throw new Exception("error"); });

			// ACT
			Assert.DoesNotThrow(() => _testInstance.SetErrorStatusesToExpired(_WORKSPACE_ID, jobHistoryTypeId));
		}
	}
}
