using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class FileRepositoryTests
    {
        private Mock<ISearchService> _searchServiceMock;
        private Mock<IServicesMgr> _servicesMgr;
        private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
        private Mock<IExternalServiceSimpleInstrumentation> _instrumentationSimpleProviderMock;
        private Mock<IRetryHandler> _retryHandlerMock;
        private Mock<IRetryHandlerFactory> _retryHandlerFactoryMock;
        private FileRepository _sut;
        private const int _WORKSPACE_ID = 1001000;
        private const string _KEPLER_SERVICE_TYPE = "Kepler";
        private const string _KEPLER_SERVICE_NAME = nameof(ISearchService);
        private const string _DOCUMENT_ARTIFACT_ID_COLUMN = "DocumentArtifactID";
        private const string _FILE_NAME_COLUMN = "Filename";
        private const string _LOCATION_COLUMN = "Location";
        private const string _FILE_SIZE_COLUMN = "Size";
        private readonly FileResponse[] _testFileResponses =
        {
            new FileResponse
            {
                DocumentArtifactID = 1700,
                Filename = "Filename12",
                Guid = "82644DB3-3865-4B99-9DB4-60CE40401BD1",
                Identifier = "Identifier22",
                Location = "Location33",
                Order = 0,
                Rotation = 1,
                Type = 2,
                InRepository = true,
                Size = 1234,
                Details = "Details232",
                Billable = true
            },
            new FileResponse
            {
                DocumentArtifactID = 17023,
                Filename = "Filename124",
                Guid = "82644DB1-3865-4B99-9DB4-60CE40401BD1",
                Identifier = "Identifier232",
                Location = "Location331",
                Order = 0,
                Rotation = 1,
                Type = 2,
                InRepository = true,
                Size = 1234,
                Details = "Details2325",
                Billable = true
            },
        };

        private readonly ProductionDocumentImageResponse[] _testProductionDocumentImageResponses =
        {
            new ProductionDocumentImageResponse
            {
                DocumentArtifactID = 1700,
                BatesNumber = "Bates123",
                Location = "Location33",
                ByteRange = 2,
                ImageFileName = "FileName1234",
                ImageGuid = "82644DB3-3865-4B99-9DB4-60CE40401BD1",
                ImageSize = 1234,
                NativeIdentifier = "NativeIdentifier888",
                PageID = 123,
                SourceGuid = "32644DB3-3865-4B99-9DB4-60CE40401BD1"
            },
            new ProductionDocumentImageResponse
            {
                DocumentArtifactID = 1702,
                BatesNumber = "Bates1233",
                Location = "Location313",
                ByteRange = 23,
                ImageFileName = "FileName12134",
                ImageGuid = "82644DB3-3865-4B99-9DB4-61CE40401BD1",
                ImageSize = 12234,
                NativeIdentifier = "NativeIdentifier1888",
                PageID = 1123,
                SourceGuid = "32644DB2-3865-4B99-9DB4-60CE40401BD1"
            },
        };

        private readonly DocumentImageResponse[] _testDocumentImageResponses =
        {
            new DocumentImageResponse
            {
                DocumentArtifactID = 1700,
                FileID = 12,
                FileName = "FileName123",
                Guid = "12644DB3-3865-4B99-9DB4-61CE40401BD1",
                Identifier = "Identifier234",
                Location = "Location22",
                Order = 1,
                Rotation = -1,
                Type = 2,
                InRepository = true,
                Size = 12344,
                Details = "Details999",
                Billable = true,
                PageID = 11,
                ByteRange = 4555
            },
            new DocumentImageResponse
            {
                DocumentArtifactID = 1701,
                FileID = 122,
                FileName = "FileName121",
                Guid = "11644DB3-3865-4B99-9DB4-61CE40401BD1",
                Identifier = "Identifier2341",
                Location = "Location221",
                Order = 12,
                Rotation = 0,
                Type = 3,
                InRepository = false,
                Size = 123441,
                Details = "Details9991",
                Billable = false,
                PageID = 121,
                ByteRange = 45155
            },
        };

        [SetUp]
        public void SetUp()
        {
            _searchServiceMock = new Mock<ISearchService>();
            _servicesMgr = new Mock<IServicesMgr>();
            _servicesMgr.Setup(x => x.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser)).Returns(_searchServiceMock.Object);
            _retryHandlerMock = new Mock<IRetryHandler>();
            _retryHandlerFactoryMock = new Mock<IRetryHandlerFactory>();
            _retryHandlerFactoryMock
                .Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>()))
                .Returns(_retryHandlerMock.Object);
            _instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
            _instrumentationSimpleProviderMock = new Mock<IExternalServiceSimpleInstrumentation>();
            _instrumentationProviderMock
                .Setup(x => x.CreateSimple(
                    _KEPLER_SERVICE_TYPE,
                    _KEPLER_SERVICE_NAME,
                    It.IsAny<string>()))
                .Returns(_instrumentationSimpleProviderMock.Object);

            _sut = new FileRepository(_servicesMgr.Object, _instrumentationProviderMock.Object, _retryHandlerFactoryMock.Object );
        }

        [Test]
        public void GetImagesForProductionDocuments_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
        {
            // arrange
            const int productionID = 1111;
            int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
            _instrumentationSimpleProviderMock
                .Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
                .Returns(_testProductionDocumentImageResponses.ToDataSet());
            _retryHandlerMock
                .Setup(x => x.ExecuteWithRetries(It.IsAny<Func<DataSet>>(), It.IsAny<string>()))
                .Returns((Func<DataSet> f, string s) => f.Invoke());

            // act
            ILookup<int, ImageFile> result = _sut.GetImagesLocationForProductionDocuments(
                _WORKSPACE_ID,
                productionID,
                documentIDs
            );

            // assert
            VerifyIfInstrumentationHasBeenCalled<DataSet>(
                operationName: nameof(ISearchService.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync)
            );
            AssertIfListsAreSameAsExpected(
                _testProductionDocumentImageResponses.Select(x => x.Location).ToList(),
                result.SelectMany(x => x).Select(x => x.Location).ToList()
            );
        }

        [Test]
        public void GetImagesForProductionDocuments_ShouldThrowWhenNullPassedAsDocumentIDs()
        {
            // arrange
            const int productionID = 1111;

            // act
            Action action = () => _sut.GetImagesLocationForProductionDocuments(
                _WORKSPACE_ID,
                productionID,
                documentIDs: null
            );

            // assert
            action.ShouldThrow<ArgumentNullException>().Where(x => x.Message.Contains("documentIDs"));
            VerifyIfInstrumentationHasNeverBeenCalled<ProductionDocumentImageResponse[]>(
                operationName: nameof(ISearchService.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync)
            );
        }

        [Test]
        public void GetImagesForProductionDocuments_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
        {
            // arrange
            const int productionID = 1111;

            // act
            ILookup<int, ImageFile> result = _sut.GetImagesLocationForProductionDocuments(
                _WORKSPACE_ID,
                productionID,
                documentIDs: new int[] { }
            );

            // assert
            result.SelectMany(x => x).Should().BeEmpty();
            VerifyIfInstrumentationHasNeverBeenCalled<ProductionDocumentImageResponse[]>(
                operationName: nameof(ISearchService.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync)
            );
        }

        [Test]
        public void GetImagesForProductionDocuments_ShouldRethrowWhenCallToServiceThrows()
        {
            // arrange
            int productionID = 1001;
            int[] documentIDs = { 1001, 2002, 3003 };
            _instrumentationSimpleProviderMock
                .Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
                .Throws<InvalidOperationException>();
            _retryHandlerMock
                .Setup(x => x.ExecuteWithRetries(It.IsAny<Func<DataSet>>(), It.IsAny<string>()))
                .Returns((Func<DataSet> f, string s) => f.Invoke());

            // act
            Action action = () => _sut.GetImagesLocationForProductionDocuments(
                _WORKSPACE_ID,
                productionID,
                documentIDs
            );

            // assert
            action.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void GetImagesForDocuments_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
        {
            // arrange
            int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
            _instrumentationSimpleProviderMock
                .Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
                .Returns(_testDocumentImageResponses.ToDataSet());
            _retryHandlerMock
                .Setup(x => x.ExecuteWithRetries(It.IsAny<Func<DataSet>>(), It.IsAny<string>()))
                .Returns((Func<DataSet> f, string s) => f.Invoke());

            // act
            ILookup<int, ImageFile> result = _sut.GetImagesLocationForDocuments(
                _WORKSPACE_ID,
                documentIDs
            );

            // assert
            VerifyIfInstrumentationHasBeenCalled<DataSet>(
                operationName: nameof(ISearchService.RetrieveImagesForSearchAsync)
            );
            AssertIfListsAreSameAsExpected(
                _testDocumentImageResponses.Select(x => x.Location).ToList(),
                result.SelectMany(x => x).Select(x => x.Location).ToList()
            );
        }

        [Test]
        public void GetImagesForDocuments_ShouldThrowWhenNullPassedAsDocumentIDs()
        {
            // act
            Action action = () => _sut.GetImagesLocationForDocuments(
                _WORKSPACE_ID,
                documentIDs: null
            );

            // assert
            action.ShouldThrow<ArgumentNullException>().Where(x => x.Message.Contains("documentIDs"));
            VerifyIfInstrumentationHasNeverBeenCalled<DocumentImageResponse[]>(
                operationName: nameof(ISearchService.RetrieveImagesForSearchAsync)
            );
        }

        [Test]
        public void GetImagesForDocuments_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
        {
            // act
            ILookup<int, ImageFile> result = _sut.GetImagesLocationForDocuments(
                _WORKSPACE_ID,
                documentIDs: new int[] { }
            );

            // assert
            result.SelectMany(x => x).Should().BeEmpty();
            VerifyIfInstrumentationHasNeverBeenCalled<DocumentImageResponse[]>(
                operationName: nameof(ISearchService.RetrieveImagesForSearchAsync)
            );
        }

        [Test]
        public void GetImagesForDocuments_ShouldRethrowWhenCallToServiceThrows()
        {
            // arrange
            int[] documentIDs = { 1001, 2002, 3003 };
            _retryHandlerMock
                .Setup(x => x.ExecuteWithRetries(It.IsAny<Func<DataSet>>(), It.IsAny<string>()))
                .Throws<InvalidOperationException>();

            // act
            Action action = () => _sut.GetImagesLocationForDocuments(
                _WORKSPACE_ID,
                documentIDs
            );

            // assert
            action.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void GetNativesForDocuments_ShouldReturnProperValueWhenCorrectDocumentIDsPassed()
        {
            // arrange
            IList<FileDto> expectedResult =
                CreateTestNativeFileDtos(out var documentArtifactIDsString, out var documentArtifactIDs);
            DataSet filesDataSet = CreateTestNativesDataSet(expectedResult);

            _instrumentationSimpleProviderMock
                .Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
                .Returns<Func<DataSet>>(x =>  x.Invoke());
            _retryHandlerMock
                .Setup(x => x.ExecuteWithRetries(It.IsAny<Func<DataSet>>(), It.IsAny<string>()))
                .Returns( (Func<DataSet> f, string s) => f.Invoke());
            _searchServiceMock
                .Setup(x => x.RetrieveNativesForSearchAsync(_WORKSPACE_ID, documentArtifactIDsString, string.Empty))
                .ReturnsAsync(new DataSetWrapper(filesDataSet));

            // act
            List<FileDto> result = _sut.GetNativesForDocuments(_WORKSPACE_ID, documentArtifactIDs);

            // assert
            VerifyIfInstrumentationHasBeenCalled<DataSet>(
                operationName: nameof(ISearchService.RetrieveNativesForSearchAsync)
            );
            AssertFileDtosAreIdentical(result, expectedResult);
        }

        [Test]
        public void GetNativesForDocuments_ShouldThrowWhenNullPassedAsDocumentIDs()
        {
            // act
            Action action = () => _sut.GetNativesForDocuments(
                _WORKSPACE_ID,
                documentIDs: null
            );

            // assert
            action.ShouldThrow<ArgumentNullException>().Where(x => x.Message.Contains("documentIDs"));
            VerifyIfInstrumentationHasNeverBeenCalled<DocumentImageResponse[]>(
                operationName: nameof(ISearchService.RetrieveNativesForSearchAsync)
            );
        }

        [Test]
        public void GetNativesForDocuments_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
        {
            // act
            List<FileDto> result = _sut.GetNativesForDocuments(
                _WORKSPACE_ID,
                documentIDs: new int[] { }
            );

            // assert
            result.Should().BeEmpty();
            VerifyIfInstrumentationHasNeverBeenCalled<DocumentImageResponse[]>(
                operationName: nameof(ISearchService.RetrieveNativesForSearchAsync)
            );
        }

        [Test]
        public void GetNativesForDocuments_ShouldRethrowWhenCallToServiceThrows()
        {
            // arrange
            int[] documentIDs = { 1001, 2002, 3003 };
            _instrumentationSimpleProviderMock
                .Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
                .Throws<InvalidOperationException>();
            _retryHandlerMock
                .Setup(x => x.ExecuteWithRetries(It.IsAny<Func<DataSet>>(), It.IsAny<string>()))
                .Returns((Func<DataSet> f, string s) => f.Invoke());

            // act
            Action action = () => _sut.GetNativesForDocuments(
                _WORKSPACE_ID,
                documentIDs
            );

            // assert
            action.ShouldThrow<InvalidOperationException>();
        }

        private void AssertIfListsAreSameAsExpected(List<string> expectedDataSet, List<string> currentDataSet)
        {
            expectedDataSet.Should().Equal(currentDataSet);
        }

        private void VerifyIfInstrumentationHasBeenCalled<T>(string operationName)
        {
            _instrumentationProviderMock.Verify(
                x => x.CreateSimple(
                    _KEPLER_SERVICE_TYPE,
                    _KEPLER_SERVICE_NAME,
                    operationName
                ),
                Times.Once
            );
            _instrumentationSimpleProviderMock.Verify(
                x => x.Execute(It.IsAny<Func<T>>()),
                Times.Once
            );
        }

        private void VerifyIfInstrumentationHasNeverBeenCalled<T>(string operationName)
        {
            _instrumentationProviderMock.Verify(
                x => x.CreateSimple(
                    _KEPLER_SERVICE_TYPE,
                    _KEPLER_SERVICE_NAME,
                    operationName
                ),
                Times.Never
            );
            _instrumentationSimpleProviderMock.Verify(
                x => x.ExecuteAsync(It.IsAny<Func<Task<T>>>()),
                Times.Never
            );
        }

        private static IList<FileDto> CreateTestNativeFileDtos(out string documentArtifactIDsString, out int[] documentArtifactIDs)
        {
            const int documentArtifactID1 = 1000123;
            const int documentArtifactID2 = 1000456;
            documentArtifactIDs = new[] { documentArtifactID1, documentArtifactID2 };
            documentArtifactIDsString = string.Join(",", documentArtifactIDs.Select(x => x.ToString()));
            FileDto file1 = new FileDto
            {
                DocumentArtifactID = documentArtifactID1,
                Location = "Location1",
                FileSize = 1000,
                FileName = "Name1"
            };
            FileDto file2 = new FileDto
            {
                DocumentArtifactID = documentArtifactID2,
                Location = "Location2",
                FileSize = 2000,
                FileName = "Name2"
            };
            return new List<FileDto> { file1, file2 };
        }

        private static DataSet CreateTestNativesDataSet(IEnumerable<FileDto> fileDtos)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(_DOCUMENT_ARTIFACT_ID_COLUMN, typeof(int));
            dataTable.Columns.Add(_FILE_NAME_COLUMN, typeof(string));
            dataTable.Columns.Add(_LOCATION_COLUMN, typeof(string));
            dataTable.Columns.Add(_FILE_SIZE_COLUMN, typeof(long));
            DataSet filesDataSet = new DataSet();
            foreach (var fileDto in fileDtos)
            {
                AddFileDtoToDataTable(dataTable, fileDto);
            }
            filesDataSet.Tables.Add(dataTable);
            return filesDataSet;
        }

        private static void AddFileDtoToDataTable(DataTable dataTable, FileDto fileDto)
        {
            object[] values = { fileDto.DocumentArtifactID, fileDto.FileName, fileDto.Location, fileDto.FileSize };
            dataTable.Rows.Add(values);
        }

        private static void AssertFileDtosAreIdentical(IEnumerable<FileDto> actual, IEnumerable<FileDto> expected)
        {
            actual.Zip(expected, (x, y) => new
            {
                Actual = x,
                Expected = y
            }).Should().OnlyContain(item =>
                item.Expected.DocumentArtifactID == item.Actual.DocumentArtifactID &&
                item.Expected.FileName == item.Actual.FileName &&
                item.Expected.FileSize == item.Actual.FileSize &&
                item.Expected.Location == item.Actual.Location);
        }
    }
}
