using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Tagging
{
	[TestFixture]
	public class SourceDocumentsTaggerTests : RelativityProviderTemplate
	{
		private ISourceDocumentsTagger _sut;

		private DestinationWorkspace _destinationWorkspaceDto;
		private IJobHistoryService _jobHistoryService;
		private IScratchTableRepository _scratchTableRepository;
		private IDocumentRepository _documentRepository;
		private IFieldQueryRepository _fieldQueryRepository;

		private const int _SOURCE_WORKSPACE_TAGGING_BATCH_SIZE = 4;

		public SourceDocumentsTaggerTests()
			: base(nameof(SourceDocumentsTaggerTests), null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IDestinationWorkspaceRepository destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(SourceWorkspaceArtifactId);
			_destinationWorkspaceDto = destinationWorkspaceRepository.Create(SourceWorkspaceArtifactId, "DestinationWorkspaceRepositoryTests", -1, "This Instance");

			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_scratchTableRepository = repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactId, "Documents2Tag", "LikeASir");
			_documentRepository = repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
			_fieldQueryRepository = repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactId);

			var configMock = new Mock<IConfig>();
			configMock.Setup(x => x.SourceWorkspaceTaggerBatchSize).Returns(_SOURCE_WORKSPACE_TAGGING_BATCH_SIZE);
			var loggerMock = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};

			IDocumentRepository documentRepository = Container.Resolve<IDocumentRepository>();
			_sut = new SourceDocumentsTagger(documentRepository, configMock.Object, loggerMock.Object);
		}

		public override void SuiteTeardown()
		{
			_scratchTableRepository.Dispose();
			base.SuiteTeardown();
		}

		[Test]
		public async Task ShouldTagDocuments()
		{
			// arrange
			const int numberOfDocuments = 10;
			DataTable dataTable = Import.GetImportTable("DocsToTag", numberOfDocuments);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, dataTable);
			int[] documentArtifactIds = await _documentRepository
				.RetrieveDocumentByIdentifierPrefixAsync(Fields.GetDocumentIdentifierFieldName(_fieldQueryRepository), "DocsToTag")
				.ConfigureAwait(false);
			_scratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIds);

			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
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
			Data.IntegrationPoint integrationPoint = await IntegrationPointRepository
				.ReadAsync(integrationModelCreated.ArtifactID)
				.ConfigureAwait(false);

			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint,
				batchInstance,
				JobTypeChoices.JobHistoryRun,
				startTimeUtc: DateTime.UtcNow);

			// act
			await _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
				_scratchTableRepository,
				_destinationWorkspaceDto.ArtifactId,
				jobHistory.ArtifactId);

			// assert
			await VerifyDocumentTaggingAsync(documentArtifactIds, jobHistory.Name).ConfigureAwait(false);
		}

		[Test]
		public void ShouldThrowExceptionWhenArtifactIdIsInvalid()
		{
			//Act & Assert
			Assert.Throws<Exception>(
				() => _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(_scratchTableRepository, -1, -1),
				"Tagging Documents with DestinationWorkspace and JobHistory object failed - Mass Edit failure.");
		}

		private async Task VerifyDocumentTaggingAsync(int[] documentArtifactIds, string expectedJobHistory)
		{
			string expectedDestinationCase = $"DestinationWorkspaceRepositoryTests - { SourceWorkspaceArtifactId }";
			HashSet<int> tagsFieldsArtifactsIDs = GetTagFieldsArtifactIDs();
			ArtifactDTO[] documentArtifacts = await _documentRepository
				.RetrieveDocumentsAsync(documentArtifactIds, tagsFieldsArtifactsIDs)
				.ConfigureAwait(false);

			foreach (ArtifactDTO artifact in documentArtifacts)
			{
				string destinationCaseValue = GetFirstMultiobjectFieldValueName(artifact.Fields[0]);
				string jobHistoryValue = GetFirstMultiobjectFieldValueName(artifact.Fields[1]);
				destinationCaseValue.Should().Contain(expectedDestinationCase);
				jobHistoryValue.Should().Contain(expectedJobHistory);
			}
		}

		private HashSet<int> GetTagFieldsArtifactIDs()
		{
			var fieldsToRetrieve = new HashSet<string> { Constants.Fields.ArtifactId };

			int? documentJobHistoryFieldArtifactId = _fieldQueryRepository
				.RetrieveField(
					(int)Relativity.Client.ArtifactType.Document,
					DocumentFields.JobHistory,
					FieldTypes.MultipleObject,
					fieldsToRetrieve)
				?.ArtifactId;

			int? documentDestinationCaseFieldArtifactId = _fieldQueryRepository
				.RetrieveField(
					(int)Relativity.Client.ArtifactType.Document,
					DocumentFields.RelativityDestinationCase,
					FieldTypes.MultipleObject,
					fieldsToRetrieve)
				?.ArtifactId;

			var tagsFieldsArtifactsIDs = new HashSet<int>
			{
				documentDestinationCaseFieldArtifactId.GetValueOrDefault(),
				documentJobHistoryFieldArtifactId.GetValueOrDefault()
			};
			return tagsFieldsArtifactsIDs;
		}

		private string GetFirstMultiobjectFieldValueName(ArtifactFieldDTO fields)
		{
			var fieldsValues = fields.Value as IEnumerable<RelativityObjectValue>;
			return fieldsValues.First().Name;
		}
	}
}
