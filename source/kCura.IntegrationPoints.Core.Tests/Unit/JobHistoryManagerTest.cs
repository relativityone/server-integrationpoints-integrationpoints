using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class JobHistoryManagerTest
	{
		private ITempDocTableHelper _tempDocHelper;
		private ITempDocumentTableFactory _docTableFactory;
		private IRepositoryFactory _repositoryFactory;
		private IBatchStatus _instance;
		private IJobHistoryRepository _jobHistoryRepository;
		private readonly int _jobHistoryRdoId = 12345;
		private readonly string _uniqueJobId = "12-25-96";
		private readonly int _sourceWorkspaceId = 56879;
		private readonly Job _job;

		[SetUp]
		public void Setup()
		{
			_tempDocHelper = Substitute.For<ITempDocTableHelper>();
			_jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
			_docTableFactory = Substitute.For<ITempDocumentTableFactory>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();

			_repositoryFactory.GetJobHistoryRepository().Returns(_jobHistoryRepository);
			_docTableFactory.GetDocTableHelper(_uniqueJobId, _sourceWorkspaceId).Returns(_tempDocHelper);

			_instance = new JobHistoryManager(_docTableFactory, _repositoryFactory, _jobHistoryRdoId, _sourceWorkspaceId, _uniqueJobId);
		}

		[Test]
		public void JobComplete_EmptyDocuments()
		{
			//Arrange

			//Act
			_instance.JobComplete(_job);

			//Assert
			_tempDocHelper.Received().DeleteTable(Arg.Is(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST));
			_jobHistoryRepository.Received().TagDocsWithJobHistory(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
		}

		[Test]
		public void JobComplete_FullDocuments()
		{
			//Arrange
			//Act
			_instance.JobComplete(_job);

			//Assert
			_tempDocHelper.Received().DeleteTable(Arg.Is(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST));
			_jobHistoryRepository.Received().TagDocsWithJobHistory(Arg.Any<int>(), _jobHistoryRdoId, _sourceWorkspaceId, _uniqueJobId);
		}
	}
}