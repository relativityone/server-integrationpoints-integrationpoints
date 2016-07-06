using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Security.Claims;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Category("Integration Tests")]
	public class DestinationWorkspaceRepositoryTests : RelativityProviderTemplate
	{
		private IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private DestinationWorkspaceDTO _destinationWorkspaceDto;
		private IJobHistoryService _jobHistoryService;
		private IScratchTableRepository _scratchTableRepository;
		private IDocumentRepository _documentRepository;

		public DestinationWorkspaceRepositoryTests() : base("DestinationWorkspaceRepositoryTests", null)
		{
		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(SourceWorkspaceArtifactId);
			_destinationWorkspaceDto = _destinationWorkspaceRepository.Create(SourceWorkspaceArtifactId, "DestinationWorkspaceRepositoryTests");
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_scratchTableRepository = repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactId, "Documents2Tag", "LikeASir");
			_documentRepository = repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
		}

		[TearDown]
		public void DeleteTempTable()
		{
			_scratchTableRepository.DeleteTable();
		}

		[Test]
		public void Query_DestinationWorkspaceDto_Success()
		{
			//Act
			DestinationWorkspaceDTO queriedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(SourceWorkspaceArtifactId);

			//Assert
			Assert.AreEqual(_destinationWorkspaceDto.ArtifactId, queriedDestinationWorkspaceDto.ArtifactId);
			Assert.AreEqual(_destinationWorkspaceDto.WorkspaceArtifactId, queriedDestinationWorkspaceDto.WorkspaceArtifactId);
			Assert.AreEqual(_destinationWorkspaceDto.WorkspaceName, queriedDestinationWorkspaceDto.WorkspaceName);
		}

		[Test]
		public void Query_DestinationWorkspaceDto_ReturnsNull()
		{
			//Act
			DestinationWorkspaceDTO queriedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(-1);

			//Assert
			Assert.IsNull(queriedDestinationWorkspaceDto);
		}

		[Test]
		public void Update_DestinationWorkspaceDto_Success()
		{
			//Arrange
			const string expectedWorkspaceName = "Updated Workspace";

			DestinationWorkspaceDTO destinationWorkspaceDto = new DestinationWorkspaceDTO
			{
				ArtifactId = _destinationWorkspaceDto.ArtifactId,
				WorkspaceArtifactId = _destinationWorkspaceDto.WorkspaceArtifactId,
				WorkspaceName = expectedWorkspaceName
			};

			//Act
			_destinationWorkspaceRepository.Update(destinationWorkspaceDto);
			DestinationWorkspaceDTO updatedDestinationWorkspaceDto = _destinationWorkspaceRepository.Query(SourceWorkspaceArtifactId);

			//Assert
			Assert.AreEqual(_destinationWorkspaceDto.ArtifactId, updatedDestinationWorkspaceDto.ArtifactId);
			Assert.AreEqual(_destinationWorkspaceDto.WorkspaceArtifactId, updatedDestinationWorkspaceDto.WorkspaceArtifactId);
			Assert.AreEqual(expectedWorkspaceName, updatedDestinationWorkspaceDto.WorkspaceName);
		}

		[Test]
		public void Link_JobHistoryErrorToDestinationWorkspace_Success()
		{
			//Arrange
			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
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
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationModelCreated.ArtifactID);

			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRunNow, DateTime.Now);

			//Act
			_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceDto.ArtifactId, jobHistory.ArtifactId);
			JobHistory linkedJobHistory = _jobHistoryService.GetRdo(batchInstance);

			//Assert
			Assert.AreEqual($"DestinationWorkspaceRepositoryTests - {SourceWorkspaceArtifactId}", linkedJobHistory.DestinationWorkspace);
			CollectionAssert.Contains(linkedJobHistory.DestinationWorkspaceInformation, _destinationWorkspaceDto.ArtifactId);
		}

		[Test]
		public void Tag_DocumentsWithDestinationWorkspaceAndJobHistory_Success()
		{
			//Arrange
			DataTable dataTable = GetImportTable("DocsToTag", 10);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, dataTable);
			List<int> documentArtifactIds = GetDocumentArtifactIdsByIdentifier("DocsToTag");
			_scratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIds);

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
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
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationModelCreated.ArtifactID);

			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRunNow, DateTime.Now);

			//Act
			_destinationWorkspaceRepository.TagDocsWithDestinationWorkspaceAndJobHistory(ClaimsPrincipal.Current, 10, _destinationWorkspaceDto.ArtifactId, jobHistory.ArtifactId, _scratchTableRepository.GetTempTableName(), SourceWorkspaceArtifactId);

			//Assert
			VerifyDocumentTagging(documentArtifactIds, jobHistory.Name);
		}

		[Test]
		public void Create_DestinationWorkspaceDTOWithInvalidWorkspaceId_ThrowsException()
		{
			//Arrange
			IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(Helper, -1);
			//Act & Assert
			Assert.Throws<Exception>(() => destinationWorkspaceRepository.Create(-999, "Invalid Workspace"), "Unable to create a new instance of Destination Workspace object");
		}

		[Test]
		public void Link_DestinationWorkspaceDTOWithInvalidWorkspaceId_ThrowsException()
		{
			//Act & Assert
			Assert.Throws<Exception>(() => _destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(_destinationWorkspaceDto.WorkspaceArtifactId, -1), "Unable to link Destination Workspace object to Job History object");
		}

		[Test]
		public void Update_DestinationWorkspaceDtoWithInvalidArtifactId_ThrowsError()
		{
			//Arrange
			DestinationWorkspaceDTO destinationWorkspaceDto = new DestinationWorkspaceDTO
			{
				ArtifactId = 12345,
				WorkspaceArtifactId = _destinationWorkspaceDto.WorkspaceArtifactId,
				WorkspaceName = _destinationWorkspaceDto.WorkspaceName
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

		private DataTable GetImportTable(string documentPrefix, int numberOfDocuments)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));

			for (int index = 1; index <= numberOfDocuments; index++)
			{
				string controlNumber = $"{documentPrefix}{index}";
				table.Rows.Add(controlNumber);
			}
			return table;
		}

		private List<int> GetDocumentArtifactIdsByIdentifier(string documentIdentifier)
		{
			List<int> documentArtifactIds = new List<int>();
			string query = $"SELECT [ArtifactID] FROM [Document] WHERE ControlNumber like '{documentIdentifier}%'";

			using (DbDataReader dataReader = CaseContext.SqlContext.ExecuteSqlStatementAsDbDataReader(query))
			{
				while (dataReader.Read())
				{
					documentArtifactIds.Add(dataReader.GetInt32(0));
				}
			}
			return documentArtifactIds;
		}
		private int GetDocumentFieldArtifactId(string fieldName)
		{
			const int documentArtifactTypeId = 10;
			string query = $"SELECT [ArtifactID] FROM [Field] WHERE [DisplayName] = '{fieldName}' AND [FieldArtifactTypeID] = {documentArtifactTypeId}";
			int artifactId = CaseContext.SqlContext.ExecuteSqlStatementAsScalar<int>(query);
			return artifactId;
		}

		private void VerifyDocumentTagging(List<int> documentArtifactIds, string expectedJobHistory)
		{
			string expectedDestinationCase = $"DestinationWorkspaceRepositoryTests - { SourceWorkspaceArtifactId }";
			int documentJobHistoryFieldArtifactId = GetDocumentFieldArtifactId("Job History");
			int documentDestinationCaseFieldArtifactId = GetDocumentFieldArtifactId("Relativity Destination Case");
			ArtifactDTO[] documentArtifacts = _documentRepository.RetrieveDocumentsAsync(documentArtifactIds, new HashSet<int>() { documentDestinationCaseFieldArtifactId, documentJobHistoryFieldArtifactId }).ConfigureAwait(false).GetAwaiter().GetResult();

			foreach (ArtifactDTO artifact in documentArtifacts)
			{
				StringAssert.Contains(expectedDestinationCase, artifact.Fields[0].Value.ToString());
				StringAssert.Contains(expectedJobHistory, artifact.Fields[1].Value.ToString());
			}
		}
	}
}
