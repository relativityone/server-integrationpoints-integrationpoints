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
		private const string _scratchTableName = "IntegrationPoint_Relativity_JobHistory";
		private readonly Job _job;

		[SetUp]
		public void Setup()
		{
			_scratchTableRepository = Substitute.For<IScratchTableRepository>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_onBehalfOfUserClaimsPrincipalFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();

			_scratchTableRepository.GetTempTableName().Returns(_scratchTableName);
			_repositoryFactory.GetScratchTableRepository(_sourceWorkspaceId, _scratchTableName, Arg.Any<string>()).ReturnsForAnyArgs(_scratchTableRepository);

			_repositoryFactory.GetJobHistoryRepository(_sourceWorkspaceId).Returns(_jobHistoryRepository);
			_onBehalfOfUserClaimsPrincipalFactory.CreateClaimsPrincipal(_submittedBy).Returns(_claimsPrincipal);

			_instance = new JobHistoryBatchUpdateManager(_repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, _sourceWorkspaceId, _jobHistoryRdoId, _submittedBy, _uniqueJobId);
			_onBehalfOfUserClaimsPrincipalFactory.Received(1).CreateClaimsPrincipal(_submittedBy);
		}

		[Test]
		public void OnJobComplete_EmptyDocuments()
		{
			//Arrange

			//Act
			_instance.OnJobComplete(_job);

			//Assert
			_scratchTableRepository.Received(1).Dispose();
			_jobHistoryRepository.Received(1).TagDocsWithJobHistory(_claimsPrincipal, 0, _jobHistoryRdoId, _sourceWorkspaceId, _scratchTableName);
		}

		[Test]
		public void OnJobComplete_FullDocuments()
		{
			//Arrange
			//Act
			_instance.OnJobComplete(_job);

			//Assert
			_scratchTableRepository.Received(1).Dispose();
			_jobHistoryRepository.Received(1).TagDocsWithJobHistory(_claimsPrincipal, Arg.Any<int>(), _jobHistoryRdoId, _sourceWorkspaceId, _scratchTableName);
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