using System.Collections.Generic;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Agent;
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
		private readonly string _tableSuffix = "12-25-96";
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
			int destinationWorkspaceInstanceId = 0401;

			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(destinationWorkspaceInstanceId);

			//Act
			_instance.JobComplete(_job);

			//Assert
			_tempDocHelper.Received().DeleteTable(Arg.Is(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS));
			_destinationWorkspaceRepository.Received().TagDocsWithDestinationWorkspace(Arg.Any<int>(), null, Arg.Any<string>(), Arg.Any<int>());
		}

		[Test]
		public void JobComplete_FullDocuments()
		{
			//Act
			_instance.JobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received().TagDocsWithDestinationWorkspace(Arg.Any<int>(), null, _tableSuffix, _sourceConfig.SourceWorkspaceArtifactId);

			_tempDocHelper.Received().DeleteTable(Arg.Is(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS));
		}
	}
}