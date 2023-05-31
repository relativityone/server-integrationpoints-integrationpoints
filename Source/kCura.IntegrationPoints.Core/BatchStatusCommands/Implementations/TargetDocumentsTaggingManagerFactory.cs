using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
    public class TargetDocumentsTaggingManagerFactory
    {
        private readonly ImportSettings _destinationConfig;
        private readonly IDocumentRepository _documentRepository;
        private readonly FieldMap[] _fields;
        private readonly IHelper _helper;
        private readonly int _jobHistoryArtifactId;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ISerializer _serializer;
        private readonly SourceConfiguration _sourceConfig;
        private readonly ITagsCreator _tagsCreator;
        private readonly ISynchronizerFactory _synchronizerFactory;
        private readonly ITagSavedSearchManager _tagSavedSearchManager;
        private readonly string _uniqueJobId;
        private readonly IDiagnosticLog _diagnosticLog;

        public TargetDocumentsTaggingManagerFactory(
            IRepositoryFactory repositoryFactory,
            ITagsCreator tagsCreator,
            ITagSavedSearchManager tagSavedSearchManager,
            IDocumentRepository documentRepository,
            ISynchronizerFactory synchronizerFactory,
            IHelper helper,
            ISerializer serializer,
            FieldMap[] fields,
            SourceConfiguration sourceConfig,
            ImportSettings destinationConfig,
            int jobHistoryArtifactId,
            string uniqueJobId,
            IDiagnosticLog diagnosticLog)
        {
            _repositoryFactory = repositoryFactory;
            _tagSavedSearchManager = tagSavedSearchManager;
            _tagsCreator = tagsCreator;
            _documentRepository = documentRepository;
            _synchronizerFactory = synchronizerFactory;
            _helper = helper;
            _serializer = serializer;
            _fields = fields;
            _sourceConfig = sourceConfig;
            _jobHistoryArtifactId = jobHistoryArtifactId;
            _uniqueJobId = uniqueJobId;
            _diagnosticLog = diagnosticLog;
            _destinationConfig = destinationConfig;
            AdjustDestinationConfig(_destinationConfig);
        }

        public IConsumeScratchTableBatchStatus BuildDocumentsTagger()
        {
            Tagger tagger = CreateTagger();

            IConsumeScratchTableBatchStatus taggingManager = new TargetDocumentsTaggingManager(
                _repositoryFactory,
                _tagsCreator,
                tagger,
                _tagSavedSearchManager,
                _helper,
                _destinationConfig,
                _sourceConfig.SourceWorkspaceArtifactId,
                _sourceConfig.TargetWorkspaceArtifactId,
                _sourceConfig.FederatedInstanceArtifactId,
                _jobHistoryArtifactId,
                _uniqueJobId);

            return taggingManager;
        }

        private Tagger CreateTagger()
        {
            IDataSynchronizer synchronizer = _synchronizerFactory.CreateSynchronizer(
                Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID,
                _destinationConfig.DestinationConfiguration);
            var tagsSynchronizer = new TagsSynchronizer(_helper, synchronizer, _serializer);

            var tagger = new Tagger(
                _documentRepository,
                tagsSynchronizer,
                _helper,
                _fields,
                _destinationConfig,
                _diagnosticLog);
            return tagger;
        }

        private void AdjustDestinationConfig(ImportSettings importSettings)
        {
            // specify settings to tag
            importSettings.DestinationConfiguration.ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly;
            importSettings.DestinationConfiguration.FieldOverlayBehavior = ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE;
            importSettings.DestinationConfiguration.CopyFilesToDocumentRepository = false;
            importSettings.FileNameColumn = null;
            importSettings.NativeFilePathSourceFieldName = null;
            importSettings.FolderPathSourceFieldName = null;
            importSettings.DestinationConfiguration.Provider = string.Empty;
            importSettings.DestinationConfiguration.ImportNativeFile = false;
        }
    }
}
