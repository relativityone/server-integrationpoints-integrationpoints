using System.Collections.Generic;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
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
		private ITempDocumentTableFactory _docTableFactory;
		private IRepositoryFactory _repositoryFactory;
		private IBatchStatus _instance;
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly int _jobHistoryRdoId = 12345;
		private readonly string _tableSuffix= "12-25-96";
		private SourceConfiguration _sourceConfig;
		private readonly Job _job;

		[SetUp]
		public void Setup()
		{
			_tempDocHelper = Substitute.For<ITempDocTableHelper>();
			_destinationWorkspaceRepository = Substitute.For<IDestinationWorkspaceRepository>();
			_docTableFactory = Substitute.For<ITempDocumentTableFactory>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();

			_sourceConfig = new SourceConfiguration();
			_sourceConfig.SourceWorkspaceArtifactId = 56879;
			_sourceConfig.TargetWorkspaceArtifactId = 98765;

			_repositoryFactory.GetDestinationWorkspaceRepository(_sourceConfig.SourceWorkspaceArtifactId, _sourceConfig.TargetWorkspaceArtifactId)
				.Returns(_destinationWorkspaceRepository);
			_docTableFactory.GetDocTableHelper(_tableSuffix, _sourceConfig.SourceWorkspaceArtifactId).Returns(_tempDocHelper);

			_instance = new DestinationWorkspaceManager(_docTableFactory, _repositoryFactory, _sourceConfig,
				_tableSuffix, _jobHistoryRdoId);
		}

		[Test]
		public void JobComplete_EmptyDocuments()
		{
			//Arrange
			var documentIds = new List<int>();
			int destinationWorkspaceInstanceId = 0401;

			_tempDocHelper.GetDocumentIdsFromTable(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS).Returns(documentIds);
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(destinationWorkspaceInstanceId);
			
			//Act
			_instance.JobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(destinationWorkspaceInstanceId, _jobHistoryRdoId);
			_tempDocHelper.Received().DeleteTable(Arg.Is(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS));

			_destinationWorkspaceRepository.DidNotReceive().CreateDestinationWorkspaceRdoInstance();
			_destinationWorkspaceRepository.Received().TagDocsWithDestinationWorkspace(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>());
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

			_tempDocHelper.GetDocumentIdsFromTable(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS).Returns(documentIds);
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(-1);
			_destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance().Returns(destinationWorkspaceInstanceId);

			//Act
			_instance.JobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received().QueryDestinationWorkspaceRdoInstance();
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(destinationWorkspaceInstanceId, _jobHistoryRdoId);
			_destinationWorkspaceRepository.Received().CreateDestinationWorkspaceRdoInstance();
			_destinationWorkspaceRepository.Received().TagDocsWithDestinationWorkspace(documentIds.Count, destinationWorkspaceInstanceId, _tableSuffix, _sourceConfig.SourceWorkspaceArtifactId);

			_tempDocHelper.Received().DeleteTable(Arg.Is(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS));
		}
	}
}
