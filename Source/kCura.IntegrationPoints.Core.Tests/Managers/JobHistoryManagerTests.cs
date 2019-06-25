using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
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
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class JobHistoryManagerTests : TestBase
	{
		private IJobHistoryManager _sut;

		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryRepository _jobHistoryRepository;
		private IObjectTypeRepository _objectTypeRepo;
		private IArtifactGuidRepository _artifactGuidRepo;
		private IJobHistoryErrorRepository _jobHistoryErrorRepo;
		private Mock<IMassUpdateHelper> _massUpdateHelperMock;

		private const int _WORKSPACE_ID = 100532;

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_objectTypeRepo = Substitute.For<IObjectTypeRepository>();
			_artifactGuidRepo = Substitute.For<IArtifactGuidRepository>();
			_jobHistoryErrorRepo = Substitute.For<IJobHistoryErrorRepository>();
			_massUpdateHelperMock = new Mock<IMassUpdateHelper>();
			_massUpdateHelperMock.Setup(x => x.UpdateArtifactsAsync(
				It.IsAny<ICollection<int>>(),
				It.IsAny<FieldUpdateRequestDto[]>(),
				It.IsAny<IMassUpdateRepository>())
			).Returns(Task.CompletedTask);

			IAPILog logger = Substitute.For<IAPILog>();
			_sut = new JobHistoryManager(_repositoryFactory, logger, _massUpdateHelperMock.Object);

			_repositoryFactory.GetJobHistoryRepository(_WORKSPACE_ID).Returns(_jobHistoryRepository);
			_repositoryFactory.GetJobHistoryErrorRepository(_WORKSPACE_ID).Returns(_jobHistoryErrorRepo);
		}

		[Test]
		public void GetLastJobHistoryArtifactId_GoldFlow()
		{
			// ARRANGE
			int integrationPointArtifactId = 1322131;
			int expectedLastTwoJobHistoryIds = 234242;
			_jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointArtifactId).Returns(expectedLastTwoJobHistoryIds);

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
			int[] pendingJobHistoryIDs = { 234323, 980934 };
			int[] processingJobHistoryIDs = { 323, 9893 };
			IDictionary<Guid, int[]> artifactIdsByStatus = new Dictionary<Guid, int[]>()
			{
				{JobStatusChoices.JobHistoryPending.Guids.First(), pendingJobHistoryIDs},
				{JobStatusChoices.JobHistoryProcessing.Guids.First(), processingJobHistoryIDs},
			};

			_jobHistoryRepository.GetStoppableJobHistoryArtifactIdsByStatus(integrationPointArtifactId).Returns(artifactIdsByStatus);

			// ACT
			StoppableJobCollection result = _sut.GetStoppableJobCollection(_WORKSPACE_ID, integrationPointArtifactId);

			// ASSERT
			Assert.IsTrue(pendingJobHistoryIDs.SequenceEqual(result.PendingJobArtifactIds),
				"The PendingJobArtifactIds should be correct");
			Assert.IsTrue(processingJobHistoryIDs.SequenceEqual(result.ProcessingJobArtifactIds),
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
			StoppableJobCollection result = _sut.GetStoppableJobCollection(_WORKSPACE_ID, integrationPointArtifactId);

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
			int[] itemLevelErrors = { 2, 3, 4 };
			int[] jobLevelErrors = { };
			var guids = new Dictionary<Guid, int>
			{
				{ErrorStatusChoices.JobHistoryErrorExpiredGuid, errorChoiceArtifactId}
			};
			_repositoryFactory.GetObjectTypeRepository(_WORKSPACE_ID).Returns(_objectTypeRepo);
			_repositoryFactory.GetArtifactGuidRepository(_WORKSPACE_ID).Returns(_artifactGuidRepo);

			_objectTypeRepo.RetrieveObjectTypeDescriptorArtifactTypeId(ObjectTypeGuids.JobHistoryErrorGuid)
				.Returns(jobHistoryTypeId);
			_artifactGuidRepo.GetArtifactIdsForGuids(new[] { ErrorStatusChoices.JobHistoryErrorExpiredGuid })
				.Returns(guids);

			_jobHistoryErrorRepo.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item)
				.Returns(itemLevelErrors);
			_jobHistoryErrorRepo.RetrieveJobHistoryErrorArtifactIds(jobHistoryTypeId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job)
				.Returns(jobLevelErrors);

			// ACT
			_sut.SetErrorStatusesToExpired(_WORKSPACE_ID, jobHistoryTypeId);

			// ASSERT
			_massUpdateHelperMock.Verify(
				x => x.UpdateArtifactsAsync(
					itemLevelErrors,
					It.Is<FieldUpdateRequestDto[]>(fields => ValidateErrorStatusField(fields, ErrorStatusChoices.JobHistoryErrorExpiredGuid)),
					_jobHistoryErrorRepo)
				);
			_massUpdateHelperMock.Verify(
				x => x.UpdateArtifactsAsync(
					jobLevelErrors,
					It.Is<FieldUpdateRequestDto[]>(fields => ValidateErrorStatusField(fields, ErrorStatusChoices.JobHistoryErrorExpiredGuid)),
					_jobHistoryErrorRepo)
				);
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

			_massUpdateHelperMock.Setup(x => x.UpdateArtifactsAsync(
					It.IsAny<ICollection<int>>(),
					It.IsAny<FieldUpdateRequestDto[]>(),
					It.IsAny<IMassUpdateRepository>()))
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
