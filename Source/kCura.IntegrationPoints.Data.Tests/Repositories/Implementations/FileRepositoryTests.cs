using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API.Foundation;
using Relativity.Core.Service;
using Relativity.Services.FileField.Models;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class FileRepositoryTests
	{
		private Mock<IFileManager> _fileManagerMock;
		private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
		private Mock<IExternalServiceSimpleInstrumentation> _instrumentationSimpleProviderMock;

		private FileRepository _sut;

		private const int _WORKSPACE_ID = 1001000;
		private const int _PRODUCTION_ID = 1710;
		private const int _PRODUCTION_ID_2 = 1711;
		private const string _KEPLER_SERVICE_TYPE = "Kepler";
		private const string _KEPLER_SERVICE_NAME = nameof(IFileManager);

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
			_fileManagerMock = new Mock<IFileManager>();
			_instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
			_instrumentationSimpleProviderMock = new Mock<IExternalServiceSimpleInstrumentation>();
			_instrumentationProviderMock
				.Setup(x => x.CreateSimple(
					_KEPLER_SERVICE_TYPE,
					_KEPLER_SERVICE_NAME,
					It.IsAny<string>()))
				.Returns(_instrumentationSimpleProviderMock.Object);

			_sut = new FileRepository(_fileManagerMock.Object, _instrumentationProviderMock.Object);
		}

		[Test]
		public void GetNativesForSearch_ShouldReturnResponsesWhenCorrectDocumentIDsPassed()
		{
			//arrange
			int[] documentIDs = _testFileResponses.Select(x => x.DocumentArtifactID).ToArray();
			_instrumentationSimpleProviderMock
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<FileResponse[]>>>()))
				.Returns(Task.FromResult(_testFileResponses));

			//act
			FileResponse[] result = _sut.GetNativesForSearch(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<FileResponse[]>(
				operationName: nameof(IFileManager.GetNativesForSearchAsync)
			);
			AssertIfResponsesAreSameAsExpected(_testFileResponses, result);
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
				operationName: nameof(IFileManager.GetNativesForSearchAsync)
			);
		}

		[Test]
		public void GetNativesForSearch_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//act
			FileResponse[] result = _sut.GetNativesForSearch(
				_WORKSPACE_ID,
				documentIDs: new int[] { }
			);

			//assert
			result.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<FileResponse[]>(
				operationName: nameof(IFileManager.GetNativesForSearchAsync)
			);
		}

		[Test]
		public void GetNativesForSearch_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int[] documentIDs = { 1001, 2002, 3003 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<FileResponse[]>>>()))
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
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<FileResponse[]>>>()))
				.Returns(Task.FromResult(_testFileResponses));

			//act
			FileResponse[] result = _sut.GetNativesForProduction(
				_WORKSPACE_ID,
				productionID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<FileResponse[]>(
				operationName: nameof(IFileManager.GetNativesForProductionAsync)
			);
			AssertIfResponsesAreSameAsExpected(_testFileResponses, result);
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
				operationName: nameof(IFileManager.GetNativesForProductionAsync)
			);
		}

		[Test]
		public void GetNativesForProduction_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//arrange
			const int productionID = 1111;

			//act
			FileResponse[] result = _sut.GetNativesForProduction(
				_WORKSPACE_ID,
				productionID,
				documentIDs: new int[] { }
			);

			//assert
			result.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<FileResponse[]>(
				operationName: nameof(IFileManager.GetNativesForProductionAsync)
			);
		}

		[Test]
		public void GetNativesForProduction_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int productionID = 1001;
			int[] documentIDs = { 1001, 2002, 3003 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<FileResponse[]>>>()))
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
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<ProductionDocumentImageResponse[]>>>()))
				.Returns(Task.FromResult(_testProductionDocumentImageResponses));

			//act
			ProductionDocumentImageResponse[] result = _sut.GetImagesForProductionDocuments(
				_WORKSPACE_ID,
				productionID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<ProductionDocumentImageResponse[]>(
				operationName: nameof(IFileManager.GetImagesForProductionDocumentsAsync)
			);
			AssertIfResponsesAreSameAsExpected(
				_testProductionDocumentImageResponses,
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
				operationName: nameof(IFileManager.GetImagesForProductionDocumentsAsync)
			);
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//arrange
			const int productionID = 1111;

			//act
			ProductionDocumentImageResponse[] result = _sut.GetImagesForProductionDocuments(
				_WORKSPACE_ID,
				productionID,
				documentIDs: new int[] { }
			);

			//assert
			result.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<ProductionDocumentImageResponse[]>(
				operationName: nameof(IFileManager.GetImagesForProductionDocumentsAsync)
			);
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int productionID = 1001;
			int[] documentIDs = { 1001, 2002, 3003 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<ProductionDocumentImageResponse[]>>>()))
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
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<DocumentImageResponse[]>>>()))
				.Returns(Task.FromResult(_testDocumentImageResponses));

			//act
			DocumentImageResponse[] result = _sut.GetImagesForDocuments(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DocumentImageResponse[]>(
				operationName: nameof(IFileManager.GetImagesForDocumentsAsync)
			);
			AssertIfResponsesAreSameAsExpected(
				_testDocumentImageResponses,
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
				operationName: nameof(IFileManager.GetImagesForDocumentsAsync)
			);
		}

		[Test]
		public void GetImagesForDocuments_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsDocumentIDs()
		{
			//act
			DocumentImageResponse[] result = _sut.GetImagesForDocuments(
				_WORKSPACE_ID,
				documentIDs: new int[] { }
			);

			//assert
			result.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<DocumentImageResponse[]>(
				operationName: nameof(IFileManager.GetImagesForDocumentsAsync)
			);
		}

		[Test]
		public void GetImagesForDocuments_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int[] documentIDs = { 1001, 2002, 3003 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<DocumentImageResponse[]>>>()))
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
			FileResponse[] fileResponses = _testFileResponses
				.Where(x => x.DocumentArtifactID == documentID)
				.ToArray();
			_instrumentationSimpleProviderMock
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<FileResponse[]>>>()))
				.Returns(Task.FromResult(fileResponses));

			//act
			FileResponse[] result = _sut.GetProducedImagesForDocument(
				_WORKSPACE_ID,
				documentID
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<FileResponse[]>(
				operationName: nameof(IFileManager.GetProducedImagesForDocumentAsync)
			);
			AssertIfResponsesAreSameAsExpected(
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
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<FileResponse[]>>>()))
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
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<ExportProductionDocumentImageResponse[]>>>()))
				.Returns(Task.FromResult(_testExportProductionDocumentImageResponses));

			//act
			ExportProductionDocumentImageResponse[] result = _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<ExportProductionDocumentImageResponse[]>(
				operationName: nameof(IFileManager.GetImagesForExportAsync)
			);
			AssertIfResponsesAreSameAsExpected(
				_testExportProductionDocumentImageResponses,
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
				operationName: nameof(IFileManager.GetImagesForExportAsync)
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
				operationName: nameof(IFileManager.GetImagesForExportAsync)
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
			ExportProductionDocumentImageResponse[] result = _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs,
				documentIDs: new int[] { }
			);

			//assert
			result.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<ExportProductionDocumentImageResponse[]>(
				operationName: nameof(IFileManager.GetImagesForExportAsync)
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
			ExportProductionDocumentImageResponse[] result = _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs: new int[] { },
				documentIDs: documentIDs
			);

			//assert
			result.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<ExportProductionDocumentImageResponse[]>(
				operationName: nameof(IFileManager.GetImagesForExportAsync)
			);
		}

		[Test]
		public void GetImagesForExport_ShouldRethrowWhenCallToServiceThrows()
		{
			//arrange
			int[] documentIDs = { 1001, 2002, 3003 };
			int[] productionIDs = { 1011, 2022, 3033 };
			_instrumentationSimpleProviderMock
				.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<ExportProductionDocumentImageResponse[]>>>()))
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

		private void AssertIfResponsesAreSameAsExpected(FileResponse[] expectedResponses, FileResponse[] currentResponses)
		{
			expectedResponses.Length.Should().Be(currentResponses.Length);

			var asserts = expectedResponses.Zip(currentResponses, (e, a) => new
			{
				Expected = e,
				Actual = a
			});

			foreach (var assert in asserts)
			{
				FileResponse actual = assert.Actual;
				FileResponse expected = assert.Expected;

				actual.DocumentArtifactID.Should().Be(expected.DocumentArtifactID);
				actual.Filename.Should().Be(expected.Filename);
				actual.Guid.Should().Be(expected.Guid);
				actual.Identifier.Should().Be(expected.Identifier);
				actual.Location.Should().Be(expected.Location);
				actual.Order.Should().Be(expected.Order);
				actual.Rotation.Should().Be(expected.Rotation);
				actual.Type.Should().Be(expected.Type);
				actual.InRepository.Should().Be(expected.InRepository);
				actual.Size.Should().Be(expected.Size);
				actual.Details.Should().Be(expected.Details);
				actual.Billable.Should().Be(expected.Billable);
			}
		}

		private void AssertIfResponsesAreSameAsExpected(
			ProductionDocumentImageResponse[] expectedResponses,
			ProductionDocumentImageResponse[] currentResponses)
		{
			expectedResponses.Length.Should().Be(currentResponses.Length);

			var asserts = expectedResponses.Zip(currentResponses, (e, a) => new
			{
				Expected = e,
				Actual = a
			});

			foreach (var assert in asserts)
			{
				ProductionDocumentImageResponse actual = assert.Actual;
				ProductionDocumentImageResponse expected = assert.Expected;

				actual.DocumentArtifactID.Should().Be(expected.DocumentArtifactID);
				actual.SourceGuid.Should().Be(expected.SourceGuid);
				actual.BatesNumber.Should().Be(expected.BatesNumber);
				actual.ImageSize.Should().Be(expected.ImageSize);
				actual.ImageGuid.Should().Be(expected.ImageGuid);
				actual.ImageFileName.Should().Be(expected.ImageFileName);
				actual.Location.Should().Be(expected.Location);
				actual.PageID.Should().Be(expected.PageID);
				actual.ByteRange.Should().Be(expected.ByteRange);
				actual.NativeIdentifier.Should().Be(expected.NativeIdentifier);
			}
		}

		private void AssertIfResponsesAreSameAsExpected(DocumentImageResponse[] expectedResponses, DocumentImageResponse[] currentResponses)
		{
			expectedResponses.Length.Should().Be(currentResponses.Length);

			var asserts = expectedResponses.Zip(currentResponses, (e, a) => new
			{
				Expected = e,
				Actual = a
			});

			foreach (var assert in asserts)
			{
				DocumentImageResponse actual = assert.Actual;
				DocumentImageResponse expected = assert.Expected;

				actual.DocumentArtifactID.Should().Be(expected.DocumentArtifactID);
				actual.FileID.Should().Be(expected.FileID);
				actual.FileName.Should().Be(expected.FileName);
				actual.Guid.Should().Be(expected.Guid);
				actual.Identifier.Should().Be(expected.Identifier);
				actual.Location.Should().Be(expected.Location);
				actual.Order.Should().Be(expected.Order);
				actual.Rotation.Should().Be(expected.Rotation);
				actual.Type.Should().Be(expected.Type);
				actual.InRepository.Should().Be(expected.InRepository);
				actual.Size.Should().Be(expected.Size);
				actual.Details.Should().Be(expected.Details);
				actual.Billable.Should().Be(expected.Billable);
				actual.PageID.Should().Be(expected.PageID);
				actual.ByteRange.Should().Be(expected.ByteRange);
			}
		}

		private void AssertIfResponsesAreSameAsExpected(
			ExportProductionDocumentImageResponse[] expectedResponses,
			ExportProductionDocumentImageResponse[] currentResponses)
		{
			expectedResponses.Length.Should().Be(currentResponses.Length);

			var asserts = expectedResponses.Zip(currentResponses, (e, a) => new
			{
				Expected = e,
				Actual = a
			});

			foreach (var assert in asserts)
			{
				ExportProductionDocumentImageResponse actual = assert.Actual;
				ExportProductionDocumentImageResponse expected = assert.Expected;

				actual.DocumentArtifactID.Should().Be(expected.DocumentArtifactID);
				actual.ProductionArtifactID.Should().Be(expected.ProductionArtifactID);
				actual.SourceGuid.Should().Be(expected.SourceGuid);
				actual.BatesNumber.Should().Be(expected.BatesNumber);
				actual.ImageSize.Should().Be(expected.ImageSize);
				actual.ImageGuid.Should().Be(expected.ImageGuid);
				actual.ImageFileName.Should().Be(expected.ImageFileName);
				actual.Location.Should().Be(expected.Location);
				actual.PageID.Should().Be(expected.PageID);
				actual.ByteRange.Should().Be(expected.ByteRange);
				actual.Order.Should().Be(expected.Order);
			}
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
				x => x.ExecuteAsync(It.IsAny<Func<Task<T>>>()),
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
