using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class FileRepositoryTests
	{
		private Mock<ISearchManager> _searchManagerMock;
		private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
		private Mock<IExternalServiceSimpleInstrumentation> _instrumentationSimpleProviderMock;

		private FileRepository _sut;

		private const int _WORKSPACE_ID = 1001000;
		private const int _PRODUCTION_ID = 1710;
		private const int _PRODUCTION_ID_2 = 1711;
		private const string _KEPLER_SERVICE_TYPE = "Kepler";
		private const string _KEPLER_SERVICE_NAME = nameof(ISearchManager);

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

		private readonly ExportProductionDocumentImageResponse[] _testExportProductionDocumentImageResponses =
		{
			new ExportProductionDocumentImageResponse
			{
				DocumentArtifactID = 1700,
				ProductionArtifactID = _PRODUCTION_ID,
				BatesNumber = "Bates123",
				Location = "Location33",
				ByteRange = 2,
				ImageFileName = "FileName1234",
				ImageGuid = "82644DB3-3865-4B99-9DB4-60CE40401BD1",
				ImageSize = 1234,
				PageID = 123,
				SourceGuid = "32644DB3-3865-4B99-9DB4-60CE40401BD1",
				Order = 1
			},
			new ExportProductionDocumentImageResponse
			{
				DocumentArtifactID = 1702,
				ProductionArtifactID = _PRODUCTION_ID_2,
				BatesNumber = "Bates1233",
				Location = "Location313",
				ByteRange = 23,
				ImageFileName = "FileName12134",
				ImageGuid = "82644DB3-3865-4B99-9DB4-61CE40401BD1",
				ImageSize = 12234,
				PageID = 1123,
				SourceGuid = "32644DB2-3865-4B99-9DB4-60CE40401BD1",
				Order = 2
			},
		};

		[SetUp]
		public void SetUp()
		{
			_searchManagerMock = new Mock<ISearchManager>();
			_instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
			_instrumentationSimpleProviderMock = new Mock<IExternalServiceSimpleInstrumentation>();
			_instrumentationProviderMock
				.Setup(x => x.CreateSimple(
					_KEPLER_SERVICE_TYPE,
					_KEPLER_SERVICE_NAME,
					It.IsAny<string>()))
				.Returns(_instrumentationSimpleProviderMock.Object);

			_sut = new FileRepository(_searchManagerMock.Object, _instrumentationProviderMock.Object);
		}

		[Test]
		public void GetNativesForSearch_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Returns(_testFileResponses.ToDataSet());

			//act
			DataSet result = _sut.GetNativesForSearch(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DataSet>(
				operationName: nameof(ISearchManager.RetrieveNativesForSearch)
			);
			AssertIfDataSetsAreSameAsExpected(_testFileResponses.ToDataSet(), result);
		}

		[Test]
		public void GetNativesForSearch_ShouldThrowWhenNullPassedAsDocumentIDs()
		{
			//act
			Action action = () => _sut.GetNativesForSearch(
				_WORKSPACE_ID,
				documentIDs: null
			);

			//assert
			action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentIDs");
			VerifyIfInstrumentationHasNeverBeenCalled<FileResponse[]>(
				operationName: nameof(ISearchManager.RetrieveNativesForSearch)
			);
		}

		[Test]
		public void GetNativesForSearch_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//act
			DataSet result = _sut.GetNativesForSearch(
				_WORKSPACE_ID,
				documentIDs: new int[] { }
			);

			//assert
			result.Tables.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<FileResponse[]>(
				operationName: nameof(ISearchManager.RetrieveNativesForSearch)
			);
		}

		[Test]
		public void GetNativesForSearch_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int[] documentIDs = { 1001, 2002, 3003 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Throws<InvalidOperationException>();

			//act
			Action action = () => _sut.GetNativesForSearch(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
			action.ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void GetNativesForProduction_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			const int productionID = 1111;
			int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Returns(_testFileResponses.ToDataSet());

			//act
			DataSet result = _sut.GetNativesForProduction(
				_WORKSPACE_ID,
				productionID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DataSet>(
				operationName: nameof(ISearchManager.RetrieveNativesForProduction)
			);
			AssertIfDataSetsAreSameAsExpected(_testFileResponses.ToDataSet(), result);
		}

		[Test]
		public void GetNativesForProduction_ShouldThrowWhenNullPassedAsDocumentIDs()
		{
			//arrange
			const int productionID = 1111;

			//act
			Action action = () => _sut.GetNativesForProduction(
				_WORKSPACE_ID,
				productionID,
				documentIDs: null
			);

			//assert
			action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentIDs");
			VerifyIfInstrumentationHasNeverBeenCalled<FileResponse[]>(
				operationName: nameof(ISearchManager.RetrieveNativesForProduction)
			);
		}

		[Test]
		public void GetNativesForProduction_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//arrange
			const int productionID = 1111;

			//act
			DataSet result = _sut.GetNativesForProduction(
				_WORKSPACE_ID,
				productionID,
				documentIDs: new int[] { }
			);

			//assert
			result.Tables.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<FileResponse[]>(
				operationName: nameof(ISearchManager.RetrieveNativesForProduction)
			);
		}

		[Test]
		public void GetNativesForProduction_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int productionID = 1001;
			int[] documentIDs = { 1001, 2002, 3003 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Throws<InvalidOperationException>();

			//act
			Action action = () => _sut.GetNativesForProduction(
				_WORKSPACE_ID,
				productionID,
				documentIDs
			);

			//assert
			action.ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			const int productionID = 1111;
			int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Returns(_testProductionDocumentImageResponses.ToDataSet());

			//act
			DataSet result = _sut.GetImagesForProductionDocuments(
				_WORKSPACE_ID,
				productionID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DataSet>(
				operationName: nameof(ISearchManager.RetrieveImagesForProductionDocuments)
			);
			AssertIfDataSetsAreSameAsExpected(
				_testProductionDocumentImageResponses.ToDataSet(),
				result
			);
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldThrowWhenNullPassedAsDocumentIDs()
		{
			//arrange
			const int productionID = 1111;

			//act
			Action action = () => _sut.GetImagesForProductionDocuments(
				_WORKSPACE_ID,
				productionID,
				documentIDs: null
			);

			//assert
			action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentIDs");
			VerifyIfInstrumentationHasNeverBeenCalled<ProductionDocumentImageResponse[]>(
				operationName: nameof(ISearchManager.RetrieveImagesForProductionDocuments)
			);
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//arrange
			const int productionID = 1111;

			//act
			DataSet result = _sut.GetImagesForProductionDocuments(
				_WORKSPACE_ID,
				productionID,
				documentIDs: new int[] { }
			);

			//assert
			result.Tables.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<ProductionDocumentImageResponse[]>(
				operationName: nameof(ISearchManager.RetrieveImagesForProductionDocuments)
			);
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int productionID = 1001;
			int[] documentIDs = { 1001, 2002, 3003 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Throws<InvalidOperationException>();

			//act
			Action action = () => _sut.GetImagesForProductionDocuments(
				_WORKSPACE_ID,
				productionID,
				documentIDs
			);

			//assert
			action.ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void GetImagesForDocuments_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Returns(_testDocumentImageResponses.ToDataSet());

			//act
			DataSet result = _sut.GetImagesForDocuments(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DataSet>(
				operationName: nameof(ISearchManager.RetrieveImagesForDocuments)
			);
			AssertIfDataSetsAreSameAsExpected(
				_testDocumentImageResponses.ToDataSet(),
				result
			);
		}

		[Test]
		public void GetImagesForDocuments_ShouldThrowWhenNullPassedAsDocumentIDs()
		{
			//act
			Action action = () => _sut.GetImagesForDocuments(
				_WORKSPACE_ID,
				documentIDs: null
			);

			//assert
			action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentIDs");
			VerifyIfInstrumentationHasNeverBeenCalled<DocumentImageResponse[]>(
				operationName: nameof(ISearchManager.RetrieveImagesForDocuments)
			);
		}

		[Test]
		public void GetImagesForDocuments_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//act
			DataSet result = _sut.GetImagesForDocuments(
				_WORKSPACE_ID,
				documentIDs: new int[] { }
			);

			//assert
			result.Tables.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<DocumentImageResponse[]>(
				operationName: nameof(ISearchManager.RetrieveImagesForDocuments)
			);
		}

		[Test]
		public void GetImagesForDocuments_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int[] documentIDs = { 1001, 2002, 3003 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Throws<InvalidOperationException>();

			//act
			Action action = () => _sut.GetImagesForDocuments(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
			action.ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void GetProducedImagesForDocument_ShouldReturnCorrectResponses()
		{
			//arrange
			int documentID = _testFileResponses.First().DocumentArtifactID;
			DataSet fileResponses = _testFileResponses
				.Where(x => x.DocumentArtifactID == documentID)
				.ToDataSet();
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Returns(fileResponses);

			//act
			DataSet result = _sut.GetProducedImagesForDocument(
				_WORKSPACE_ID,
				documentID
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DataSet>(
				operationName: nameof(ISearchManager.RetrieveProducedImagesForDocument)
			);
			AssertIfDataSetsAreSameAsExpected(
				fileResponses,
				result
			);
		}

		[Test]
		public void GetProducedImagesForDocument_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int documentID = 1001;
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Throws<InvalidOperationException>();

			//act
			Action action = () => _sut.GetProducedImagesForDocument(
				_WORKSPACE_ID,
				documentID
			);

			//assert
			action.ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void GetImagesForExport_ShouldReturnResponsesWhenCorrectDocumentIDsAndProductionIDsPassed()
		{
			//arrange
			int[] documentIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.DocumentArtifactID)
				.ToArray();
			int[] productionIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.ProductionArtifactID)
				.ToArray();
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Returns(_testExportProductionDocumentImageResponses.ToDataSet());

			//act
			DataSet result = _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DataSet>(
				operationName: nameof(ISearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport)
			);
			AssertIfDataSetsAreSameAsExpected(
				_testExportProductionDocumentImageResponses.ToDataSet(),
				result
			);
		}

		[Test]
		public void GetImagesForExport_ShouldThrowWhenNullPassedAsDocumentIDs()
		{
			//arrange
			int[] productionIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.ProductionArtifactID)
				.ToArray();

			//act
			Action action = () => _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs,
				documentIDs: null
			);

			//assert
			action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentIDs");
			VerifyIfInstrumentationHasNeverBeenCalled<ExportProductionDocumentImageResponse[]>(
				operationName: nameof(ISearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport)
			);
		}

		[Test]
		public void GetImagesForExport_ShouldThrowWhenNullPassedAsProductionIDs()
		{
			//arrange
			int[] documentIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.ProductionArtifactID)
				.ToArray();

			//act
			Action action = () => _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs: null,
				documentIDs: documentIDs
			);

			//assert
			action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: productionIDs");
			VerifyIfInstrumentationHasNeverBeenCalled<ExportProductionDocumentImageResponse[]>(
				operationName: nameof(ISearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport)
			);
		}

		[Test]
		public void GetImagesForExport_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//arrange
			int[] productionIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.ProductionArtifactID)
				.ToArray();

			//act
			DataSet result = _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs,
				documentIDs: new int[] { }
			);

			//assert
			result.Tables.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<ExportProductionDocumentImageResponse[]>(
				operationName: nameof(ISearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport)
			);
		}

		[Test]
		public void GetImagesForExport_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsProductionIDs()
		{
			//arrange
			int[] documentIDs = _testExportProductionDocumentImageResponses
				.Select(x => x.ProductionArtifactID)
				.ToArray();

			//act
			DataSet result = _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs: new int[] { },
				documentIDs: documentIDs
			);

			//assert
			result.Tables.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<ExportProductionDocumentImageResponse[]>(
				operationName: nameof(ISearchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport)
			);
		}

		[Test]
		public void GetImagesForExport_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int[] documentIDs = { 1001, 2002, 3003 };
			int[] productionIDs = { 1011, 2022, 3033 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DataSet>>()))
				.Throws<InvalidOperationException>();

			//act
			Action action = () => _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs,
				documentIDs
			);

			//assert
			action.ShouldThrow<InvalidOperationException>();
		}

		private void AssertIfDataSetsAreSameAsExpected(DataSet expectedDataSet, DataSet currentDataSet)
		{
			expectedDataSet.Tables[0].Rows.Count.ShouldBeEquivalentTo(expectedDataSet.Tables[0].Rows.Count);
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
	}
}
