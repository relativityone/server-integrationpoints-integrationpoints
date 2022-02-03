using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture, Category("Unit")]
	public class JobHistoryManagerTests : TestBase
	{
		private IJobHistoryManager _sut;

		private Mock<IRepositoryFactory> _repositoryFactoryMock;
		private Mock<IJobHistoryRepository> _jobHistoryRepositoryMock;
		private Mock<IObjectTypeRepository> _objectTypeRepoMock;
		private Mock<IArtifactGuidRepository> _artifactGuidRepoMock;
		private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepoMock;
		private Mock<IMassUpdateHelper> _massUpdateHelperMock;

		private const int _WORKSPACE_ID = 100532;

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactoryMock = new Mock<IRepositoryFactory>();
			_jobHistoryRepositoryMock = new Mock<IJobHistoryRepository>();
			_objectTypeRepoMock = new Mock<IObjectTypeRepository>();
			_artifactGuidRepoMock = new Mock<IArtifactGuidRepository>();
			_jobHistoryErrorRepoMock = new Mock<IJobHistoryErrorRepository>();
			_massUpdateHelperMock = new Mock<IMassUpdateHelper>();
			_massUpdateHelperMock.Setup(x => x.UpdateArtifactsAsync(
				It.IsAny<ICollection<int>>(),
				It.IsAny<FieldUpdateRequestDto[]>(),
				It.IsAny<IRepositoryWithMassUpdate>())
			).Returns(Task.CompletedTask);

			var loggerMock = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};
			_sut = new JobHistoryManager(_repositoryFactoryMock.Object, loggerMock.Object, _massUpdateHelperMock.Object);

			_repositoryFactoryMock
				.Setup(x => x.GetJobHistoryRepository(_WORKSPACE_ID))
				.Returns(_jobHistoryRepositoryMock.Object);
			_repositoryFactoryMock
				.Setup(x => x.GetJobHistoryErrorRepository(_WORKSPACE_ID))
				.Returns(_jobHistoryErrorRepoMock.Object);
		}

		[Test]
		public void GetLastJobHistoryArtifactId_GoldFlow()
		{
			// ARRANGE
			int integrationPointArtifactId = 1322131;
			int expectedLastTwoJobHistoryIds = 234242;
			_jobHistoryRepositoryMock
				.Setup(x => x.GetLastJobHistoryArtifactId(integrationPointArtifactId))
				.Returns(expectedLastTwoJobHistoryIds);

			// ACT
			int result = _sut.GetLastJobHistoryArtifactId(_WORKSPACE_ID, integrationPointArtifactId);

			// ASSERT
			Assert.AreEqual(expectedLastTwoJobHistoryIds, result);
		}

		[Test]
		public void GetStoppableJobCollection_GoldFlow()
		{
			// ARRANGE
			int integrationPointArtifactId = 1322131;
			JobHistory[] pendingJobHistoryIDs = 
			{ 
				new JobHistory { ArtifactId = 234323 }, 
				new JobHistory { ArtifactId = 980934 } 
			};
			JobHistory[] processingJobHistoryIDs = 
			{
				new JobHistory { ArtifactId = 323 },
				new JobHistory { ArtifactId = 9893 }
			};

			IDictionary<Guid, int[]> artifactIdsByStatus = new Dictionary<Guid, int[]>()
			{
				{JobStatusChoices.JobHistoryPending.Guids.First(), pendingJobHistoryIDs.Select(x => x.ArtifactId).ToArray()},
				{JobStatusChoices.JobHistoryProcessing.Guids.First(), processingJobHistoryIDs.Select(x => x.ArtifactId).ToArray()},
			};

			_jobHistoryRepositoryMock
				.Setup(x => x.GetStoppableJobHistoryArtifactIdsByStatus(integrationPointArtifactId))
				.Returns(artifactIdsByStatus);

			// ACT
			StoppableJobHistoryCollection result = _sut.GetStoppableJobHistory(_WORKSPACE_ID, integrationPointArtifactId);

			// ASSERT
			Assert.IsTrue(pendingJobHistoryIDs.SequenceEqual(result.PendingJobHistory),
				"The PendingJobArtifactIds should be correct");
			Assert.IsTrue(processingJobHistoryIDs.SequenceEqual(result.ProcessingJobHistory),
				"The ProcessingJobArtifactIds should be correct");
		}

		[Test]
		public void GetStoppableJobCollection_NoResults_ReturnsEmptyArrays()
		{
			// ARRANGE
			int integrationPointArtifactId = 1322131;
			IDictionary<Guid, int[]> artifactIdsByStatus = new Dictionary<Guid, int[]>();

			_jobHistoryRepositoryMock
				.Setup(x => x.GetStoppableJobHistoryArtifactIdsByStatus(integrationPointArtifactId))
				.Returns(artifactIdsByStatus);

			// ACT
			StoppableJobHistoryCollection result = _sut.GetStoppableJobHistory(_WORKSPACE_ID, integrationPointArtifactId);

			// ASSERT
			Assert.IsNotNull(result.PendingJobHistory, $"The {nameof(StoppableJobHistoryCollection.PendingJobHistory)} should not be null.");
			Assert.IsNotNull(result.ProcessingJobHistory, $"The {nameof(StoppableJobHistoryCollection.ProcessingJobHistory)} should not be null.");
			Assert.IsTrue(result.PendingJobHistory.Length == 0, "There should be no results.");
			Assert.IsTrue(result.ProcessingJobHistory.Length == 0, "There should be no results.");
		}

		[Test]
		[Category(TestConstants.TestCategories.STOP_JOB)]
		public void SetErrorStatusesToExpired_GoldFlow()
		{
			// ARRANGE
			const int jobHistoryTypeId = 78;
			const int errorChoiceArtifactId = 98756;
			int[] itemLevelErrors = { 2, 3, 4 };
			int[] jobLevelErrors = { };
			var guids = new Dictionary<Guid, int>
			{
				{ErrorStatusChoices.JobHistoryErrorExpiredGuid, errorChoiceArtifactId}
			};
			_repositoryFactoryMock
				.Setup(x => x.GetObjectTypeRepository(_WORKSPACE_ID))
				.Returns(_objectTypeRepoMock.Object);
			_repositoryFactoryMock
				.Setup(x => x.GetArtifactGuidRepository(_WORKSPACE_ID))
				.Returns(_artifactGuidRepoMock.Object);

			_objectTypeRepoMock
				.Setup(x => x.RetrieveObjectTypeDescriptorArtifactTypeId(ObjectTypeGuids.JobHistoryErrorGuid))
				.Returns(jobHistoryTypeId);
			_artifactGuidRepoMock
				.Setup(x => x.GetArtifactIdsForGuids(new[] { ErrorStatusChoices.JobHistoryErrorExpiredGuid }))
				.Returns(guids);

			_jobHistoryErrorRepoMock
				.Setup(x => x.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item))
				.Returns(itemLevelErrors);
			_jobHistoryErrorRepoMock
				.Setup(x => x.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job))
				.Returns(jobLevelErrors);

			// ACT
			_sut.SetErrorStatusesToExpired(_WORKSPACE_ID, jobHistoryTypeId);

			// ASSERT
			_massUpdateHelperMock.Verify(
				x => x.UpdateArtifactsAsync(
					itemLevelErrors,
					It.Is<FieldUpdateRequestDto[]>(fields => ValidateErrorStatusField(fields, ErrorStatusChoices.JobHistoryErrorExpiredGuid)),
					_jobHistoryErrorRepoMock.Object)
				);
			_massUpdateHelperMock.Verify(
				x => x.UpdateArtifactsAsync(
					jobLevelErrors,
					It.Is<FieldUpdateRequestDto[]>(fields => ValidateErrorStatusField(fields, ErrorStatusChoices.JobHistoryErrorExpiredGuid)),
					_jobHistoryErrorRepoMock.Object)
				);
		}

		[Test]
		[Category(TestConstants.TestCategories.STOP_JOB)]
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
			_repositoryFactoryMock
				.Setup(x => x.GetObjectTypeRepository(_WORKSPACE_ID))
				.Returns(_objectTypeRepoMock.Object);
			_repositoryFactoryMock
				.Setup(x => x.GetArtifactGuidRepository(_WORKSPACE_ID))
				.Returns(_artifactGuidRepoMock.Object);

			_objectTypeRepoMock
				.Setup(x => x.RetrieveObjectTypeDescriptorArtifactTypeId(ObjectTypeGuids.JobHistoryErrorGuid))
				.Returns(jobHistoryTypeId);
			_artifactGuidRepoMock
				.Setup(x => x.GetArtifactIdsForGuids(ErrorStatusChoices.JobHistoryErrorExpired.Guids))
				.Returns(guids);

			_jobHistoryErrorRepoMock
				.Setup(x => x.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item))
				.Returns(itemLevelErrors);
			_jobHistoryErrorRepoMock
				.Setup(x => x.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job))
				.Returns(new int[] { });

			_massUpdateHelperMock.Setup(x => x.UpdateArtifactsAsync(
					It.IsAny<ICollection<int>>(),
					It.IsAny<FieldUpdateRequestDto[]>(),
					It.IsAny<IRepositoryWithMassUpdate>()))
				.Throws<Exception>();

			// ACT
			Assert.DoesNotThrow(() => _sut.SetErrorStatusesToExpired(_WORKSPACE_ID, jobHistoryTypeId));
		}

		private bool ValidateErrorStatusField(FieldUpdateRequestDto[] fields, Guid expectedStatus)
		{
			FieldUpdateRequestDto statusField = fields.SingleOrDefault(x => x.FieldIdentifier == JobHistoryErrorFieldGuids.ErrorStatusGuid);
			return statusField?.NewValue is SingleChoiceReferenceDto errorStatusValue && errorStatusValue.ChoiceValueGuid == expectedStatus;
		}
	}
}
