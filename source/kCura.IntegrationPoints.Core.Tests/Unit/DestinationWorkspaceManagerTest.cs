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
	public class DestinationWorkspaceManagerTest
	{
		private ITempDocTableHelper _tempDocHelper;
		private IBatchStatus _instance;
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly int _jobHistoryRdoId = 12345;
		private readonly string _tableSuffix= "12-25-96";
		private readonly int _sourceWorkspaceId = 56879;
		private readonly Job _job;

		[SetUp]
		public void Setup()
		{
			_tempDocHelper = Substitute.For<ITempDocTableHelper>();
			_destinationWorkspaceRepository = Substitute.For<IDestinationWorkspaceRepository>();

			_instance = new DestinationWorkspaceManager(_tempDocHelper, _destinationWorkspaceRepository, _jobHistoryRdoId, _tableSuffix, _sourceWorkspaceId);
		}

		[Test]
		public void JobComplete_EmptyDocuments()
		{
			//Arrange
			var documentIds = new List<int>();
			int destinationWorkspaceInstanceId = 0401;

			_tempDocHelper.GetDocumentIdsFromTable(ScratchTables.DestinationWorkspace).Returns(documentIds);
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(destinationWorkspaceInstanceId);
			
			//Act
			_instance.JobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(destinationWorkspaceInstanceId, _jobHistoryRdoId);
			_tempDocHelper.Received().DeleteTable(Arg.Is(ScratchTables.DestinationWorkspace));

			_destinationWorkspaceRepository.DidNotReceive().CreateDestinationWorkspaceRdoInstance();
			_destinationWorkspaceRepository.DidNotReceive().TagDocsWithDestinationWorkspace(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>());
		}

		[Test]
		public void JobComplete_FullDocuments()
		{
			//Arrange
			var documentIds = new List<int>();
			int destinationWorkspaceInstanceId = 0401;

			documentIds.Add(1);
			documentIds.Add(2);
			documentIds.Add(3);

			_tempDocHelper.GetDocumentIdsFromTable(ScratchTables.DestinationWorkspace).Returns(documentIds);
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(-1);
			_destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance().Returns(destinationWorkspaceInstanceId);

			//Act
			_instance.JobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received().QueryDestinationWorkspaceRdoInstance();
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(destinationWorkspaceInstanceId, _jobHistoryRdoId);
			_destinationWorkspaceRepository.Received().CreateDestinationWorkspaceRdoInstance();
			_destinationWorkspaceRepository.Received().TagDocsWithDestinationWorkspace(documentIds.Count, destinationWorkspaceInstanceId, _tableSuffix, _sourceWorkspaceId);

			_tempDocHelper.DidNotReceive().DeleteTable(Arg.Is(ScratchTables.DestinationWorkspace));
		}
	}
}
