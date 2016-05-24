using System.Security.Claims;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.BatchStatusCommands
{
	[TestFixture]
	public class JobHistoryBatchUpdateManagerTest
	{
		private IScratchTableRepository _scratchTableRepository;
		private IRepositoryFactory _repositoryFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _onBehalfOfUserClaimsPrincipalFactory;
		private ClaimsPrincipal _claimsPrincipal;
		private IConsumeScratchTableBatchStatus _instance;
		private IJobHistoryRepository _jobHistoryRepository;
		private readonly int _jobHistoryRdoId = 12345;
		private readonly string _uniqueJobId = "12-25-96";
		private readonly int _sourceWorkspaceId = 56879;
		private readonly int _submittedBy = 4141;
		private readonly Job _job;

		[SetUp]
		public void Setup()
		{
			_scratchTableRepository = Substitute.For<IScratchTableRepository>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_onBehalfOfUserClaimsPrincipalFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_scratchTableRepository.GetTempTableName().Returns(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST);

			
			_onBehalfOfUserClaimsPrincipalFactory.CreateClaimsPrincipal(_submittedBy).Returns(_claimsPrincipal);

			_instance = new JobHistoryBatchUpdateManager(_repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, _jobHistoryRdoId, _sourceWorkspaceId, _submittedBy, _uniqueJobId);

			_repositoryFactory.GetJobHistoryRepository(_sourceWorkspaceId).Returns(_jobHistoryRepository);
			_onBehalfOfUserClaimsPrincipalFactory.Received().CreateClaimsPrincipal(_submittedBy);
		}

		[Test]
		[Ignore]
		public void OnJobComplete_EmptyDocuments()
		{
			//Arrange

			//Act
			_instance.OnJobComplete(_job);

			//Assert
			_scratchTableRepository.Received().DeleteTable();
			_jobHistoryRepository.Received().TagDocsWithJobHistory(_claimsPrincipal, 0, _jobHistoryRdoId, _sourceWorkspaceId, Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST);
		}

		[Test]
		[Ignore]
		public void OnJobComplete_FullDocuments()
		{
			//Arrange
			//Act
			_instance.OnJobComplete(_job);

			//Assert
			_scratchTableRepository.Received().DeleteTable();
			_jobHistoryRepository.Received().TagDocsWithJobHistory(_claimsPrincipal, Arg.Any<int>(), _jobHistoryRdoId, _sourceWorkspaceId, Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST);
		}

		[Test]
		public void GetStratchTableRepo_AlwaysGivesTheSameObject()
		{
			//Act
			IScratchTableRepository repository = _instance.ScratchTableRepository;
			IScratchTableRepository repository2 = _instance.ScratchTableRepository;

			//Assert
			Assert.AreSame(repository, repository2);
		}
	}
}