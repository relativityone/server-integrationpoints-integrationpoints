using System;
using Castle.MicroKernel.Registration;
using FluentAssertions;
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

		public DestinationWorkspaceRepositoryTests() : base(
			sourceWorkspaceName: "DestinationWorkspaceRepositoryTests",
			targetWorkspaceName: null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(SourceWorkspaceArtifactID);
			_destinationWorkspaceDto = _destinationWorkspaceRepository.Create(SourceWorkspaceArtifactID, "DestinationWorkspaceRepositoryTests", -1, "This Instance");
			IFederatedInstanceManager federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			var federatedInstanceDto = new FederatedInstanceDto
			{
				ArtifactId = 12345,
				Name = "federatedInstanceName"
			};
			federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(Arg.Any<int>()).Returns(federatedInstanceDto);
			var thisInstanceDto = new FederatedInstanceDto
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
			DestinationWorkspace queriedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(
				SourceWorkspaceArtifactID,
				federatedInstanceArtifactId: -1);

			//Assert
			queriedDestinationWorkspaceDto.ArtifactId.Should()
				.Be(_destinationWorkspaceDto.ArtifactId);
			queriedDestinationWorkspaceDto.DestinationWorkspaceName.Should()
				.Be(_destinationWorkspaceDto.DestinationWorkspaceName);
		}

		[IdentifiedTest("148c3ec2-7a40-4674-b25f-cb2da3eb4864")]
		public void Query_DestinationWorkspaceDto_ReturnsNull()
		{
			//Act
			DestinationWorkspace queriedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(
				targetWorkspaceArtifactId: -1,
				federatedInstanceArtifactId: -1);

			//Assert
			queriedDestinationWorkspaceDto.Should().BeNull();
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
			DestinationWorkspace updatedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(
				SourceWorkspaceArtifactID,
				federatedInstanceArtifactId: -1);

			//Assert
			updatedDestinationWorkspaceDto.ArtifactId.Should().Be(_destinationWorkspaceDto.ArtifactId);
			updatedDestinationWorkspaceDto.DestinationWorkspaceName.Should().Be(expectedWorkspaceName);
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
			string expectedDestinationWorkspaceName = $"DestinationWorkspaceRepositoryTests - {SourceWorkspaceArtifactID}";
			linkedJobHistory.DestinationWorkspace.Should().Be(expectedDestinationWorkspaceName);
			linkedJobHistory.DestinationWorkspaceInformation.Should().Contain(_destinationWorkspaceDto.ArtifactId);
		}

		[IdentifiedTest("466c92e1-accc-4996-b62a-7b8e4e6b9b48")]
		public void Create_DestinationWorkspaceDTOWithInvalidWorkspaceId_EmptyArtifactId()
		{
			//Arrange
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(Substitute.For<IRelativityObjectManager>());

			//Act
			DestinationWorkspace destinationWorkspace = destinationWorkspaceRepository.Create(
				targetWorkspaceArtifactId: -999,
				targetWorkspaceName: "Invalid Workspace",
				federatedInstanceArtifactId: -1,
				federatedInstanceName: "This Instance");

			//Assert
			destinationWorkspace.ArtifactId.Should().Be(0);
		}

		[IdentifiedTest("3bb291b1-43d2-4775-a71e-c1dfcb53cd8e")]
		public void Link_DestinationWorkspaceDTOWithInvalidWorkspaceId_ThrowsException()
		{
			// Act
			Action linkDestinationWorkspaceAction = () => _destinationWorkspaceRepository
				.LinkDestinationWorkspaceToJobHistory(
					_destinationWorkspaceDto.DestinationWorkspaceArtifactID.Value,
					jobHistoryInstanceId: -1);

			// Assert
			_destinationWorkspaceDto.DestinationWorkspaceArtifactID.Should().NotBeNull();
			linkDestinationWorkspaceAction.ShouldThrow<IntegrationPointsException>()
				.WithMessage("Unable to link Destination Workspace object to Job History object");
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

			//Act
			Action updateDestinationWorkspaceAction =
				() => _destinationWorkspaceRepository.Update(destinationWorkspaceDto);

			//Assert
			string expectedExceptionMessagePart = "Cannot UPDATE object of type DestinationWorkspace with ObjectManager";
			updateDestinationWorkspaceAction.ShouldThrow<IntegrationPointsException>()
				.Which.Message.Should().Contain(expectedExceptionMessagePart);
		}
	}
}