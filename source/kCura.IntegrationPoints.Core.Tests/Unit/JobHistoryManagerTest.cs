using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
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

			_instance = new JobHistoryManager(_tempDocHelper, _jobHistoryRepository, _jobHistoryRdoId, _sourceWorkspaceId, _uniqueJobId);
		}

		[Test]
		public void JobComplete_EmptyDocuments()
		{
			//Arrange
			var documentIds = new List<int>();
			_tempDocHelper.GetDocumentIdsFromTable(ScratchTables.JobHistory).Returns(documentIds);

			//Act
			_instance.JobComplete(_job);
			
			//Assert
			_tempDocHelper.Received().DeleteTable(Arg.Is(ScratchTables.JobHistory));
			_jobHistoryRepository.DidNotReceive().TagDocsWithJobHistory(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
		}

		[Test]
		public void JobComplete_FullDocuments()
		{
			//Arrange
			var documentIds = new List<int>();
			documentIds.Add(1);
			documentIds.Add(2);
			documentIds.Add(3);

			_tempDocHelper.GetDocumentIdsFromTable(ScratchTables.JobHistory).Returns(documentIds);

			//Act
			_instance.JobComplete(_job);

			//Assert
			_tempDocHelper.DidNotReceive().DeleteTable(Arg.Is(ScratchTables.JobHistory));
			_jobHistoryRepository.Received().TagDocsWithJobHistory(documentIds.Count, _jobHistoryRdoId, _sourceWorkspaceId, _uniqueJobId);
		}
	}
}
