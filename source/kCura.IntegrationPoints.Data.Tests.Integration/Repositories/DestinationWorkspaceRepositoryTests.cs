using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using Castle.MicroKernel.Registration;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class DestinationWorkspaceRepositoryTests : RelativityProviderTemplate
	{
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private DestinationWorkspace _destinationWorkspaceDto;
		private IJobHistoryService _jobHistoryService;
		private IScratchTableRepository _scratchTableRepository;
		private IDocumentRepository _documentRepository;
		private IFieldQueryRepository _fieldQueryRepository;

		public DestinationWorkspaceRepositoryTests() : base("DestinationWorkspaceRepositoryTests", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(SourceWorkspaceArtifactId);
			_destinationWorkspaceDto = _destinationWorkspaceRepository.Create(SourceWorkspaceArtifactId, "DestinationWorkspaceRepositoryTests", -1, "This Instance");
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
			Container.Register(Component.For <IFederatedInstanceManager>().Instance(federatedInstanceManager).IsDefault());
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_scratchTableRepository = repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactId, "Documents2Tag", "LikeASir");
			_documentRepository = repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
			_fieldQueryRepository = repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactId);
		}

		public override void SuiteTeardown()
		{
			_scratchTableRepository.Dispose();
			base.SuiteTeardown();
		}

		[Test]
		public void Query_DestinationWorkspaceDto_Success()
		{
			//Act
			DestinationWorkspace queriedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(SourceWorkspaceArtifactId, -1);

			//Assert
			Assert.AreEqual(_destinationWorkspaceDto.ArtifactId, queriedDestinationWorkspaceDto.ArtifactId);
			Assert.AreEqual(_destinationWorkspaceDto.DestinationWorkspaceArtifactID, queriedDestinationWorkspaceDto.DestinationWorkspaceArtifactID);
			Assert.AreEqual(_destinationWorkspaceDto.DestinationWorkspaceName, queriedDestinationWorkspaceDto.DestinationWorkspaceName);
		}

		[Test]
		public void Query_DestinationWorkspaceDto_ReturnsNull()
		{
			//Act
			DestinationWorkspace queriedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(-1, -1);

			//Assert
			Assert.IsNull(queriedDestinationWorkspaceDto);
		}

		[Test]
		public void Update_DestinationWorkspaceDto_Success()
		{
			//Arrange
			const string expectedWorkspaceName = "Updated Workspace";

			DestinationWorkspace destinationWorkspaceDto = new DestinationWorkspace
			{
				ArtifactId = _destinationWorkspaceDto.ArtifactId,
				DestinationWorkspaceArtifactID = _destinationWorkspaceDto.DestinationWorkspaceArtifactID,
				DestinationWorkspaceName = expectedWorkspaceName,
				DestinationInstanceName = _destinationWorkspaceDto.DestinationInstanceName
			};

			//Act
			_destinationWorkspaceRepository.Update(destinationWorkspaceDto);
			DestinationWorkspace updatedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(SourceWorkspaceArtifactId, -1);

			//Assert
			Assert.AreEqual(_destinationWorkspaceDto.ArtifactId, updatedDestinationWorkspaceDto.ArtifactId);
			Assert.AreEqual(_destinationWorkspaceDto.DestinationWorkspaceArtifactID, updatedDestinationWorkspaceDto.DestinationWorkspaceArtifactID);
			Assert.AreEqual(expectedWorkspaceName, updatedDestinationWorkspaceDto.DestinationWorkspaceName);
		}

		[Test]
		[TestCase(null)]
		[TestCase(1000)]
		public void Link_JobHistoryErrorToDestinationWorkspace_Success(int? federatedInstanceArtifactId)
		{
			//Arrange
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly, federatedInstanceArtifactId),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationModelCreated.ArtifactID);

			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRun, DateTime.Now);

			//Act
			_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceDto.ArtifactId, jobHistory.ArtifactId);
			JobHistory linkedJobHistory = _jobHistoryService.GetRdo(batchInstance);

			//Assert
			Assert.AreEqual($"DestinationWorkspaceRepositoryTests - {SourceWorkspaceArtifactId}", linkedJobHistory.DestinationWorkspace);
			CollectionAssert.Contains(linkedJobHistory.DestinationWorkspaceInformation, _destinationWorkspaceDto.ArtifactId);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void Tag_DocumentsWithDestinationWorkspaceAndJobHistory_Success()
		{
			//Arrange
			DataTable dataTable = Import.GetImportTable("DocsToTag", 10);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, dataTable);
			int[] documentArtifactIds = _documentRepository.RetrieveDocumentByIdentifierPrefixAsync(Fields.GetDocumentIdentifierFieldName(_fieldQueryRepository), "DocsToTag").ConfigureAwait(false).GetAwaiter().GetResult();
			_scratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIds);

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationModelCreated.ArtifactID);

			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRun, DateTime.Now);

			//Act
			_destinationWorkspaceRepository.TagDocsWithDestinationWorkspaceAndJobHistory(ClaimsPrincipal.Current, 10, _destinationWorkspaceDto.ArtifactId, jobHistory.ArtifactId, _scratchTableRepository.GetTempTableName(), SourceWorkspaceArtifactId);

			//Assert
			VerifyDocumentTagging(documentArtifactIds, jobHistory.Name);
		}

		[Test]
		public void Create_DestinationWorkspaceDTOWithInvalidWorkspaceId_EmptyArtifactId()
		{
			//Arrange
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(Helper, -1, Substitute.For<IRSAPIService>());

			//Act
			DestinationWorkspace destinationWorkspace= destinationWorkspaceRepository.Create(-999, "Invalid Workspace", -1, "This Instance");

			//Assert
			Assert.AreEqual(destinationWorkspace.ArtifactId, 0);
		}

		[Test]
		public void Link_DestinationWorkspaceDTOWithInvalidWorkspaceId_ThrowsException()
		{
			//Act & Assert
			Assert.Throws<Exception>(() => _destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceDto.DestinationWorkspaceArtifactID.Value, -1), "Unable to link Destination Workspace object to Job History object");
		}

		[Test]
		public void Update_DestinationWorkspaceDtoWithInvalidArtifactId_ThrowsError()
		{
			//Arrange
			DestinationWorkspace destinationWorkspaceDto = new DestinationWorkspace
			{
				ArtifactId = 12345,
				DestinationWorkspaceArtifactID = _destinationWorkspaceDto.DestinationWorkspaceArtifactID,
				DestinationWorkspaceName = _destinationWorkspaceDto.DestinationWorkspaceName,
				DestinationInstanceName = _destinationWorkspaceDto.DestinationInstanceName
			};

			//Act & Assert
			Assert.Throws<Exception>(() => _destinationWorkspaceRepository.Update(destinationWorkspaceDto), "Unable to update instance of Destination Workspace object: Unable to retrieve Destination Workspace instance");
		}

		[Test]
		public void Tag_DocumentWithInvalidArtifactId_ThrowsError()
		{
			//Act & Assert
			Assert.Throws<Exception>(() => _destinationWorkspaceRepository.TagDocsWithDestinationWorkspaceAndJobHistory(ClaimsPrincipal.Current, 1, -1, -1, "None", SourceWorkspaceArtifactId), "Tagging Documents with DestinationWorkspace and JobHistory object failed - Mass Edit failure.");
		}

		private void VerifyDocumentTagging(int[] documentArtifactIds, string expectedJobHistory)
		{
			string expectedDestinationCase = $"DestinationWorkspaceRepositoryTests - { SourceWorkspaceArtifactId }";
			int? documentJobHistoryFieldArtifactId = _fieldQueryRepository.RetrieveField((int)Relativity.Client.ArtifactType.Document, "Job History", FieldTypes.MultipleObject, new HashSet<string>() { "ArtifactID" })?.ArtifactId;
			int? documentDestinationCaseFieldArtifactId = _fieldQueryRepository.RetrieveField((int)Relativity.Client.ArtifactType.Document, "Relativity Destination Case", FieldTypes.MultipleObject, new HashSet<string>() { "ArtifactID" })?.ArtifactId;
			ArtifactDTO[] documentArtifacts = _documentRepository.RetrieveDocumentsAsync(documentArtifactIds, new HashSet<int>() { documentDestinationCaseFieldArtifactId.GetValueOrDefault(), documentJobHistoryFieldArtifactId.GetValueOrDefault() }).ConfigureAwait(false).GetAwaiter().GetResult();

			foreach (ArtifactDTO artifact in documentArtifacts)
			{
				StringAssert.Contains(expectedDestinationCase, artifact.Fields[0].Value.ToString());
				StringAssert.Contains(expectedJobHistory, artifact.Fields[1].Value.ToString());
			}
		}
	}
}