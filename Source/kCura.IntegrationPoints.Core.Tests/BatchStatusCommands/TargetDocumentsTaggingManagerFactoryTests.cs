using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Logging;
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
        private ImportSettings _importSettings;
        private IDataSynchronizer _dataSynchronizer;
        private IDiagnosticLog _diagnosticLog;
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
            _importSettings = new ImportSettings(new DestinationConfiguration());
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
                _importSettings,
                _JOBHISTORY_ARTIFACT_ID,
                _UNIQUE_JOBID,
                _diagnosticLog
            );

            // ASSERT
            Assert.AreEqual(ImportOverwriteModeEnum.OverlayOnly, _importSettings.DestinationConfiguration.ImportOverwriteMode);
            Assert.AreEqual(ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE, _importSettings.DestinationConfiguration.FieldOverlayBehavior);
            Assert.IsFalse(_importSettings.DestinationConfiguration.CopyFilesToDocumentRepository);
            Assert.IsNull(_importSettings.FileNameColumn);
            Assert.IsNull(_importSettings.NativeFilePathSourceFieldName);
            Assert.IsNull(_importSettings.FolderPathSourceFieldName);
            Assert.That(_importSettings.DestinationConfiguration.Provider, Is.Null.Or.Empty);
            Assert.IsFalse(_importSettings.DestinationConfiguration.ImportNativeFile);
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
            _synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _importSettings.DestinationConfiguration).Returns(_dataSynchronizer);
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
                _importSettings,
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
