using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class JobHistoryErrorManagerTests
	{
		private IJobHistoryErrorManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IJobHistoryRepository _jobHistoryRepository;
		private ISavedSearchRepository _savedSearchRepository;
		private IScratchTableRepository _jobHistoryErrorJobError;
		private IScratchTableRepository _jobHistoryErrorItemErrorIncluded;
		private IScratchTableRepository _jobHistoryErrorItemErrorExcluded;

		private const int _workspaceArtifactId = 102448;
		private const int _integrationPointArtifactId = 4651358;
		private const int _submittedByArtifactId = 2448071;
		private const int _originalSavedSearchArtifactId = 7748963;
		private const string _uniqueJobId = "1324_JobIdGuid";
		private const string _jobErrorPrefix = "IntegrationPoint_Relativity_JobHistoryErrors_JobError";
		private const string _itemErrorIncludedPrefix = "IntegrationPoint_Relativity_JobHistoryErrors_ItemErrors_Included";
		private const string _itemErrorExcludedPrefix = "IntegrationPoint_Relativity_JobHistoryErrors_ItemErrors_Excluded";
		private Job _job;

		private readonly List<int> _sampleJobError = new List<int>() { 4598735 };
		private readonly List<int> _sampleItemErrors = new List<int>() { 4598733, 4598734 };

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_jobHistoryErrorRepository = Substitute.For<IJobHistoryErrorRepository>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_savedSearchRepository = Substitute.For<ISavedSearchRepository>();
			_jobHistoryErrorJobError = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorItemErrorIncluded = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorItemErrorExcluded = Substitute.For<IScratchTableRepository>();

			_repositoryFactory.GetJobHistoryErrorRepository(_workspaceArtifactId).Returns(_jobHistoryErrorRepository);
			_repositoryFactory.GetJobHistoryRepository(_workspaceArtifactId).Returns(_jobHistoryRepository);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, _jobErrorPrefix, _uniqueJobId).Returns(_jobHistoryErrorJobError);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, _itemErrorIncludedPrefix, _uniqueJobId).Returns(_jobHistoryErrorItemErrorIncluded);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, _itemErrorExcludedPrefix, _uniqueJobId).Returns(_jobHistoryErrorItemErrorExcluded);

			_testInstance = new JobHistoryErrorManager(_repositoryFactory, _workspaceArtifactId, _uniqueJobId);

			_job = JobExtensions.CreateJob(_workspaceArtifactId, _integrationPointArtifactId, _submittedByArtifactId);
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow);

			//Assert
			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow);

			//Assert
			_jobHistoryErrorJobError.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemErrorIncluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow);

			//Assert
			_jobHistoryErrorJobError.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemErrorIncluded.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow);

			//Assert
			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_jobHistoryErrorJobError.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemErrorIncluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_jobHistoryErrorJobError.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemErrorIncluded.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_jobHistoryErrorJobError.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemErrorIncluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_jobHistoryErrorItemErrorIncluded.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorJobError.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.Received(0).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void CreateItemLevelErrorsSavedSearch_GoldFlow()
		{
			//Act
			_testInstance.CreateItemLevelErrorsSavedSearch(_job, _originalSavedSearchArtifactId);

			//Assert
			_jobHistoryRepository.Received().GetLastJobHistoryArtifactId(_integrationPointArtifactId);
			_jobHistoryErrorRepository.Received().CreateItemLevelErrorsSavedSearch(_integrationPointArtifactId, _originalSavedSearchArtifactId, 0);
		}

		[Test]
		public void CreateErrorListTempTablesForItemLevelErrors_GoldFlow()
		{
			// ARRANGE
			const int savedSearchId = 2321393;
			const int artifactTypeId = 10;
			const int documentId1 = 100501;
			const int documentId2 = 100502;
			const int error1 = 424324;
			const int error2 = 234234;
			const string controlNumber1 = "REL0000000179.0001";
			const string controlNumber2 = "REL0000000179.0002";
			const string controlNumber3 = "REL0000000179.0003";
			const int lastJobHistoryId = 2322133;

			_savedSearchRepository.AllDocumentsRetrieved().Returns(false, false, true);

			_repositoryFactory.GetSavedSearchRepository(_workspaceArtifactId, savedSearchId).Returns(_savedSearchRepository);

			var artifactDtos = new ArtifactDTO[]
			{
				new ArtifactDTO(documentId1, artifactTypeId, controlNumber1, new ArtifactFieldDTO[0]),
				new ArtifactDTO(documentId2, artifactTypeId, controlNumber2, new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(artifactDtos, new ArtifactDTO[0]);

			_repositoryFactory.GetJobHistoryErrorRepository(_workspaceArtifactId).Returns(_jobHistoryErrorRepository);

			Dictionary<int, string> itemLevelErrorsAndSourceUniqueIds = new Dictionary<int, string>()
			{
				{error1, controlNumber2},
				{error2, controlNumber3}
			};

			_repositoryFactory.GetJobHistoryRepository(_workspaceArtifactId).Returns(_jobHistoryRepository);
			_jobHistoryRepository.GetLastJobHistoryArtifactId(_integrationPointArtifactId).Returns(lastJobHistoryId);

			_jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId,
				JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(itemLevelErrorsAndSourceUniqueIds);

			// Act
			_testInstance.CreateErrorListTempTablesForItemLevelErrors(_job, savedSearchId);

			// Assert
			_savedSearchRepository.Received(2).RetrieveNextDocuments();
			_savedSearchRepository.Received(3).AllDocumentsRetrieved();
			_repositoryFactory.Received(1).GetSavedSearchRepository(_workspaceArtifactId, savedSearchId);
			_repositoryFactory.Received(1).GetJobHistoryErrorRepository(_workspaceArtifactId);
			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<HashSet<int>>(x => x.Count == 1 && x.Contains(error1)));
			_jobHistoryErrorItemErrorExcluded.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<HashSet<int>>(x => x.Count == 1 && x.Contains(error2)));
		}

		[Test]
		public void CreateErrorListTempTablesForItemLevelErrors_MultipleBatches_NoExpiredItemLevelErrors()
		{
			// Arrange
			const int lastJobHistoryId = 2322133;
			const int savedSearchId = 2321393;
			const int artifactTypeId = 10;
			const int documentId1 = 100501;
			const int documentId2 = 100502;
			const int error1 = 424324;
			const int error2 = 234234;
			const string controlNumber1 = "REL0000000179.0001";
			const string controlNumber2 = "REL0000000179.0002";
			var itemLevelErrorsAndSourceUniqueIds = new Dictionary<int, string>()
			{
				{error1, controlNumber1},
				{error2, controlNumber2}
			};

			var artifactDtoBatchOne = new ArtifactDTO[]
			{
				new ArtifactDTO(documentId1, artifactTypeId, controlNumber1, new ArtifactFieldDTO[0]),
			};

			var artifactDtoBatchTwo = new ArtifactDTO[]
			{
				new ArtifactDTO(documentId2, artifactTypeId, controlNumber2, new ArtifactFieldDTO[0]),
			};

			_jobHistoryRepository.GetLastJobHistoryArtifactId(_integrationPointArtifactId).Returns(lastJobHistoryId);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(itemLevelErrorsAndSourceUniqueIds);
			_repositoryFactory.GetSavedSearchRepository(_workspaceArtifactId, savedSearchId).Returns(_savedSearchRepository);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(false, false, true);
			_savedSearchRepository.RetrieveNextDocuments().Returns(x => artifactDtoBatchOne, x => artifactDtoBatchTwo, x => new ArtifactDTO[0]);

			// Act
			_testInstance.CreateErrorListTempTablesForItemLevelErrors(_job, savedSearchId);

			// Assert
			_repositoryFactory.Received(1).GetJobHistoryErrorRepository(_workspaceArtifactId);
			_repositoryFactory.Received(1).GetJobHistoryRepository(_workspaceArtifactId);
			_jobHistoryRepository.Received(1).GetLastJobHistoryArtifactId(_integrationPointArtifactId);
			_jobHistoryErrorRepository.Received(1).RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item);
			_repositoryFactory.Received(1).GetSavedSearchRepository(_workspaceArtifactId, savedSearchId);
			_savedSearchRepository.Received(3).AllDocumentsRetrieved();
			_savedSearchRepository.Received(2).RetrieveNextDocuments();

			_jobHistoryErrorItemErrorIncluded.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<HashSet<int>>(y => itemLevelErrorsAndSourceUniqueIds.Keys.SequenceEqual(y)));
			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorExcluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void CreateErrorListTempTablesForItemLevelErrors_MultipleBatches_AllExpiredItemLevelErrors()
		{
			// Arrange
			const int lastJobHistoryId = 2322133;
			const int savedSearchId = 2321393;
			const int artifactTypeId = 10;
			const int documentId1 = 100501;
			const int documentId2 = 100502;
			const int error1 = 424324;
			const int error2 = 234234;
			const string controlNumber1 = "REL0000000179.0001";
			const string controlNumber2 = "REL0000000179.0002";
			var itemLevelErrorsAndSourceUniqueIds = new Dictionary<int, string>()
			{
				{error1, "NEVER"},
				{error2, "GUNNA"}
			};

			var artifactDtoBatchOne = new ArtifactDTO[]
			{
				new ArtifactDTO(documentId1, artifactTypeId, controlNumber1, new ArtifactFieldDTO[0]),
			};

			var artifactDtoBatchTwo = new ArtifactDTO[]
			{
				new ArtifactDTO(documentId2, artifactTypeId, controlNumber2, new ArtifactFieldDTO[0]),
			};

			_jobHistoryRepository.GetLastJobHistoryArtifactId(_integrationPointArtifactId).Returns(lastJobHistoryId);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(itemLevelErrorsAndSourceUniqueIds);
			_repositoryFactory.GetSavedSearchRepository(_workspaceArtifactId, savedSearchId).Returns(_savedSearchRepository);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(false, false, true);
			_savedSearchRepository.RetrieveNextDocuments().Returns(x => artifactDtoBatchOne, x => artifactDtoBatchTwo, x => new ArtifactDTO[0]);

			// Act
			_testInstance.CreateErrorListTempTablesForItemLevelErrors(_job, savedSearchId);

			// Assert
			_repositoryFactory.Received(1).GetJobHistoryErrorRepository(_workspaceArtifactId);
			_repositoryFactory.Received(1).GetJobHistoryRepository(_workspaceArtifactId);
			_jobHistoryRepository.Received(1).GetLastJobHistoryArtifactId(_integrationPointArtifactId);
			_jobHistoryErrorRepository.Received(1).RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item);
			_repositoryFactory.Received(1).GetSavedSearchRepository(_workspaceArtifactId, savedSearchId);
			_savedSearchRepository.Received(3).AllDocumentsRetrieved();
			_savedSearchRepository.Received(2).RetrieveNextDocuments();

			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorExcluded.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<HashSet<int>>(y => itemLevelErrorsAndSourceUniqueIds.Keys.SequenceEqual(y)));
		}

		[Test]
		public void CreateErrorListTempTablesForItemLevelErrors_MultipleBatches_CurrentAndExpiredItemsLevelErrors()
		{
			// Arrange
			const int lastJobHistoryId = 2322133;
			const int savedSearchId = 2321393;
			const int artifactTypeId = 10;
			const int documentId1 = 100501;
			const int documentId2 = 100502;
			const int error1 = 424324;
			const int error2 = 234234;
			const int error3 = 89324;
			const int error4 = 999234;
			const string controlNumber1 = "REL0000000179.0001";
			const string controlNumber2 = "REL0000000179.0002";
			var itemLevelErrorsAndSourceUniqueIds = new Dictionary<int, string>()
			{
				{error1, "NEVER"},
				{error2, controlNumber1},
				{error3, controlNumber2},
				{error4, "GUNNA"},
			};

			var artifactDtoBatchOne = new ArtifactDTO[]
			{
				new ArtifactDTO(documentId1, artifactTypeId, controlNumber1, new ArtifactFieldDTO[0]),
			};

			var artifactDtoBatchTwo = new ArtifactDTO[]
			{
				new ArtifactDTO(documentId2, artifactTypeId, controlNumber2, new ArtifactFieldDTO[0]),
			};

			_jobHistoryRepository.GetLastJobHistoryArtifactId(_integrationPointArtifactId).Returns(lastJobHistoryId);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Returns(itemLevelErrorsAndSourceUniqueIds);
			_repositoryFactory.GetSavedSearchRepository(_workspaceArtifactId, savedSearchId).Returns(_savedSearchRepository);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(false, false, true);
			_savedSearchRepository.RetrieveNextDocuments().Returns(x => artifactDtoBatchOne, x => artifactDtoBatchTwo, x => new ArtifactDTO[0]);

			// Act
			_testInstance.CreateErrorListTempTablesForItemLevelErrors(_job, savedSearchId);

			// Assert
			_repositoryFactory.Received(1).GetJobHistoryErrorRepository(_workspaceArtifactId);
			_repositoryFactory.Received(1).GetJobHistoryRepository(_workspaceArtifactId);
			_jobHistoryRepository.Received(1).GetLastJobHistoryArtifactId(_integrationPointArtifactId);
			_jobHistoryErrorRepository.Received(1).RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item);
			_repositoryFactory.Received(1).GetSavedSearchRepository(_workspaceArtifactId, savedSearchId);
			_savedSearchRepository.Received(3).AllDocumentsRetrieved();
			_savedSearchRepository.Received(2).RetrieveNextDocuments();

			_jobHistoryErrorJobError.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemErrorIncluded.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<HashSet<int>>(y => y.SequenceEqual(new[] { error2, error3 })));
			_jobHistoryErrorItemErrorExcluded.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<HashSet<int>>(y => y.SequenceEqual(new[] { error1, error4 })));
		}
	}
}