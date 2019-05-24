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
			: base(
				sourceWorkspaceName: nameof(SourceDocumentsTaggerTests),
				targetWorkspaceName: null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			IRepositoryFactory repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IDestinationWorkspaceRepository destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(SourceWorkspaceArtifactID);
			_destinationWorkspaceDto = destinationWorkspaceRepository.Create(
				SourceWorkspaceArtifactID,
				targetWorkspaceName: "DestinationWorkspaceRepositoryTests",
				federatedInstanceArtifactId: -1,
				federatedInstanceName: "This Instance");

			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_scratchTableRepository = repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactID, "Documents2Tag", "LikeASir");
			_documentRepository = repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactID);
			_fieldQueryRepository = repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactID);

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
			Import.ImportNewDocuments(SourceWorkspaceArtifactID, dataTable);
			int[] documentArtifactIDs = await _documentRepository
				.RetrieveDocumentByIdentifierPrefixAsync(Fields.GetDocumentIdentifierFieldName(_fieldQueryRepository), "DocsToTag")
				.ConfigureAwait(false);
			_scratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIDs);
			Data.IntegrationPoint integrationPoint = await CreateAndGetDefaultIntegrationPointModel().ConfigureAwait(false);

			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint,
				batchInstance,
				JobTypeChoices.JobHistoryRun,
				startTimeUtc: DateTime.UtcNow);

			// act
			await _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
				_scratchTableRepository,
				_destinationWorkspaceDto.ArtifactId,
				jobHistory.ArtifactId)
				.ConfigureAwait(false);

			// assert
			await VerifyDocumentTaggingAsync(documentArtifactIDs, jobHistory.Name).ConfigureAwait(false);
		}

		[Test]
		public void ShouldThrowExceptionWhenArtifactIdIsInvalid()
		{
			// act
			Func<Task> tagDocumentsAction = () =>
				_sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(_scratchTableRepository, -1, -1);

			// assert
			string expectedMessage = "Tagging Documents with DestinationWorkspace and JobHistory object failed - Mass Edit failure.";
			tagDocumentsAction.ShouldThrow<Exception>().WithMessage(expectedMessage);
		}

		private async Task VerifyDocumentTaggingAsync(int[] documentArtifactIDs, string expectedJobHistory)
		{
			string expectedDestinationCase = $"DestinationWorkspaceRepositoryTests - { SourceWorkspaceArtifactID }";
			HashSet<int> tagsFieldsArtifactsIDs = GetTagFieldsArtifactIDs();
			ArtifactDTO[] documentArtifacts = await _documentRepository
				.RetrieveDocumentsAsync(documentArtifactIDs, tagsFieldsArtifactsIDs)
				.ConfigureAwait(false);

			foreach (ArtifactDTO artifact in documentArtifacts)
			{
				string destinationCaseValue = GetFirstMultiobjectFieldValueName(artifact.Fields[0]);
				string jobHistoryValue = GetFirstMultiobjectFieldValueName(artifact.Fields[1]);
				destinationCaseValue.Should().Contain(expectedDestinationCase);
				jobHistoryValue.Should().Contain(expectedJobHistory);
			}
		}

		private async Task<Data.IntegrationPoint> CreateAndGetDefaultIntegrationPointModel()
		{
			IntegrationPointModel integrationPointModel = BuildIntegrationPointModel();
			IntegrationPointModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationPointModel);
			Data.IntegrationPoint integrationPoint = await IntegrationPointRepository
				.ReadAsync(integrationModelCreated.ArtifactID)
				.ConfigureAwait(false);
			return integrationPoint;
		}

		private IntegrationPointModel BuildIntegrationPointModel()
		{
			return new IntegrationPointModel
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
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}

		private HashSet<int> GetTagFieldsArtifactIDs()
		{
			var fieldsToRetrieve = new HashSet<string> { Constants.Fields.ArtifactId };

			int? documentJobHistoryFieldArtifactID = _fieldQueryRepository
				.RetrieveField(
					(int)Relativity.Client.ArtifactType.Document,
					DocumentFields.JobHistory,
					FieldTypes.MultipleObject,
					fieldsToRetrieve)
				?.ArtifactId;

			int? documentDestinationCaseFieldArtifactID = _fieldQueryRepository
				.RetrieveField(
					(int)Relativity.Client.ArtifactType.Document,
					DocumentFields.RelativityDestinationCase,
					FieldTypes.MultipleObject,
					fieldsToRetrieve)
				?.ArtifactId;

			var tagsFieldsArtifactsIDs = new HashSet<int>
			{
				documentDestinationCaseFieldArtifactID.GetValueOrDefault(),
				documentJobHistoryFieldArtifactID.GetValueOrDefault()
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
