using System;
using Castle.MicroKernel.Registration;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class DestinationWorkspaceRepositoryTests : RelativityProviderTemplate
	{
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private DestinationWorkspace _destinationWorkspaceDto;
		private IJobHistoryService _jobHistoryService;
		private IScratchTableRepository _scratchTableRepository;

		public DestinationWorkspaceRepositoryTests() : base("DestinationWorkspaceRepositoryTests", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			var repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(SourceWorkspaceArtifactID);
			_destinationWorkspaceDto = _destinationWorkspaceRepository.Create(SourceWorkspaceArtifactID, "DestinationWorkspaceRepositoryTests", -1, "This Instance");
			var federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			var federatedInstanceDto = new FederatedInstanceDto()
			{
				ArtifactId = 12345,
				Name = "federatedInstanceName"
			};
			federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(Arg.Any<int>()).Returns(federatedInstanceDto);
			var thisInstanceDto = new FederatedInstanceDto()
			{
				Name = "This Instance",
				ArtifactId = null
			};
			federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(null).Returns(thisInstanceDto);
			Container.Register(Component.For<IFederatedInstanceManager>().Instance(federatedInstanceManager).IsDefault());
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_scratchTableRepository = repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactID, "Documents2Tag", "LikeASir");
		}

		public override void SuiteTeardown()
		{
			_scratchTableRepository.Dispose();
			base.SuiteTeardown();
		}

		[IdentifiedTest("83e65d4c-39e4-4753-8427-2fb73af3f875")]
		public void Query_DestinationWorkspaceDto_Success()
		{
			//Act
			var queriedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(SourceWorkspaceArtifactID, -1);

			//Assert
			Assert.AreEqual(_destinationWorkspaceDto.ArtifactId, queriedDestinationWorkspaceDto.ArtifactId);
			Assert.AreEqual(_destinationWorkspaceDto.DestinationWorkspaceName, queriedDestinationWorkspaceDto.DestinationWorkspaceName);
		}

		[IdentifiedTest("148c3ec2-7a40-4674-b25f-cb2da3eb4864")]
		public void Query_DestinationWorkspaceDto_ReturnsNull()
		{
			//Act
			var queriedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(-1, -1);

			//Assert
			Assert.IsNull(queriedDestinationWorkspaceDto);
		}

		[IdentifiedTest("f6a801bb-f793-44b2-adcb-11f432ba019b")]
		public void Update_DestinationWorkspaceDto_Success()
		{
			//Arrange
			const string expectedWorkspaceName = "Updated Workspace";

			var destinationWorkspaceDto = new DestinationWorkspace
			{
				ArtifactId = _destinationWorkspaceDto.ArtifactId,
				DestinationWorkspaceArtifactID = _destinationWorkspaceDto.DestinationWorkspaceArtifactID,
				DestinationWorkspaceName = expectedWorkspaceName,
				DestinationInstanceName = _destinationWorkspaceDto.DestinationInstanceName
			};

			//Act
			_destinationWorkspaceRepository.Update(destinationWorkspaceDto);
			var updatedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(SourceWorkspaceArtifactID, -1);

			//Assert
			Assert.AreEqual(_destinationWorkspaceDto.ArtifactId, updatedDestinationWorkspaceDto.ArtifactId);
			Assert.AreEqual(expectedWorkspaceName, updatedDestinationWorkspaceDto.DestinationWorkspaceName);
		}

		[IdentifiedTestCase("78ae6365-1ea7-420b-bb82-42d1e59c6b40", null)]
		[IdentifiedTestCase("1556dd98-4abf-424e-8a20-e9b1301422d8", 1000)]
		public void Link_JobHistoryErrorToDestinationWorkspace_Success(int? federatedInstanceArtifactId)
		{
			//Arrange
			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly, federatedInstanceArtifactId),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			IntegrationPoint integrationPoint = IntegrationPointRepository.ReadAsync(integrationModelCreated.ArtifactID)
				.GetAwaiter().GetResult();

			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRun, DateTime.Now);

			//Act
			_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceDto.ArtifactId, jobHistory.ArtifactId);
			JobHistory linkedJobHistory = _jobHistoryService.GetRdo(batchInstance);

			//Assert
			Assert.AreEqual($"DestinationWorkspaceRepositoryTests - {SourceWorkspaceArtifactID}", linkedJobHistory.DestinationWorkspace);
			CollectionAssert.Contains(linkedJobHistory.DestinationWorkspaceInformation, _destinationWorkspaceDto.ArtifactId);
		}

		[IdentifiedTest("466c92e1-accc-4996-b62a-7b8e4e6b9b48")]
		public void Create_DestinationWorkspaceDTOWithInvalidWorkspaceId_EmptyArtifactId()
		{
			//Arrange
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(Substitute.For<IRelativityObjectManager>());

			//Act
			var destinationWorkspace = destinationWorkspaceRepository.Create(-999, "Invalid Workspace", -1, "This Instance");

			//Assert
			Assert.AreEqual(destinationWorkspace.ArtifactId, 0);
		}

		[IdentifiedTest("3bb291b1-43d2-4775-a71e-c1dfcb53cd8e")]
		public void Link_DestinationWorkspaceDTOWithInvalidWorkspaceId_ThrowsException()
		{
			//Act & Assert
			Assert.NotNull(_destinationWorkspaceDto.DestinationWorkspaceArtifactID);
			Assert.Throws<IntegrationPointsException>(() => _destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceDto.DestinationWorkspaceArtifactID.Value, -1), "Unable to link Destination Workspace object to Job History object");
		}

		[IdentifiedTest("13c501b8-62cc-45c7-8c8f-38af044f331e")]
		public void Update_DestinationWorkspaceDtoWithInvalidArtifactId_ThrowsError()
		{
			//Arrange
			var destinationWorkspaceDto = new DestinationWorkspace
			{
				ArtifactId = 12345,
				DestinationWorkspaceArtifactID = _destinationWorkspaceDto.DestinationWorkspaceArtifactID,
				DestinationWorkspaceName = _destinationWorkspaceDto.DestinationWorkspaceName,
				DestinationInstanceName = _destinationWorkspaceDto.DestinationInstanceName
			};

			//Act & Assert
			Assert.Throws<Exception>(() => _destinationWorkspaceRepository.Update(destinationWorkspaceDto), "Unable to update instance of Destination Workspace object: Unable to retrieve Destination Workspace instance");
		}
	}
}