﻿using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.BatchStatusCommands
{
	[TestFixture]
	public class DestinationWorkspaceManagerTest
	{
		private ITempDocTableHelper _tempDocHelper;
		private ITempDocumentTableFactory _docTableFactory;
		private IRepositoryFactory _repositoryFactory;
		private IWorkspaceRepository _workspaceRepository;
		private IConsumeScratchTableBatchStatus _instance;
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly int _jobHistoryRdoId = 12345;
		private readonly int _destWorkspaceInstanceId = 54321;
		private readonly int _destinationWorkspaceId = 99999;
		private readonly string _tableSuffix = "12-25-96";
		private readonly string _destWorkspaceName = "Workspace X";
		private readonly string _updatedDestWorkspaceName = "New Workspace Name";
		private SourceConfiguration _sourceConfig;
		private readonly Job _job;
		private DestinationWorkspaceDTO _emptyDestinationWorkspace;
		private DestinationWorkspaceDTO _destinationWorkspace;
		private WorkspaceDTO _workspaceX;
		private WorkspaceDTO _workspaceY;

		[SetUp]
		public void Setup()
		{
			_tempDocHelper = Substitute.For<ITempDocTableHelper>();
			_destinationWorkspaceRepository = Substitute.For<IDestinationWorkspaceRepository>();
			_docTableFactory = Substitute.For<ITempDocumentTableFactory>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_workspaceRepository = Substitute.For<IWorkspaceRepository>();

			_sourceConfig = new SourceConfiguration();
			_sourceConfig.SourceWorkspaceArtifactId = 56879;
			_sourceConfig.TargetWorkspaceArtifactId = _destinationWorkspaceId;

			_emptyDestinationWorkspace = null;
			_destinationWorkspace = new DestinationWorkspaceDTO()
			{
				ArtifactId = _destWorkspaceInstanceId,
				WorkspaceArtifactId = _destinationWorkspaceId,
				WorkspaceName = _destWorkspaceName
			};

			_workspaceX = new WorkspaceDTO()
			{
				ArtifactId = _sourceConfig.TargetWorkspaceArtifactId,
				Name = _destWorkspaceName
			};
			_workspaceY = new WorkspaceDTO()
			{
				ArtifactId = _sourceConfig.TargetWorkspaceArtifactId,
				Name = _updatedDestWorkspaceName
			};

			_repositoryFactory.GetDestinationWorkspaceRepository(_sourceConfig.SourceWorkspaceArtifactId)
				.Returns(_destinationWorkspaceRepository);
			_repositoryFactory.GetWorkspaceRepository().Returns(_workspaceRepository);
			_docTableFactory.GetDocTableHelper(_tableSuffix, _sourceConfig.SourceWorkspaceArtifactId).Returns(_tempDocHelper);

			_instance = new DestinationWorkspaceManager(_docTableFactory, _repositoryFactory, _sourceConfig,
				_tableSuffix, _jobHistoryRdoId);
		}

		/*[Test]
		public void JobStart_CreateWorkspaceRdoAndLinkToJobHistory()
		{

			// Arrange
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance(Arg.Any<int>()).Returns(_emptyDestinationWorkspace);
			_destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance(_destinationWorkspaceId, _destWorkspaceName).Returns(_destinationWorkspace);
			//_workspaceRepository.Retrieve(_destinationWorkspaceId).Returns(_workspaceX); //name has not been changed

			// Act
			_instance.JobStarted(_job);

			// Assert
			_destinationWorkspaceRepository.Received().QueryDestinationWorkspaceRdoInstance(Arg.Any<int>());
			_destinationWorkspaceRepository.Received().CreateDestinationWorkspaceRdoInstance(_destinationWorkspaceId, _destWorkspaceName);
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId);
			//_workspaceRepository.Received().Retrieve(_destinationWorkspaceId);
			_destinationWorkspaceRepository.DidNotReceive().UpdateDestinationWorkspaceRdoInstance(Arg.Any<DestinationWorkspaceDTO>()); 
		}*/

		[Test]
		public void JobStart_DoesntCreateWorkspaceRdoWhenItAlreadyExists()
		{

			// Arrange
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId).Returns(_destinationWorkspace);
			_workspaceRepository.Retrieve(_destinationWorkspaceId).Returns(_workspaceX); //name has not been changed

			// Act
			_instance.JobStarted(_job);

			// Assert
			_destinationWorkspaceRepository.Received().QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId);
			_destinationWorkspaceRepository.DidNotReceive().CreateDestinationWorkspaceRdoInstance(Arg.Any<int>(), Arg.Any<string>());
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId);
			//_workspaceRepository.Received().Retrieve(_destinationWorkspaceId);
		}

		[Test]
		public void JobStart_UpdateWorkspaceInstanceName()
		{

			// Arrange
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId).Returns(_destinationWorkspace);
			_workspaceRepository.Retrieve(_destinationWorkspaceId).Returns(_workspaceY); //name of destination case has changed

			// Act
			_instance.JobStarted(_job);

			// Assert
			_destinationWorkspaceRepository.Received().QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId);
			_destinationWorkspaceRepository.DidNotReceive().CreateDestinationWorkspaceRdoInstance(Arg.Any<int>(), Arg.Any<string>());
			_destinationWorkspaceRepository.Received().UpdateDestinationWorkspaceRdoInstance(_destinationWorkspace);
			_destinationWorkspaceRepository.Received().LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId);
			//_workspaceRepository.Received().Retrieve(_destinationWorkspaceId);
		}

		[Test]
		public void JobComplete_EmptyDocuments()
		{
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
			_tempDocHelper.Received().GetTempTableName(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS);
		}

		[Test]
		public void ErrorOccurDuringJobStart_OnQueryDestinationWorkspaceRdoInstance()
		{
			//Arrange
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId).Throws(new Exception());
			
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
			_destinationWorkspaceRepository.Received().QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId);
			_destinationWorkspaceRepository.DidNotReceive().TagDocsWithDestinationWorkspace(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}

		[Test]
		public void ErrorOccurDuringJobStart_OnCreateDestinationWorkspaceRdoInstance()
		{
			//Arrange
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId).Returns(_emptyDestinationWorkspace);
			_destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance(_destinationWorkspaceId, _destWorkspaceName).Throws(new Exception());

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
			_destinationWorkspaceRepository.Received().QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId);
			_destinationWorkspaceRepository.DidNotReceive().TagDocsWithDestinationWorkspace(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}


		[Test]
		public void ErrorOccurDuringJobStart_LinkDestinationWorkspaceToJobHistory()
		{
			//Arrange
			_destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId).Returns(_destinationWorkspace);
			_destinationWorkspaceRepository.When( x => x.LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId)).Do(
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
			_destinationWorkspaceRepository.Received().QueryDestinationWorkspaceRdoInstance(_destinationWorkspaceId);
			_destinationWorkspaceRepository.Received().When(
				x => x.LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId));
			_destinationWorkspaceRepository.DidNotReceiveWithAnyArgs().TagDocsWithDestinationWorkspace(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}

	}
}