using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Tagging
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class SourceDocumentsTaggerTests : RelativityProviderTemplate
	{
		private ISourceDocumentsTagger _sut;

		private DestinationWorkspace _destinationWorkspaceDto;
		private IJobHistoryService _jobHistoryService;
		private IScratchTableRepository _scratchTableRepository;
		private IDocumentRepository _documentRepository;
		private IFieldQueryRepository _fieldQueryRepository;

		private const int _SOURCE_WORKSPACE_TAGGING_BATCH_SIZE = 10000;

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
			configMock.Setup(x => x.MassUpdateBatchSize).Returns(_SOURCE_WORKSPACE_TAGGING_BATCH_SIZE);
			var loggerMock = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};

			var massUpdateHelper = new MassUpdateHelper(configMock.Object, loggerMock.Object);

			IDocumentRepository documentRepository = Container.Resolve<IDocumentRepository>();
			_sut = new SourceDocumentsTagger(documentRepository, loggerMock.Object, massUpdateHelper);
		}

		[SetUp]
		public void SetUp()
		{
			_scratchTableRepository.DeleteTable();
		}

		public override void SuiteTeardown()
		{
			_scratchTableRepository.Dispose();
			base.SuiteTeardown();
		}

		[IdentifiedTestCase("528f7d6d-6b29-4b86-a3a8-80e23333312b", 10)]
		[IdentifiedTestCase("7BE519E5-04D9-4DEC-A286-747389254856", 12384)]
		[IdentifiedTestCase("0780AAB1-366D-4E41-8C9F-D0CDC497F28B", 500000, Category = TestCategories.STRESS_TEST, Explicit = true)]
		public async Task ShouldTagDocuments(int numberOfDocuments)
		{
			// arrange
			string documentsPrefix = Guid.NewGuid().ToString();
			Task<int[]> importDocumentsToWorkspaceAndScratchTableTask = ImportDocumentsToWorkspaceAndScratchTable(documentsPrefix, numberOfDocuments);
			Task<Data.IntegrationPoint> createDefaultIntegrationPointTask = CreateAndGetDefaultIntegrationPointModel();

			int[] documentArtifactIDs = await importDocumentsToWorkspaceAndScratchTableTask.ConfigureAwait(false);
			Data.IntegrationPoint integrationPoint = await createDefaultIntegrationPointTask.ConfigureAwait(false);

			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = _jobHistoryService.CreateRdo(
				integrationPoint,
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

		[IdentifiedTest("dbe75edd-3e9e-4b41-93ea-ac5fe491e085")]
		public async Task ShouldThrowExceptionWhenTagsArtifactsIDsAreInvalid()
		{
			// arrange
			const int numberOfDocuments = 1;
			string documentsPrefix = Guid.NewGuid().ToString();
			await ImportDocumentsToWorkspaceAndScratchTable(documentsPrefix, numberOfDocuments)
				.ConfigureAwait(false);

			// act
			Func<Task> tagDocumentsAction = () =>
				_sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(_scratchTableRepository, -1, -1);

			// assert
			string expectedMessage = "Mass edit of artifacts failed - Object Manager failure.";
			tagDocumentsAction.ShouldThrow<IntegrationPointsException>().WithMessage(expectedMessage);
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

		private async Task<int[]> ImportDocumentsToWorkspaceAndScratchTable(string documentsPrefix, int numberOfDocuments)
		{
			DataTable dataTable = Import.GetImportTable(documentsPrefix, numberOfDocuments);
			Import.ImportNewDocuments(SourceWorkspaceArtifactID, dataTable);

			string documentIdentifierFieldName = Fields.GetDocumentIdentifierFieldName(_fieldQueryRepository);
			int[] documentArtifactIDs = await _documentRepository
				.RetrieveDocumentByIdentifierPrefixAsync(documentIdentifierFieldName, documentsPrefix)
				.ConfigureAwait(false);
			_scratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIDs);

			return documentArtifactIDs;
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
					(int)ArtifactType.Document,
					DocumentFields.JobHistory,
					FieldTypes.MultipleObject,
					fieldsToRetrieve)
				?.ArtifactId;

			int? documentDestinationCaseFieldArtifactID = _fieldQueryRepository
				.RetrieveField(
					(int)ArtifactType.Document,
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
