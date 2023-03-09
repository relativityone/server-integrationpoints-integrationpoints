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
        private ImportSettings _destinationConfiguration;
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
            _destinationConfiguration = new ImportSettings();
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
                _destinationConfiguration,
                _JOBHISTORY_ARTIFACT_ID,
                _UNIQUE_JOBID,
                _diagnosticLog
            );

            // ASSERT
            Assert.AreEqual(ImportOverwriteModeEnum.OverlayOnly, _destinationConfiguration.ImportOverwriteMode);
            Assert.AreEqual(ImportSettings.FIELDOVERLAYBEHAVIOR_MERGE, _destinationConfiguration.FieldOverlayBehavior);
            Assert.IsFalse(_destinationConfiguration.CopyFilesToDocumentRepository);
            Assert.IsNull(_destinationConfiguration.FileNameColumn);
            Assert.IsNull(_destinationConfiguration.NativeFilePathSourceFieldName);
            Assert.IsNull(_destinationConfiguration.FolderPathSourceFieldName);
            Assert.That(_destinationConfiguration.Provider, Is.Null.Or.Empty);
            Assert.IsFalse(_destinationConfiguration.ImportNativeFile);
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
            _synchronizerFactory.CreateSynchronizer(Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID, _destinationConfiguration).Returns(_dataSynchronizer);
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
                _destinationConfiguration,
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
