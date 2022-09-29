using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tests.BatchStatusCommands
{
    [TestFixture, Category("Unit")]
    public class TargetDocumentsTaggingManagerFactoryTests : TestBase
    {
        private IRepositoryFactory _repositoryFactory;
        private ITagsCreator _tagsCreator;
        private ITagSavedSearchManager _tagSavedSearchManager;
        private IDocumentRepository _documentRepository;
        private ISynchronizerFactory _synchronizerFactory;
        private IHelper _helper;
        private ISerializer _serializer;
        private FieldMap[] _fields;
        private TargetDocumentsTaggingManagerFactory _instance;
        private ImportSettings _settings;
        private IDataSynchronizer _dataSynchronizer;
        private IDiagnosticLog _diagnosticLog;

        private const string _DEST_CONFIG = "destination config";
        private const string _NEW_DEST_CONFIG = "new destination config";
        private const int _JOBHISTORY_ARTIFACT_ID = 321;
        private const string _UNIQUE_JOBID = "very unique";

        private readonly SourceConfiguration _sourceConfiguration = new SourceConfiguration();

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _tagsCreator = Substitute.For<ITagsCreator>();
            _tagSavedSearchManager = Substitute.For<ITagSavedSearchManager>();
            _documentRepository = Substitute.For<IDocumentRepository>();
            _synchronizerFactory = Substitute.For<ISynchronizerFactory>();
            _serializer = Substitute.For<ISerializer>();
            _dataSynchronizer = Substitute.For<IDataSynchronizer>();
            _helper = Substitute.For<IHelper>();
            _diagnosticLog = Substitute.For<IDiagnosticLog>();
            _fields = new FieldMap[0];
            _settings = new ImportSettings();
            _serializer.Deserialize<ImportSettings>(_DEST_CONFIG).Returns(_settings);
            _serializer.Serialize(_settings).Returns(_NEW_DEST_CONFIG);
        }

        [Test]
        public void Init_OverrideImportSettings()
        {
            // ARRANGE & ACT
            _instance = new TargetDocumentsTaggingManagerFactory
            (
                _repositoryFactory,
                _tagsCreator,
                _tagSavedSearchManager,
                _documentRepository,
                _synchronizerFactory,
                _helper,
                _serializer,
                _fields,
                _sourceConfiguration,
                _DEST_CONFIG,
                _JOBHISTORY_ARTIFACT_ID,
                _UNIQUE_JOBID,
                _diagnosticLog
            );

            // ASSERT
            Assert.AreEqual(ImportOverwriteModeEnum.OverlayOnly, _settings.ImportOverwriteMode);
            Assert.AreEqual(ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE, _settings.FieldOverlayBehavior);
            Assert.IsFalse(_settings.CopyFilesToDocumentRepository);
            Assert.IsNull(_settings.FileNameColumn);
            Assert.IsNull(_settings.NativeFilePathSourceFieldName);
            Assert.IsNull(_settings.FolderPathSourceFieldName);
            Assert.That(_settings.Provider, Is.Null.Or.Empty);
            Assert.IsFalse(_settings.ImportNativeFile);
        }

        [Test]
        public void BuildDocumentsTagger_GoldFlow()
        {
            // ARRANGE
            SourceConfiguration exportSettings = new SourceConfiguration()
            {
                SourceWorkspaceArtifactId = 1,
                TargetWorkspaceArtifactId = 2
            };
            _synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _NEW_DEST_CONFIG).Returns(_dataSynchronizer);
            _instance = new TargetDocumentsTaggingManagerFactory
            (
                _repositoryFactory,
                _tagsCreator,
                _tagSavedSearchManager,
                _documentRepository,
                _synchronizerFactory,
                _helper,
                _serializer,
                _fields,
                _sourceConfiguration,
                _DEST_CONFIG,
                _JOBHISTORY_ARTIFACT_ID,
                _UNIQUE_JOBID,
                _diagnosticLog
            );

            // ACT
            IConsumeScratchTableBatchStatus tagger = _instance.BuildDocumentsTagger();

            // ASSERT
            Assert.IsNotNull(tagger);
            Assert.IsTrue(tagger is TargetDocumentsTaggingManager);
        }
    }
}