using System.Collections.Generic;
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
		private IJobHistoryErrorManager _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IJobHistoryRepository _jobHistoryRepository;

		
		private const int _workspaceArtifactId = 102448;
		private const int _integrationPointArtifactId = 4651358;
		private const int _submittedByArtifactId = 2448071;
		private const int _originalSavedSearchArtifactId = 7748963;
		private const string _uniqueJobId = "petDetective";
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
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_jobHistoryErrorRepository = Substitute.For<IJobHistoryErrorRepository>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_testInstance = new JobHistoryErrorManager(_repositoryFactory);

			_repositoryFactory.GetJobHistoryErrorRepository(_workspaceArtifactId).Returns(_jobHistoryErrorRepository);
			_repositoryFactory.GetJobHistoryRepository(_workspaceArtifactId).Returns(_jobHistoryRepository);
			
			_job = new Job(_workspaceArtifactId, _integrationPointArtifactId, _submittedByArtifactId);
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().CreateErrorListTempTable(Arg.Any<List<int>>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleJobError, _jobErrorOnStartPrefix, _uniqueJobId);
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleJobError, _jobErrorOnStartPrefix, _uniqueJobId);
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleItemErrors, _itemErrorOnStartPrefix, _uniqueJobId);
		}

		[Test]
		public void StageForUpdatingErrors_RunNow_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRunNow, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleItemErrors, _itemErrorOnStartPrefix, _uniqueJobId);
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().CreateErrorListTempTable(Arg.Any<List<int>>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleJobError, _jobErrorOnStartPrefix, _uniqueJobId);
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleJobError, _jobErrorOnStartPrefix, _uniqueJobId);
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleItemErrors, _itemErrorOnStartPrefix, _uniqueJobId);
		}

		[Test]
		public void StageForUpdatingErrors_ScheduledRun_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryScheduledRun, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleItemErrors, _itemErrorOnStartPrefix, _uniqueJobId);
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_NoErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).ReturnsForAnyArgs(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.DidNotReceiveWithAnyArgs().CreateErrorListTempTable(Arg.Any<List<int>>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_JobError()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(new List<int>());

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleJobError, _jobErrorOnStartPrefix, _uniqueJobId);
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleJobError, _jobErrorOnCompletePrefix, _uniqueJobId);
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_JobAndItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(_sampleJobError);
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleJobError, _jobErrorOnStartPrefix, _uniqueJobId);
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleJobError, _jobErrorOnCompletePrefix, _uniqueJobId);
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleItemErrors, _itemErrorOnStartPrefix, _uniqueJobId);
		}

		[Test]
		public void StageForUpdatingErrors_RetryErrors_ItemErrors()
		{
			//Arrange
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorJob).Returns(new List<int>());
			_jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(0, ErrorTypeChoices.JobHistoryErrorItem).Returns(_sampleItemErrors);

			//Act
			_testInstance.StageForUpdatingErrors(_job, JobTypeChoices.JobHistoryRetryErrors, _uniqueJobId);

			//Assert
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleItemErrors, _itemErrorOnStartPrefix, _uniqueJobId);
			_jobHistoryErrorRepository.Received().CreateErrorListTempTable(_sampleItemErrors, _itemErrorOnCompletePrefix, _uniqueJobId);
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
	}
}
