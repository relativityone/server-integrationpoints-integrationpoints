using System.Security.Claims;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Data;
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
		private ITempDocTableHelper _tempDocHelper;
		private ITempDocumentTableFactory _docTableFactory;
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
			_tempDocHelper = Substitute.For<ITempDocTableHelper>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_docTableFactory = Substitute.For<ITempDocumentTableFactory>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_onBehalfOfUserClaimsPrincipalFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();

			
			_docTableFactory.GetDocTableHelper(_uniqueJobId, _sourceWorkspaceId).Returns(_tempDocHelper);
			_onBehalfOfUserClaimsPrincipalFactory.CreateClaimsPrincipal(_submittedBy).Returns(_claimsPrincipal);

			_instance = new JobHistoryBatchUpdateManager(_docTableFactory, _repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, _jobHistoryRdoId, _sourceWorkspaceId, _uniqueJobId, _submittedBy);

			_repositoryFactory.GetJobHistoryRepository(_sourceWorkspaceId).Returns(_jobHistoryRepository);
			_docTableFactory.Received().GetDocTableHelper(_uniqueJobId, _sourceWorkspaceId);
			_onBehalfOfUserClaimsPrincipalFactory.Received().CreateClaimsPrincipal(_submittedBy);
		}

		[Test]
		public void OnJobComplete_EmptyDocuments()
		{
			//Arrange

			//Act
			_instance.OnJobComplete(_job);

			//Assert
			_tempDocHelper.Received().DeleteTable(Arg.Is(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST));
			_jobHistoryRepository.Received().TagDocsWithJobHistory(_claimsPrincipal, 0, _jobHistoryRdoId, _sourceWorkspaceId, _uniqueJobId);
		}

		[Test]
		public void OnJobComplete_FullDocuments()
		{
			//Arrange
			//Act
			_instance.OnJobComplete(_job);

			//Assert
			_tempDocHelper.Received().DeleteTable(Arg.Is(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST));
			_jobHistoryRepository.Received().TagDocsWithJobHistory(_claimsPrincipal, Arg.Any<int>(), _jobHistoryRdoId, _sourceWorkspaceId, _uniqueJobId);
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

		[Test]
		public void GetStratchTableRepo_UsingTheCorrectPrefix()
		{
			//Arrange
			string expected = Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST;
			_tempDocHelper.GetTempTableName(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST).Returns(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST);

			//Act
			IScratchTableRepository repository = _instance.ScratchTableRepository;
			string name = repository.GetTempTableName();

			//Assert
			Assert.AreSame(expected, name);
		}
	}
}