using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class TargetDocumentsTaggingManagerFactory
	{
		private readonly string _destinationConfig;
		private readonly IDocumentRepository _documentRepository;
		private readonly FieldMap[] _fields;
		private readonly IHelper _helper;
		private readonly int _jobHistoryArtifactId;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ISerializer _serializer;
		private readonly string _sourceConfig;
		private readonly ISourceJobManager _sourceJobManager;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly ISynchronizerFactory _synchronizerFactory;
		private readonly ITagSavedSearchManager _tagSavedSearchManager;
		private readonly string _uniqueJobId;

		public TargetDocumentsTaggingManagerFactory(
			IRepositoryFactory repositoryFactory,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			ITagSavedSearchManager tagSavedSearchManager,
			IDocumentRepository documentRepository,
			ISynchronizerFactory synchronizerFactory,
			IHelper helper,
			ISerializer serializer,
			FieldMap[] fields,
			string sourceConfig,
			string destinationConfig,
			int jobHistoryArtifactId,
			string uniqueJobId)
		{
			_repositoryFactory = repositoryFactory;
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_sourceJobManager = sourceJobManager;
			_tagSavedSearchManager = tagSavedSearchManager;
			_documentRepository = documentRepository;
			_synchronizerFactory = synchronizerFactory;
			_helper = helper;
			_serializer = serializer;
			_fields = fields;
			_sourceConfig = sourceConfig;
			_jobHistoryArtifactId = jobHistoryArtifactId;
			_uniqueJobId = uniqueJobId;

			// specify settings to tag
			ImportSettings importSettings = serializer.Deserialize<ImportSettings>(destinationConfig);
			importSettings.ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly;
			importSettings.FieldOverlayBehavior = ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE;
			importSettings.CopyFilesToDocumentRepository = false;
			importSettings.FileNameColumn = null;
			importSettings.NativeFilePathSourceFieldName = null;
			importSettings.FolderPathSourceFieldName = null;
			importSettings.Provider = string.Empty;
			importSettings.ImportNativeFile = false;
			_destinationConfig = _serializer.Serialize(importSettings);
		}

		public IConsumeScratchTableBatchStatus BuildDocumentsTagger()
		{
			var settings = _serializer.Deserialize<SourceConfiguration>(_sourceConfig);
			var importSettings = _serializer.Deserialize<ImportSettings>(_destinationConfig);
			var synchronizer = _synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _destinationConfig);

			var tagger = new Tagger(_documentRepository, synchronizer, _helper, _fields, _destinationConfig, settings.SourceWorkspaceArtifactId);
			var tagsCreator = new TagsCreator(_sourceJobManager, _sourceWorkspaceManager, _helper);

			IConsumeScratchTableBatchStatus taggingManager = new TargetDocumentsTaggingManager(
				_repositoryFactory,
				tagsCreator,
				tagger,
				_tagSavedSearchManager,
				_helper,
				importSettings,
				settings.SourceWorkspaceArtifactId,
				settings.TargetWorkspaceArtifactId,
				settings.FederatedInstanceArtifactId,
				_jobHistoryArtifactId,
				_uniqueJobId);

			return taggingManager;
		}
	}
}