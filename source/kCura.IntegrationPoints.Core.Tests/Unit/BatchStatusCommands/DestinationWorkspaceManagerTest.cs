using System;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class DestinationWorkspaceManagerTest
	{
		private ITempDocTableHelper _tempDocHelper;
		private ITempDocumentTableFactory _docTableFactory;
		private IRepositoryFactory _repositoryFactory;
		private IConsumeScratchTableBatchStatus _instance;
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
		public void JobStart_CreateWorkspaceRdoAndLinkToJobHistory()
		{

			// Arrange
			int destinationRdo = 789;
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(-1);
			_destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance().Returns(destinationRdo);

			// Act
			_instance.JobStarted(_job);

			// Assert
			_destinationWorkspaceRepository.Received().CreateDestinationWorkspaceRdoInstance();
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(destinationRdo, _jobHistoryRdoId);
		}

		[Test]
		public void JobStart_DoesntCreateWorkspaceRdoWhenItAlreadyExists()
		{

			// Arrange
			int destinationRdo = 789;
			int someOtherRdo = 879;
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(destinationRdo);
			_destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance().Returns(someOtherRdo);

			// Act
			_instance.JobStarted(_job);

			// Assert
			_destinationWorkspaceRepository.DidNotReceive().CreateDestinationWorkspaceRdoInstance();
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(destinationRdo, _jobHistoryRdoId);
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
			string expected = Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS;
			_tempDocHelper.GetTempTableName(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS).Returns(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS);

			//Act
			IScratchTableRepository repository = _instance.ScratchTableRepository;
			string name = repository.GetTempTableName();

			//Assert
			Assert.AreSame(expected, name);
		}

		[Test]
		public void ErrorOccurDuringJobStart_OnQueryDestinationWorkspaceRdoInstance()
		{
			//Arrange
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Throws(new Exception());
			
			//Act
			try
			{
				_instance.JobStarted(_job);
			}
			catch
			{
			}

			_instance.JobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.DidNotReceive().TagDocsWithDestinationWorkspace(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}

		[Test]
		public void ErrorOccurDuringJobStart_OnCreateDestinationWorkspaceRdoInstance()
		{
			//Arrange
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(-1);
			_destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance().Throws(new Exception());

			//Act
			try
			{
				_instance.JobStarted(_job);
			}
			catch
			{
			}

			_instance.JobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.DidNotReceive().TagDocsWithDestinationWorkspace(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}


		[Test]
		public void ErrorOccurDuringJobStart_LinkDestinationWorkspaceToJobHistory()
		{
			//Arrange
			int rdo = 111;
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance().Returns(rdo);
			_destinationWorkspaceRepository.When( x => x.LinkDestinationWorkspaceToJobHistory(rdo, _jobHistoryRdoId)).Do(
				x => { throw new Exception(); });

			//Act
			try
			{
				_instance.JobStarted(_job);
			}
			catch
			{
			}

			_instance.JobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.DidNotReceiveWithAnyArgs().TagDocsWithDestinationWorkspace(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}

	}
}