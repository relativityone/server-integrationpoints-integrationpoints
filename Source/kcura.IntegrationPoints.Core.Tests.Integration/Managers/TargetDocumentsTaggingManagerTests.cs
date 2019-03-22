using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
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

		public TargetDocumentsTaggingManagerTests() : base("TargetDocumentsTaggingManagerSource", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_serializer = Container.Resolve<ISerializer>();
			_helper = Container.Resolve<IHelper>();
		    var serviceManagerProvider = Container.Resolve<IServiceManagerProvider>();
            var managerFactory = new ManagerFactory(_helper, serviceManagerProvider);
			_tagsCreator = managerFactory.CreateTagsCreator(new ContextContainer(_helper));
			_tagSavedSearchManager = new TagSavedSearchManager(new TagSavedSearch(_repositoryFactory, new MultiObjectSavedSearchCondition(), _helper), new TagSavedSearchFolder(_repositoryFactory, _helper));
			_synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
			_fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactId);
			_fieldMaps = GetDefaultFieldMap();
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.SMOKE_TEST)]
		[TestCase(499, "UnderBatch")]
		[TestCase(500, "EqualBatch")]
		[TestCase(502, "OverBatch")]
		public void TargetWorkspaceDocumentTagging_GoldFlow(int numberOfDocuments, string documentIdentifier)
		{
			//Act
			string expectedRelativitySourceCase = $"TargetDocumentsTaggingManagerSource - {SourceWorkspaceArtifactId}";
			DataTable dataTable = Import.GetImportTable(documentIdentifier, numberOfDocuments);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, dataTable);
			int[] documentArtifactIds = _documentRepository.RetrieveDocumentByIdentifierPrefixAsync(Fields.GetDocumentIdentifierFieldName(_fieldQueryRepository), documentIdentifier).ConfigureAwait(false).GetAwaiter().GetResult();

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
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
			Data.IntegrationPoint integrationPoint = IntegrationPointRepository.ReadAsync(integrationModelCreated.ArtifactID).GetAwaiter().GetResult();
			JobHistory jobHistory = _jobHistoryService.GetOrCreateScheduledRunHistoryRdo(integrationPoint, Guid.NewGuid(), DateTime.Now);

			string destinationConfig = AppendWebAPIPathToImportSettings(integrationModelCreated.Destination);
			TargetDocumentsTaggingManagerFactory targetDocumentsTaggingManagerFactory = new TargetDocumentsTaggingManagerFactory(_repositoryFactory, _tagsCreator, _tagSavedSearchManager, _documentRepository, _synchronizerFactory, _helper, _serializer, _fieldMaps, integrationModelCreated.SourceConfiguration, destinationConfig, jobHistory.ArtifactId, jobHistory.BatchInstance);
			IConsumeScratchTableBatchStatus targetDocumentsTaggingManager = targetDocumentsTaggingManagerFactory.BuildDocumentsTagger();
			targetDocumentsTaggingManager.ScratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIds);

			//Act
			Job job = new JobBuilder().WithJobId(1)
				.WithWorkspaceId(SourceWorkspaceArtifactId)
				.WithRelatedObjectArtifactId(integrationModelCreated.ArtifactID)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();
			targetDocumentsTaggingManager.OnJobStart(job);
			targetDocumentsTaggingManager.OnJobComplete(job);

			//Assert
			VerifyRelativitySourceJobAndSourceCase(documentArtifactIds, jobHistory.Name, expectedRelativitySourceCase);
		}

		private void VerifyRelativitySourceJobAndSourceCase(int[] documentArtifactIds, string expectedSourceJob, string expectedSourceCase)
		{
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);

			ArtifactDTO relativitySourceCaseField = _fieldQueryRepository.RetrieveField((int)Relativity.Client.ArtifactType.Document, _RELATIVITY_SOURCE_CASE, FieldTypes.MultipleObject, new HashSet<string>() { "ArtifactID" });
			int? relativitySourceCaseFieldArtifactId = relativitySourceCaseField?.ArtifactId;
			ArtifactDTO relativitySourceJobField = _fieldQueryRepository.RetrieveField((int)Relativity.Client.ArtifactType.Document, _RELATIVITY_SOURCE_JOB, FieldTypes.MultipleObject, new HashSet<string>() { "ArtifactID" });
			int? relativitySourceJobArtifactId = relativitySourceJobField?.ArtifactId;

			ArtifactDTO[] documentArtifacts =
				_documentRepository.RetrieveDocumentsAsync(documentArtifactIds,
					new HashSet<int>() { relativitySourceJobArtifactId.GetValueOrDefault(), relativitySourceCaseFieldArtifactId.GetValueOrDefault() })
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

			foreach (ArtifactDTO artifact in documentArtifacts)
			{
				if (artifact.Fields[0].Value == null || !GetFirstMultiobjectFieldValueName(artifact.Fields[0]).Contains(expectedSourceJob))
				{
					throw new Exception($"Failed to correctly tag Document field 'Relativity Source Job'. Expected value: {expectedSourceJob}. Actual: {artifact.Fields[1].Value}.");
				}

				if (artifact.Fields[1].Value == null || !GetFirstMultiobjectFieldValueName(artifact.Fields[1]).Contains(expectedSourceCase))
				{
					throw new Exception($"Failed to correctly tag Document field 'Relativity Source Case'. Expected value: {expectedSourceCase}. Actual: {artifact.Fields[0].Value}.");
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
			var options = _serializer.Deserialize<ImportSettings>(importSettings);
			options.WebServiceURL = SharedVariables.RelativityWebApiUrl;
			return _serializer.Serialize(options);
		}

		#endregion
	}
}