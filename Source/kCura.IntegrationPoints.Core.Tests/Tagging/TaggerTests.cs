using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
    [TestFixture]
    [Category("Unit")]
    public class TaggerTests : TestBase
    {
        private Tagger _instance;
        private IDocumentRepository _documentRepository;
        private IDataSynchronizer _dataSynchronizer;
        private ImportSettings _importConfig;

        public override void SetUp()
        {
            _documentRepository = Substitute.For<IDocumentRepository>();
            _dataSynchronizer = Substitute.For<IDataSynchronizer>();
            ILogger<Tagger> logger = Substitute.For<ILogger<Tagger>>();

            FieldMap[] fields =
            {
                new FieldMap
                {
                    DestinationField = new FieldEntry(),
                    SourceField = new FieldEntry()
                },
                new FieldMap
                {
                    DestinationField = new FieldEntry
                    {
                        DisplayName = "destination id",
                        FieldIdentifier = "123456"
                    },
                    FieldMapType = FieldMapTypeEnum.Identifier,
                    SourceField = new FieldEntry
                    {
                        DisplayName = "source id",
                        FieldIdentifier = "789456"
                    }
                }
            };

            _importConfig = new ImportSettings(new DestinationConfiguration());

            _instance = new Tagger(_documentRepository, _dataSynchronizer, fields, _importConfig, logger);
        }

        [Test]
        public void ItShouldImportTaggingFieldsWhenThereAreDocumentsToTag()
        {
            // Arrange
            SourceJobDTO job = new SourceJobDTO
            {
                Name = "whatever"
            };
            SourceWorkspaceDTO workspace = new SourceWorkspaceDTO
            {
                Name = "whatever"
            };

            IScratchTableRepository scratchTableRepository = Substitute.For<IScratchTableRepository>();

            scratchTableRepository.GetCount().Returns(1);

            TagsContainer tagsContainer = new TagsContainer(job, workspace);

            // Act
            _instance.TagDocuments(tagsContainer, scratchTableRepository);

            // Assert
            _dataSynchronizer
                .Received(1)
                .SyncData(
                    Arg.Any<IDataTransferContext>(),
                    Arg.Any<FieldMap[]>(),
                    _importConfig,
                    null);
        }

        [Test]
        public void ItShouldNotImportTaggingFieldsWhenThereIsNoDocumentToTag()
        {
            // Arrange
            SourceJobDTO job = new SourceJobDTO
            {
                Name = "whatever"
            };
            SourceWorkspaceDTO workspace = new SourceWorkspaceDTO
            {
                Name = "whatever"
            };

            IScratchTableRepository scratchTableRepository = Substitute.For<IScratchTableRepository>();
            scratchTableRepository.GetCount().Returns(0);

            TagsContainer tagsContainer = new TagsContainer(job, workspace);

            // Act
            _instance.TagDocuments(tagsContainer, scratchTableRepository);

            // Assert
            _dataSynchronizer
                .DidNotReceiveWithAnyArgs()
                .SyncData(
                    Arg.Any<IDataTransferContext>(),
                    Arg.Any<FieldMap[]>(),
                    _importConfig,
                    null);
        }
    }
}
