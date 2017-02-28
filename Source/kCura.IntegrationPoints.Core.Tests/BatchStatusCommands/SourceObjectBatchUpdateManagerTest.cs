using System;
using System.Security.Claims;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Security;
using IFederatedInstanceManager = kCura.IntegrationPoints.Domain.Managers.IFederatedInstanceManager;

namespace kCura.IntegrationPoints.Core.Tests.BatchStatusCommands
{
	[TestFixture]
	public class SourceObjectBatchUpdateManagerTest : TestBase
	{
		private IScratchTableRepository _scratchTableRepository;
		private IHelper _helper;
		private IRepositoryFactory _repositoryFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _onBehalfOfUserClaimsPrincipalFactory;
		private ClaimsPrincipal _claimsPrincipal = null;
		private IWorkspaceRepository _workspaceRepository;
		private IConsumeScratchTableBatchStatus _instance;
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly int _jobHistoryRdoId = 12345;
		private readonly int _destWorkspaceInstanceId = 54321;
		private readonly int _federatedInstanceId = 134648;
		private readonly int _destinationWorkspaceId = 99999;
		private const string _scratchTableName = "IntegrationPoint_Relativity_DestinationWorkspace";
		private readonly string _destWorkspaceName = "Workspace X";
		private readonly string _federatedInstanceName = "fed_name";
		private readonly string _updatedDestWorkspaceName = "New Workspace Name";
		private readonly int _submittedBy = 4141;
		private readonly string _uniqueJobId = "1_SomeGuid";
		private SourceConfiguration _sourceConfig;
		private readonly Job _job = null;
		private DestinationWorkspace _emptyDestinationWorkspace;
		private DestinationWorkspace _normalDestinationWorkspace;
		private WorkspaceDTO _workspaceX;
		private WorkspaceDTO _workspaceY;

		[SetUp]
		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_scratchTableRepository = Substitute.For<IScratchTableRepository>();
			_destinationWorkspaceRepository = Substitute.For<IDestinationWorkspaceRepository>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_onBehalfOfUserClaimsPrincipalFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_workspaceRepository = Substitute.For<IWorkspaceRepository>();
			var federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(_federatedInstanceId).Returns(new FederatedInstanceDto {Name = _federatedInstanceName});

			_sourceConfig = new SourceConfiguration();
			_sourceConfig.SourceWorkspaceArtifactId = 56879;
			_sourceConfig.TargetWorkspaceArtifactId = _destinationWorkspaceId;
			_sourceConfig.FederatedInstanceArtifactId = _federatedInstanceId;

			_emptyDestinationWorkspace = null;
			_normalDestinationWorkspace = new DestinationWorkspace()
			{
				ArtifactId = _destWorkspaceInstanceId,
				DestinationWorkspaceArtifactID = _destinationWorkspaceId,
				DestinationWorkspaceName = _destWorkspaceName,
				DestinationInstanceName = _federatedInstanceName,
				DestinationInstanceArtifactID = _federatedInstanceId
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
			_onBehalfOfUserClaimsPrincipalFactory.CreateClaimsPrincipal(_submittedBy).Returns(_claimsPrincipal);

			_repositoryFactory.GetScratchTableRepository(_sourceConfig.SourceWorkspaceArtifactId, _scratchTableName, Arg.Any<string>()).ReturnsForAnyArgs(_scratchTableRepository);
			_scratchTableRepository.GetTempTableName().Returns(_scratchTableName);

			_instance = new SourceObjectBatchUpdateManager(_repositoryFactory, _repositoryFactory, _onBehalfOfUserClaimsPrincipalFactory, _helper, federatedInstanceManager, _sourceConfig,
				_jobHistoryRdoId, _submittedBy, _uniqueJobId);

			_repositoryFactory.Received().GetDestinationWorkspaceRepository(_sourceConfig.SourceWorkspaceArtifactId);
			_repositoryFactory.Received().GetWorkspaceRepository();
			_onBehalfOfUserClaimsPrincipalFactory.Received().CreateClaimsPrincipal(_submittedBy);
		}

		[Test]
		public void OnJobStart_CreateWorkspaceRdoAndLinkToJobHistory()
		{

			// Arrange
			_destinationWorkspaceRepository.Query(Arg.Any<int>(), Arg.Any<int>()).Returns(_emptyDestinationWorkspace);
			_destinationWorkspaceRepository.Create(_destinationWorkspaceId, _destWorkspaceName, _federatedInstanceId, _federatedInstanceName).Returns(_normalDestinationWorkspace);
			_workspaceRepository.Retrieve(_destinationWorkspaceId).Returns(_workspaceX); //name has not been changed

			// Act
			_instance.OnJobStart(_job);

			// Assert
			_destinationWorkspaceRepository.Received(1).Query(Arg.Any<int>(), Arg.Any<int>());
			_destinationWorkspaceRepository.Received(1).Create(_destinationWorkspaceId, _destWorkspaceName, _federatedInstanceId, _federatedInstanceName);
			_destinationWorkspaceRepository.Received(1).LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId);
			_workspaceRepository.Received(1).Retrieve(_destinationWorkspaceId);
			_destinationWorkspaceRepository.DidNotReceive().Update(Arg.Any<DestinationWorkspace>()); 
		}

		[Test]
		public void OnJobStart_DoesntCreateWorkspaceRdoWhenItAlreadyExists()
		{

			// Arrange
			_destinationWorkspaceRepository.Query(_destinationWorkspaceId, _federatedInstanceId).Returns(_normalDestinationWorkspace);
			_workspaceRepository.Retrieve(_destinationWorkspaceId).Returns(_workspaceX); //name has not been changed

			// Act
			_instance.OnJobStart(_job);

			// Assert
			_destinationWorkspaceRepository.Received(1).Query(_destinationWorkspaceId, _federatedInstanceId);
			_destinationWorkspaceRepository.DidNotReceive().Create(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
			_destinationWorkspaceRepository.Received(1).LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId);
			_workspaceRepository.Received(1).Retrieve(_destinationWorkspaceId);
		}

		[Test]
		public void OnJobStart_UpdateWorkspaceInstanceName()
		{

			// Arrange
			_destinationWorkspaceRepository.Query(_destinationWorkspaceId, _federatedInstanceId).Returns(_normalDestinationWorkspace);
			_workspaceRepository.Retrieve(_destinationWorkspaceId).Returns(_workspaceY); //name of destination case has changed

			// Act
			_instance.OnJobStart(_job);

			// Assert
			_destinationWorkspaceRepository.Received(1).Query(_destinationWorkspaceId, _federatedInstanceId);
			_destinationWorkspaceRepository.DidNotReceive().Create(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
			_destinationWorkspaceRepository.Received(1).Update(_normalDestinationWorkspace);
			_destinationWorkspaceRepository.Received(1).LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId);
			_workspaceRepository.Received(1).Retrieve(_destinationWorkspaceId);
		}

		[Test]
		public void OnJobComplete_EmptyDocuments()
		{
			//Act
			_instance.OnJobComplete(_job);

			//Assert
			_scratchTableRepository.Received(1).Dispose();
			_destinationWorkspaceRepository.Received(1).TagDocsWithDestinationWorkspaceAndJobHistory(_claimsPrincipal, 0, 0, _jobHistoryRdoId, _scratchTableName, _sourceConfig.SourceWorkspaceArtifactId);
		}

		[Test]
		public void OnJobComplete_FullDocuments()
		{
			//Act
			_instance.OnJobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received(1).TagDocsWithDestinationWorkspaceAndJobHistory(_claimsPrincipal, 0, 0, _jobHistoryRdoId, _scratchTableName, _sourceConfig.SourceWorkspaceArtifactId);
			_scratchTableRepository.Received(1).Dispose();
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
		public void ErrorOccurDuringOnJobStart_OnQuery()
		{
			//Arrange
			_destinationWorkspaceRepository.Query(_destinationWorkspaceId, _federatedInstanceId).Throws(new Exception());
			
			//Act
			try
			{
				_instance.OnJobStart(_job);
			}
			catch
			{
			}

			_instance.OnJobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received(1).Query(_destinationWorkspaceId, _federatedInstanceId);
			_destinationWorkspaceRepository.DidNotReceive().TagDocsWithDestinationWorkspaceAndJobHistory(_claimsPrincipal, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}

		[Test]
		public void ErrorOccurDuringOnJobStart_OnCreate()
		{
			//Arrange
			_destinationWorkspaceRepository.Query(_destinationWorkspaceId, _federatedInstanceId).Returns(_emptyDestinationWorkspace);
			_destinationWorkspaceRepository.Create(_destinationWorkspaceId, _destWorkspaceName, _federatedInstanceId, _federatedInstanceName).Throws(new Exception());

			//Act
			try
			{
				_instance.OnJobStart(_job);
			}
			catch
			{
			}

			_instance.OnJobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received(1).Query(_destinationWorkspaceId, _federatedInstanceId);
			_destinationWorkspaceRepository.DidNotReceive().TagDocsWithDestinationWorkspaceAndJobHistory(_claimsPrincipal, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}


		[Test]
		public void ErrorOccurDuringOnJobStart_LinkDestinationWorkspaceToJobHistory()
		{
			//Arrange
			_destinationWorkspaceRepository.Query(_destinationWorkspaceId, _federatedInstanceId).Returns(_normalDestinationWorkspace);
			_destinationWorkspaceRepository.When( x => x.LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId)).Do(
				x => { throw new Exception(); });

			//Act
			try
			{
				_instance.OnJobStart(_job);
			}
			catch
			{
			}

			_instance.OnJobComplete(_job);

			//Assert
			_destinationWorkspaceRepository.Received(1).Query(_destinationWorkspaceId, _federatedInstanceId);
			_destinationWorkspaceRepository.Received(1).When(
				x => x.LinkDestinationWorkspaceToJobHistory(_destWorkspaceInstanceId, _jobHistoryRdoId));
			_destinationWorkspaceRepository.DidNotReceiveWithAnyArgs().TagDocsWithDestinationWorkspaceAndJobHistory(_claimsPrincipal, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<String>(), Arg.Any<int>());
		}

	}
}