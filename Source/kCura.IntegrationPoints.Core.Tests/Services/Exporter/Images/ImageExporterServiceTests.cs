using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Images;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Images
{
    [TestFixture, Category("Unit")]
    public class ImageExporterServiceTests : TestBase
    {
        private ImageExporterService _sut;
        private IDocumentRepository _documentRepository;
        private IRepositoryFactory _repositoryFactoryMock;
        private IJobStopManager _jobStopManager;
        private IHelper _helper;
        private FieldMap[] _mappedFields;
        private IFileRepository _fileRepository;
        private IRelativityObjectManager _relativityObjectManager;
        private ISerializer _serializer;
        private const int _START_AT = 0;
        private const int _SEARCH_ARTIFACT_ID = 0;
        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;
        private const int _TARGET_WORKSPACE_ARTIFACT_ID = 2;
        private const int _FIELD_IDENTIFIER = 12345;

        [SetUp]
        public override void SetUp()
        {
            _documentRepository = Substitute.For<IDocumentRepository>();
            _repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
            _jobStopManager = Substitute.For<IJobStopManager>();
            _helper = Substitute.For<IHelper>();
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();
            _serializer = Substitute.For<ISerializer>();

            _mappedFields = new[]
            {
                new FieldMap()
                {
                    SourceField = new FieldEntry()
                    {
                        FieldIdentifier = _FIELD_IDENTIFIER.ToString(),
                        IsIdentifier = true
                    },
                }
            };

            var exportJobInfo = new ExportInitializationResultsDto(new Guid(), 1, new[] { "Name", "Identifier" });

            _documentRepository
                .InitializeSearchExportAsync(_SEARCH_ARTIFACT_ID, Arg.Any<int[]>(), _START_AT)
                .Returns(exportJobInfo);

            _documentRepository
                .InitializeProductionExportAsync(_SEARCH_ARTIFACT_ID, Arg.Any<int[]>(), _START_AT)
                .Returns(exportJobInfo);

            IQueryFieldLookupRepository queryFieldLookupRepository = Substitute.For<IQueryFieldLookupRepository>();
            var viewFieldInfo = new ViewFieldInfo("", "", FieldTypeHelper.FieldType.Empty);
            queryFieldLookupRepository.GetFieldByArtifactID(_FIELD_IDENTIFIER).Returns(viewFieldInfo);
            _repositoryFactoryMock.GetQueryFieldLookupRepository(_SOURCE_WORKSPACE_ARTIFACT_ID).Returns(queryFieldLookupRepository);

            _fileRepository = Substitute.For<IFileRepository>();
        }

        [Test]
        public void ItShouldGetDataTransferContext()
        {
            // Arrange
            SourceConfiguration sourceConfiguration = GetConfig(SourceConfiguration.ExportType.SavedSearch);

            _sut = new ImageExporterService(
                _documentRepository,
                _relativityObjectManager,
                _repositoryFactoryMock,
                _fileRepository,
                _jobStopManager,
                _helper,
                _serializer,
                _mappedFields,
                _START_AT,
                sourceConfiguration,
                _SEARCH_ARTIFACT_ID,
                null);

            IExporterTransferConfiguration transferConfiguration = Substitute.For<IExporterTransferConfiguration>();

            // Act
            IDataTransferContext actual = _sut.GetDataTransferContext(transferConfiguration);

            // Assert
            Assert.NotNull(actual);
            Assert.IsInstanceOf(typeof(ExporterTransferContext), actual);
        }

        [Test]
        public void ItShouldRetrieveOriginalImages()
        {
            // Arrange
            SourceConfiguration config = GetConfig(SourceConfiguration.ExportType.SavedSearch);
            DestinationConfiguration settings = new DestinationConfiguration();
            const int documentArtifactID = 10000;

            _documentRepository
                .RetrieveResultsBlockFromExportAsync(Arg.Any<ExportInitializationResultsDto>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(PrepareRetrievedData(documentArtifactID));

            _fileRepository
                .GetImagesLocationForDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    Arg.Is<int[]>(x => x.Single() == documentArtifactID))
                .Returns(CreateDocumentImageResponses(new[] { documentArtifactID }));

            _sut = new ImageExporterService(
                _documentRepository,
                _relativityObjectManager,
                _repositoryFactoryMock,
                _fileRepository,
                _jobStopManager,
                _helper,
                _serializer,
                _mappedFields,
                _START_AT,
                config,
                _SEARCH_ARTIFACT_ID,
                settings
            );

            _sut.GetDataTransferContext(Substitute.For<IExporterTransferConfiguration>());

            // Act
            ArtifactDTO[] actual = _sut.RetrieveData(0);

            // Assert
            Assert.That(actual.Length, Is.EqualTo(1));
            Assert.That(actual[0].GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME).Value, Is.EqualTo("AZIPPER_0007293"));
        }

        [Test]
        public void ItShouldRetrieveProductionImages()
        {
            // Arrange
            const int productionArtifactID = 10010;
            const int documentArtifactID = 10000;

            SourceConfiguration config = GetConfig(SourceConfiguration.ExportType.ProductionSet, productionArtifactID);

            DestinationConfiguration _settings = new DestinationConfiguration()
            {
                ProductionPrecedence = (int)ExportSettings.ProductionPrecedenceType.Original,
                ImagePrecedence = new List<ProductionDTO> { new ProductionDTO() { ArtifactID = productionArtifactID.ToString() } },
            };

            _documentRepository
                .RetrieveResultsBlockFromExportAsync(Arg.Any<ExportInitializationResultsDto>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(PrepareRetrievedData(documentArtifactID));

            _fileRepository
                .GetImagesLocationForProductionDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    productionArtifactID,
                    Arg.Is<int[]>(x => x.Single() == documentArtifactID))
                .Returns(CreateProductionDocumentImageResponses(new int[] { documentArtifactID }));

            _sut = new ImageExporterService(
                _documentRepository,
                _relativityObjectManager,
                _repositoryFactoryMock,
                _fileRepository,
                _jobStopManager,
                _helper,
                _serializer,
                _mappedFields,
                _START_AT,
                config,
                _SEARCH_ARTIFACT_ID,
                _settings);

            _sut.GetDataTransferContext(Substitute.For<IExporterTransferConfiguration>());

            // Act
            ArtifactDTO[] actual = _sut.RetrieveData(0);

            // Assert
            Assert.That(actual.Length, Is.EqualTo(1));
            Assert.That(actual[0].GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD_NAME).Value, Is.EqualTo("AZIPPER_0007293"));
        }

        [Test]
        public void RetrieveData_ShouldLoadOriginalImagesAfterImagePrecedence()
        {
            const int productionArtifactID = 10010;
            const int documentCount = 50;

            SourceConfiguration config = GetConfig(SourceConfiguration.ExportType.SavedSearch, productionArtifactID);

            DestinationConfiguration _settings = new DestinationConfiguration()
            {
                ProductionPrecedence = (int)ExportSettings.ProductionPrecedenceType.Produced,
                ImagePrecedence = Enumerable.Range(1, 3).Select(x => new ProductionDTO() { ArtifactID = x.ToString() }).ToList(),
                IncludeOriginalImages = true
            };

            _documentRepository
                .RetrieveResultsBlockFromExportAsync(Arg.Any<ExportInitializationResultsDto>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(Enumerable.Range(1, documentCount).SelectMany(PrepareRetrievedData).ToList());

            _fileRepository
                .GetImagesLocationForProductionDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    1,
                    Arg.Any<int[]>())
                .Returns(callInfo =>
                    CreateProductionDocumentImageResponses(new[] { 1, 2, 3 }, 1));

            _fileRepository
                .GetImagesLocationForProductionDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    2,
                    Arg.Any<int[]>())
                .Returns(callInfo =>
                    CreateProductionDocumentImageResponses(new[] { 1, 2, 3, 4, 5, 6 }, 2));

            _fileRepository
                .GetImagesLocationForProductionDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    3,
                    Arg.Any<int[]>())
                .Returns(callInfo =>
                    CreateProductionDocumentImageResponses(Enumerable.Range(1, documentCount - 10).ToArray(), 3));

            _fileRepository.GetImagesLocationForDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    Arg.Any<int[]>())
                .Returns(CreateDocumentOriginalImageResponses(Enumerable.Range(1, documentCount).ToArray()));

            _sut = new ImageExporterService(
                _documentRepository,
                _relativityObjectManager,
                _repositoryFactoryMock,
                _fileRepository,
                _jobStopManager,
                _helper,
                _serializer,
                _mappedFields,
                _START_AT,
                config,
                _SEARCH_ARTIFACT_ID,
                _settings);

            _sut.GetDataTransferContext(Substitute.For<IExporterTransferConfiguration>());

            // Act
            ArtifactDTO[] actual = _sut.RetrieveData(0);
            var sourceProductionIds = actual.Select(x => x.GetFieldByName("NATIVE_FILE_PATH_001").Value.ToString().Split('_').First()).ToArray();

            // Assert
            actual.Length.Should().Be(documentCount);

            sourceProductionIds.Take(3).All(x => x == "1").Should().BeTrue();
            sourceProductionIds.Skip(3).Take(3).All(x => x == "2").Should().BeTrue();
            sourceProductionIds.Take(40).Skip(6).All(x => x == "3").Should().BeTrue();
            sourceProductionIds.Skip(40).All(x => x == "\\someLocation").Should().BeTrue();

        }

        [Test]
        public void RetrieveData_ShouldRespectImagePrecedenceSetting()
        {
            const int productionArtifactID = 10010;
            const int documentCount = 50;

            SourceConfiguration config = GetConfig(SourceConfiguration.ExportType.SavedSearch, productionArtifactID);

            DestinationConfiguration _settings = new DestinationConfiguration()
            {
                ProductionPrecedence = (int)ExportSettings.ProductionPrecedenceType.Produced,
                ImagePrecedence = Enumerable.Range(1, 3).Select(x => new ProductionDTO() { ArtifactID = x.ToString() }).ToList(),
            };

            _documentRepository
                .RetrieveResultsBlockFromExportAsync(Arg.Any<ExportInitializationResultsDto>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(Enumerable.Range(1, documentCount).SelectMany(PrepareRetrievedData).ToList());

            _fileRepository
                .GetImagesLocationForProductionDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    1,
                    Arg.Any<int[]>())
                .Returns(callInfo =>
                    CreateProductionDocumentImageResponses(new[] { 1, 2, 3 }, 1));

            _fileRepository
                .GetImagesLocationForProductionDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    2,
                    Arg.Any<int[]>())
                .Returns(callInfo =>
                    CreateProductionDocumentImageResponses(new[] { 1, 2, 3, 4, 5, 6 }, 2));

            _fileRepository
                .GetImagesLocationForProductionDocuments(
                    _SOURCE_WORKSPACE_ARTIFACT_ID,
                    3,
                    Arg.Any<int[]>())
                .Returns(callInfo =>
                    CreateProductionDocumentImageResponses(Enumerable.Range(1, documentCount).ToArray(), 3));

            _sut = new ImageExporterService(
                _documentRepository,
                _relativityObjectManager,
                _repositoryFactoryMock,
                _fileRepository,
                _jobStopManager,
                _helper,
                _serializer,
                _mappedFields,
                _START_AT,
                config,
                _SEARCH_ARTIFACT_ID,
                _settings);

            _sut.GetDataTransferContext(Substitute.For<IExporterTransferConfiguration>());

            // Act
            ArtifactDTO[] actual = _sut.RetrieveData(0);
            var sourceProductionIds = actual.Select(x => x.GetFieldByName("NATIVE_FILE_PATH_001").Value.ToString().Split('_').First()).ToArray();

            // Assert
            actual.Length.Should().Be(documentCount);

            sourceProductionIds.Take(3).All(x => x == "1").Should().BeTrue();
            sourceProductionIds.Skip(3).Take(3).All(x => x == "2").Should().BeTrue();
            sourceProductionIds.Skip(6).All(x => x == "3").Should().BeTrue();
        }

        private SourceConfiguration GetConfig(SourceConfiguration.ExportType exportType, int sourceProductionID = 0)
        {
            var sourceConfiguration = new SourceConfiguration()
            {
                SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
                TargetWorkspaceArtifactId = _TARGET_WORKSPACE_ARTIFACT_ID,
                TypeOfExport = exportType,
                SourceProductionId = sourceProductionID
            };

            return sourceConfiguration;
        }

        private IList<RelativityObjectSlimDto> PrepareRetrievedData(int documentArtifactId)
        {
            var data = new Dictionary<string, object>
            {
                {"Control Number", "AZIPPER_0007293" },
                {"ArtifactID", documentArtifactId}
            };
            var relativityObjectSlimDto = new RelativityObjectSlimDto(documentArtifactId, data);
            var retrievedData = new List<RelativityObjectSlimDto> { relativityObjectSlimDto };
            return retrievedData;
        }

        private ILookup<int, ImageFile> CreateDocumentOriginalImageResponses(int[] ids)
        {
            return ids.ToLookup(x => x, x => new ImageFile(x, $"\\someLocation_{x}", "x.tiff", 1));
        }

        private ILookup<int, ImageFile> CreateDocumentImageResponses(int[] ids)
        {
            return ids.ToLookup(x => x, x => new ImageFile(x, $"\\someLocation_{x}", "x.tiff", 1));
        }

        private ILookup<int, ImageFile> CreateProductionDocumentImageResponses(int[] ids)
        {
            return ids.ToLookup(x => x, x => new ImageFile(x, $"0_{x}", "x.tiff", 1));
        }

        private ILookup<int, ImageFile> CreateProductionDocumentImageResponses(int[] ids, int productionId)
        {
            return ids.ToLookup(x => x, x => new ImageFile(x, $"{productionId}_{x}", "x.tiff", 1, productionId));
        }
    }
}
