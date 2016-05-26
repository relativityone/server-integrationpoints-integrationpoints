using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Managers
{
	[TestFixture]
	public class JobHistoryErrorManagerTests
	{
		private IScratchTableRepository _scratchTableRepository;
		private IJobHistoryErrorManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IJobHistoryRepository _jobHistoryRepository;
		private ISavedSearchRepository _savedSearchRepository;
		private IScratchTableRepository _jobHistoryErrorJobStart;
		private IScratchTableRepository _jobHistoryErrorJobComplete;
		private IScratchTableRepository _jobHistoryErrorItemStart;
		private IScratchTableRepository _jobHistoryErrorItemComplete;
		private IScratchTableRepository _jobHistoryErrorItemStartOther;
		
		private const int _workspaceArtifactId = 102448;
		private const int _integrationPointArtifactId = 4651358;
		private const int _submittedByArtifactId = 2448071;
		private const int _originalSavedSearchArtifactId = 7748963;
		private const string _uniqueJobId = "1324_JobIdGuid";
		private const string _jobErrorOnStartPrefix = "IntegrationPoint_Relativity_JHE_Job1";
		private const string _jobErrorOnCompletePrefix = "IntegrationPoint_Relativity_JHE_Job2";
		private const string _itemErrorOnStartPrefix = "IntegrationPoint_Relativity_JHE_Item1";
		private const string _itemErrorOnCompletePrefix = "IntegrationPoint_Relativity_JHE_Item2";
		private Job _job;

		private readonly List<int> _sampleJobError = new List<int>() { 4598735 };
		private readonly List<int> _sampleItemErrors = new List<int>() { 4598733, 4598734 };

		[SetUp]
		public void Setup()
		{
			_scratchTableRepository = Substitute.For<IScratchTableRepository>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_jobHistoryErrorRepository = Substitute.For<IJobHistoryErrorRepository>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_savedSearchRepository = Substitute.For<ISavedSearchRepository>();
			_jobHistoryErrorJobStart = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorJobComplete = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorItemStart = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorItemComplete = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorItemStartOther = Substitute.For<IScratchTableRepository>();

			_repositoryFactory.GetJobHistoryErrorRepository(_workspaceArtifactId).Returns(_jobHistoryErrorRepository);
			_repositoryFactory.GetJobHistoryRepository(_workspaceArtifactId).Returns(_jobHistoryRepository);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START, _uniqueJobId).Returns(_jobHistoryErrorJobStart);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE, _uniqueJobId).Returns(_jobHistoryErrorJobComplete);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START, _uniqueJobId).Returns(_jobHistoryErrorItemStart);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE, _uniqueJobId).Returns(_jobHistoryErrorItemComplete);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START_OTHER, _uniqueJobId).Returns(_jobHistoryErrorItemStartOther);

			_testInstance = new JobHistoryErrorManager(_repositoryFactory, _workspaceArtifactId, _uniqueJobId);

			_job = new Job(_workspaceArtifactId, _integrationPointArtifactId, _submittedByArtifactId);
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow);

			//Assert
			_scratchTableRepository.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		[Ignore]
		public void StageForUpdatingErrors_RunNow_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow);

			//Assert
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
		}

		[Test]
		[Ignore]
		public void StageForUpdatingErrors_RunNow_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow);

			//Assert
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
		}

		[Test]
		[Ignore]
		public void StageForUpdatingErrors_RunNow_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);
			
			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow);

			//Assert
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_scratchTableRepository.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		[Ignore]
		public void StageForUpdatingErrors_ScheduledRun_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
		}

		[Test]
		[Ignore]
		public void StageForUpdatingErrors_ScheduledRun_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
		}

		[Test]
		[Ignore]
		public void StageForUpdatingErrors_ScheduledRun_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_scratchTableRepository.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		[Ignore]
		public void StageForUpdatingErrors_RetryErrors_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
		}

		[Test]
		[Ignore]
		public void StageForUpdatingErrors_RetryErrors_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_scratchTableRepository.Received(0).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_scratchTableRepository.Received(0).AddArtifactIdsIntoTempTable(_sampleItemErrors);
		}

		[Test]
		public void CreateItemLevelErrorsSavedSearch_GoldFlow()
		{
			//Act
			_testInstance.CreateItemLevelErrorsSavedSearch(_job, _originalSavedSearchArtifactId);

			//Assert
			_jobHistoryRepository.Received().GetLastJobHistoryArtifactId(_integrationPointArtifactId);
			_jobHistoryErrorRepository.Received().CreateItemLevelErrorsSavedSearch(_workspaceArtifactId, _integrationPointArtifactId, 
				_originalSavedSearchArtifactId, 0, _submittedByArtifactId);
		}

		[Test]
		[Ignore]
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
				ErrorTypeChoices.JobHistoryErrorItem).Returns(itemLevelErrorsAndSourceUniqueIds);


			// Act
			_testInstance.CreateErrorListTempTablesForItemLevelErrors(_job, savedSearchId);


			// Assert
			_savedSearchRepository.Received(2).RetrieveNextDocuments();
			_repositoryFactory.Received(2).GetSavedSearchRepository(_workspaceArtifactId, savedSearchId);
			_repositoryFactory.Received(1).GetJobHistoryErrorRepository(_workspaceArtifactId);
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<List<int>>(x => x.Count == 1 && x[0] == error1));
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<List<int>>(x => x.Count == 1 && x[0] == error1));
			_scratchTableRepository.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<List<int>>(x => x.Count == 1 && x[0] == error2));

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
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, ErrorTypeChoices.JobHistoryErrorItem).Returns(itemLevelErrorsAndSourceUniqueIds);
			_repositoryFactory.GetSavedSearchRepository(_workspaceArtifactId, savedSearchId).Returns(_savedSearchRepository);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(false, false, true);
			_savedSearchRepository.RetrieveNextDocuments().Returns(x => artifactDtoBatchOne, x => artifactDtoBatchTwo, x => new ArtifactDTO[0]);

			// Act
			_testInstance.CreateErrorListTempTablesForItemLevelErrors(_job, savedSearchId);

			// Assert
			_repositoryFactory.Received(1).GetJobHistoryErrorRepository(_workspaceArtifactId);
			_repositoryFactory.Received(1).GetJobHistoryRepository(_workspaceArtifactId);
			_jobHistoryRepository.Received(1).GetLastJobHistoryArtifactId(_integrationPointArtifactId);
			_jobHistoryErrorRepository.Received(1).RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, ErrorTypeChoices.JobHistoryErrorItem);
			_repositoryFactory.Received(1).GetSavedSearchRepository(_workspaceArtifactId, savedSearchId);
			_savedSearchRepository.Received(3).AllDocumentsRetrieved();
			_savedSearchRepository.Received(2).RetrieveNextDocuments();

			_jobHistoryErrorItemStart.Received(1)
				.AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => itemLevelErrorsAndSourceUniqueIds.Keys.SequenceEqual(y)));
			_jobHistoryErrorItemComplete.Received(1)
				.AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => itemLevelErrorsAndSourceUniqueIds.Keys.SequenceEqual(y)));
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(null);
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
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, ErrorTypeChoices.JobHistoryErrorItem).Returns(itemLevelErrorsAndSourceUniqueIds);
			_repositoryFactory.GetSavedSearchRepository(_workspaceArtifactId, savedSearchId).Returns(_savedSearchRepository);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(false, false, true);
			_savedSearchRepository.RetrieveNextDocuments().Returns(x => artifactDtoBatchOne, x => artifactDtoBatchTwo, x => new ArtifactDTO[0]);

			// Act
			_testInstance.CreateErrorListTempTablesForItemLevelErrors(_job, savedSearchId);

			// Assert
			_repositoryFactory.Received(1).GetJobHistoryErrorRepository(_workspaceArtifactId);
			_repositoryFactory.Received(1).GetJobHistoryRepository(_workspaceArtifactId);
			_jobHistoryRepository.Received(1).GetLastJobHistoryArtifactId(_integrationPointArtifactId);
			_jobHistoryErrorRepository.Received(1).RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, ErrorTypeChoices.JobHistoryErrorItem);
			_repositoryFactory.Received(1).GetSavedSearchRepository(_workspaceArtifactId, savedSearchId);
			_savedSearchRepository.Received(3).AllDocumentsRetrieved();
			_savedSearchRepository.Received(2).RetrieveNextDocuments();

			_jobHistoryErrorItemStart.DidNotReceiveWithAnyArgs()
				.AddArtifactIdsIntoTempTable(null);
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs()
				.AddArtifactIdsIntoTempTable(null);
			_jobHistoryErrorItemStartOther.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => itemLevelErrorsAndSourceUniqueIds.Keys.SequenceEqual(y)));
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
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, ErrorTypeChoices.JobHistoryErrorItem).Returns(itemLevelErrorsAndSourceUniqueIds);
			_repositoryFactory.GetSavedSearchRepository(_workspaceArtifactId, savedSearchId).Returns(_savedSearchRepository);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(false, false, true);
			_savedSearchRepository.RetrieveNextDocuments().Returns(x => artifactDtoBatchOne, x => artifactDtoBatchTwo, x => new ArtifactDTO[0]);

			// Act
			_testInstance.CreateErrorListTempTablesForItemLevelErrors(_job, savedSearchId);

			// Assert
			_repositoryFactory.Received(1).GetJobHistoryErrorRepository(_workspaceArtifactId);
			_repositoryFactory.Received(1).GetJobHistoryRepository(_workspaceArtifactId);
			_jobHistoryRepository.Received(1).GetLastJobHistoryArtifactId(_integrationPointArtifactId);
			_jobHistoryErrorRepository.Received(1).RetrieveJobHistoryErrorIdsAndSourceUniqueIds(lastJobHistoryId, ErrorTypeChoices.JobHistoryErrorItem);
			_repositoryFactory.Received(1).GetSavedSearchRepository(_workspaceArtifactId, savedSearchId);
			_savedSearchRepository.Received(3).AllDocumentsRetrieved();
			_savedSearchRepository.Received(2).RetrieveNextDocuments();

			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => y.SequenceEqual(new [] {error2, error3})));
			_jobHistoryErrorItemComplete.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => y.SequenceEqual(new [] {error2, error3})));
			_jobHistoryErrorItemStartOther.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => y.SequenceEqual(new [] {error1, error4})));
		}
	}
}
