﻿using kCura.IntegrationPoint.Tests.Core.Extensions;
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
		private const string _jobErrorOnStartPrefix = "IntegrationPoint_Relativity_JobHistoryErrors_JobStart";
		private const string _jobErrorOnCompletePrefix = "IntegrationPoint_Relativity_JobHistoryErrors_JobComplete";
		private const string _itemErrorOnStartPrefix = "IntegrationPoint_Relativity_JobHistoryErrorsE_ItemStart";
		private const string _itemErrorOnCompletePrefix = "IntegrationPoint_Relativity_JobHistoryErrors_ItemComplete";
		private const string _itemErrorOnStartOtherPrefix = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart_Excluded";
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
			_jobHistoryErrorJobStart = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorJobComplete = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorItemStart = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorItemComplete = Substitute.For<IScratchTableRepository>();
			_jobHistoryErrorItemStartOther = Substitute.For<IScratchTableRepository>();

			_repositoryFactory.GetJobHistoryErrorRepository(_workspaceArtifactId).Returns(_jobHistoryErrorRepository);
			_repositoryFactory.GetJobHistoryRepository(_workspaceArtifactId).Returns(_jobHistoryRepository);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, _jobErrorOnStartPrefix, _uniqueJobId).Returns(_jobHistoryErrorJobStart);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, _jobErrorOnCompletePrefix, _uniqueJobId).Returns(_jobHistoryErrorJobComplete);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, _itemErrorOnStartPrefix, _uniqueJobId).Returns(_jobHistoryErrorItemStart);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, _itemErrorOnCompletePrefix, _uniqueJobId).Returns(_jobHistoryErrorItemComplete);
			_repositoryFactory.GetScratchTableRepository(_workspaceArtifactId, _itemErrorOnStartOtherPrefix, _uniqueJobId).Returns(_jobHistoryErrorItemStartOther);

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
			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun);

			//Assert
			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors);

			//Assert
			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorJobComplete.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorJobComplete.Received(1).AddArtifactIdsIntoTempTable(_sampleJobError);
			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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
			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.Received(0).AddArtifactIdsIntoTempTable(_sampleItemErrors);
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.Received(0).AddArtifactIdsIntoTempTable(_sampleItemErrors);
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
			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<List<int>>(x => x.Count == 1 && x[0] == error1));
			_jobHistoryErrorItemComplete.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<List<int>>(x => x.Count == 1 && x[0] == error1));
			_jobHistoryErrorItemStartOther.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<List<int>>(x => x.Count == 1 && x[0] == error2));
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

			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => itemLevelErrorsAndSourceUniqueIds.Keys.SequenceEqual(y)));
			_jobHistoryErrorItemComplete.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => itemLevelErrorsAndSourceUniqueIds.Keys.SequenceEqual(y)));
			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStartOther.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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

			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
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

			_jobHistoryErrorJobStart.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorJobComplete.DidNotReceiveWithAnyArgs().AddArtifactIdsIntoTempTable(Arg.Any<List<int>>());
			_jobHistoryErrorItemStart.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => y.SequenceEqual(new [] {error2, error3})));
			_jobHistoryErrorItemComplete.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => y.SequenceEqual(new [] {error2, error3})));
			_jobHistoryErrorItemStartOther.Received(1).AddArtifactIdsIntoTempTable(Arg.Is<IList<int>>(y => y.SequenceEqual(new [] {error1, error4})));
		}
	}
}