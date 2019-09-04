using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Managers
{
	[TestFixture]
	public class TargetDocumentsTaggingManagerTests : RelativityProviderTemplate
	{
		private IRepositoryFactory _repositoryFactory;
		private IDocumentRepository _documentRepository;
		private ITagsCreator _tagsCreator;
		private TagSavedSearchManager _tagSavedSearchManager;
		private ISynchronizerFactory _synchronizerFactory;
		private IJobHistoryService _jobHistoryService;
		private IFieldQueryRepository _fieldQueryRepository;
		private FieldMap[] _fieldMaps;
		private ISerializer _serializer;
		private IHelper _helper;
		private const int _ADMIN_USER_ID = 9;
		private const string _RELATIVITY_SOURCE_CASE = "Relativity Source Case";
		private const string _RELATIVITY_SOURCE_JOB = "Relativity Source Job";

		public TargetDocumentsTaggingManagerTests() : base(
			sourceWorkspaceName: "TargetDocumentsTaggingManagerSource",
			targetWorkspaceName: null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_serializer = Container.Resolve<ISerializer>();
			_helper = Container.Resolve<IHelper>();
			var managerFactory = new ManagerFactory(_helper);
			_tagsCreator = managerFactory.CreateTagsCreator();
			_tagSavedSearchManager = new TagSavedSearchManager(
				new TagSavedSearch(_repositoryFactory, new MultiObjectSavedSearchCondition(), _helper),
				new TagSavedSearchFolder(_repositoryFactory, _helper));
			_synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactID);
			_fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactID);
			_fieldMaps = GetDefaultFieldMap();
		}

		[Test]
		[SmokeTest]
		[IdentifiedTestCase("50d501dd-cc30-4882-8149-75bb0e8752f8", 499, "UnderBatch")]
		[IdentifiedTestCase("360e1c73-0bf2-4066-ba2d-01a9f81f2888", 500, "EqualBatch")]
		[IdentifiedTestCase("8b2f6597-11e9-4a23-b0a2-a8fea31b3d63", 502, "OverBatch")]
		public async Task TargetWorkspaceDocumentTagging_GoldFlow(int numberOfDocuments, string documentIdentifier)
		{
			//Act
			string expectedRelativitySourceCase = $"TargetDocumentsTaggingManagerSource - {SourceWorkspaceArtifactID}";
			DataTable dataTable = Import.GetImportTable(documentIdentifier, numberOfDocuments);
			Import.ImportNewDocuments(SourceWorkspaceArtifactID, dataTable);
			int[] documentArtifactIDs = await _documentRepository
				.RetrieveDocumentByIdentifierPrefixAsync(Fields.GetDocumentIdentifierFieldName(_fieldQueryRepository), documentIdentifier)
				.ConfigureAwait(false);

			string serializedSourceConfig = CreateDefaultSourceConfig();
			Data.IntegrationPoint integrationPoint = await CreateAndGetIntegrationPoint(serializedSourceConfig).ConfigureAwait(false);
			JobHistory jobHistory = _jobHistoryService.GetOrCreateScheduledRunHistoryRdo(integrationPoint, Guid.NewGuid(), DateTime.Now);

			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(serializedSourceConfig);
			string destinationConfig = AppendWebAPIPathToImportSettings(integrationPoint.DestinationConfiguration);
			var targetDocumentsTaggingManagerFactory = new TargetDocumentsTaggingManagerFactory(
				_repositoryFactory,
				_tagsCreator,
				_tagSavedSearchManager,
				_documentRepository,
				_synchronizerFactory,
				_helper,
				_serializer,
				_fieldMaps,
				sourceConfiguration,
				destinationConfig,
				jobHistory.ArtifactId,
				jobHistory.BatchInstance);
			IConsumeScratchTableBatchStatus targetDocumentsTaggingManager = targetDocumentsTaggingManagerFactory.BuildDocumentsTagger();
			targetDocumentsTaggingManager.ScratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIDs);

			//Act
			Job job = new JobBuilder().WithJobId(1)
				.WithWorkspaceId(SourceWorkspaceArtifactID)
				.WithRelatedObjectArtifactId(integrationPoint.ArtifactId)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();
			targetDocumentsTaggingManager.OnJobStart(job);
			targetDocumentsTaggingManager.OnJobComplete(job);

			//Assert
			await VerifyRelativitySourceJobAndSourceCase(
					documentArtifactIDs,
					jobHistory.Name,
					expectedRelativitySourceCase)
				.ConfigureAwait(false);
		}

		private Task<Data.IntegrationPoint> CreateAndGetIntegrationPoint(string serializedSourceConfig)
		{
			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = serializedSourceConfig,
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
			IntegrationPointModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			return IntegrationPointRepository.ReadAsync(integrationModelCreated.ArtifactID);
		}

		private async Task VerifyRelativitySourceJobAndSourceCase(int[] documentArtifactIds, string expectedSourceJob, string expectedSourceCase)
		{
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactID);

			var setOfArtifactIDField = new HashSet<string> { "ArtifactID" };

			ArtifactDTO relativitySourceCaseField = _fieldQueryRepository.RetrieveField(
				(int)Relativity.Client.ArtifactType.Document,
				_RELATIVITY_SOURCE_CASE,
				FieldTypes.MultipleObject,
				setOfArtifactIDField);
			int? relativitySourceCaseFieldArtifactID = relativitySourceCaseField?.ArtifactId;
			ArtifactDTO relativitySourceJobField = _fieldQueryRepository.RetrieveField(
				(int)Relativity.Client.ArtifactType.Document,
				_RELATIVITY_SOURCE_JOB,
				FieldTypes.MultipleObject,
				setOfArtifactIDField);
			int? relativitySourceJobArtifactID = relativitySourceJobField?.ArtifactId;

			ArtifactDTO[] documentArtifacts = await _documentRepository
				.RetrieveDocumentsAsync(documentArtifactIds,
					new HashSet<int>
					{
						relativitySourceJobArtifactID.GetValueOrDefault(),
						relativitySourceCaseFieldArtifactID.GetValueOrDefault()
					})
				.ConfigureAwait(false);

			foreach (ArtifactDTO artifact in documentArtifacts)
			{
				if (artifact.Fields[0].Value == null || !GetFirstMultiobjectFieldValueName(artifact.Fields[0]).Contains(expectedSourceJob))
				{
					Assert.Fail($"Failed to correctly tag Document field 'Relativity Source Job'. Expected value: {expectedSourceJob}. Actual: {artifact.Fields[1].Value}.");
				}

				if (artifact.Fields[1].Value == null || !GetFirstMultiobjectFieldValueName(artifact.Fields[1]).Contains(expectedSourceCase))
				{
					Assert.Fail($"Failed to correctly tag Document field 'Relativity Source Case'. Expected value: {expectedSourceCase}. Actual: {artifact.Fields[0].Value}.");
				}
			}
		}

		private string GetFirstMultiobjectFieldValueName(ArtifactFieldDTO fields)
		{
			var fieldsValues = fields.Value as IEnumerable<RelativityObjectValue>;
			return fieldsValues.First().Name;
		}

		#region "Registration helpers"

		private string AppendWebAPIPathToImportSettings(string importSettings)
		{
			ImportSettings options = _serializer.Deserialize<ImportSettings>(importSettings);
			options.WebServiceURL = SharedVariables.RelativityWebApiUrl;
			return _serializer.Serialize(options);
		}

		#endregion
	}
}