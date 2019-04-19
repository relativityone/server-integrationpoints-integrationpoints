using System;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class DisposableFileRepositoryTests
	{
		private Mock<IFileRepository> _fileRepositoryMock;
		private Mock<IServicesMgr> _servicesMgrMock;
		private Mock<IFileManager> _fileManagerMock;
		private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
		private Mock<DisposableFileRepository.CreateFileRepositoryDelegate> _createFileRepositoryDelegateMock;

		private DisposableFileRepository _sut;

		private const int _WORKSPACE_ID = 1001;
		private readonly int[] _PRODUCTION_IDS = { 1001, 2002, 3003 };
		private readonly int[] _DOCUMENT_IDS = { 101, 202, 303 };

		[SetUp]
		public void Setup()
		{
			_fileRepositoryMock = new Mock<IFileRepository>();
			_fileManagerMock = new Mock<IFileManager>();
			_servicesMgrMock = new Mock<IServicesMgr>();
			_servicesMgrMock
				.Setup(_ => _.CreateProxy<IFileManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_fileManagerMock.Object);
			_instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
			_createFileRepositoryDelegateMock = new Mock<DisposableFileRepository.CreateFileRepositoryDelegate>();
			_createFileRepositoryDelegateMock.Setup(_ => _(
				It.IsAny<IFileManager>(),
				It.IsAny<IExternalServiceInstrumentationProvider>()
			)).Returns(_fileRepositoryMock.Object);

			_sut = new DisposableFileRepository(
				_servicesMgrMock.Object,
				_instrumentationProviderMock.Object,
				_createFileRepositoryDelegateMock.Object
			);
		}

		[Test]
		public void GetNativesForSearch_ShouldReturnResponsesFromInnerFileRepository()
		{
			//arrange
			FileResponse[] responses = { new FileResponse(), new FileResponse() };
			_fileRepositoryMock
				.Setup(_ => _.GetNativesForSearch(_WORKSPACE_ID, _DOCUMENT_IDS))
				.Returns(responses);

			//arrange
			FileResponse[] result = _sut.GetNativesForSearch(_WORKSPACE_ID, _DOCUMENT_IDS);
			
			//assert
			_fileRepositoryMock.Verify(
				_ => _.GetNativesForSearch(_WORKSPACE_ID, _DOCUMENT_IDS), 
				Times.Once
			);

			result.Length.Should().Be(responses.Length);
			result.Should().Contain(responses);
		}

		[Test]
		public void GetNativesForSearch_ShouldNotThrowWhenDocumentIDsPassedAsNull()
		{
			//arrange
			Action action = () =>_sut.GetNativesForSearch(
				_WORKSPACE_ID, 
				documentIDs: null
			);

			//assert
			action.ShouldNotThrow();
		}

		[Test]
		public void GetNativesForSearch_ShouldCallDisposeOnFileManagerProxy()
		{
			//arrange
			FileResponse[] result = _sut.GetNativesForSearch(_WORKSPACE_ID, _DOCUMENT_IDS);

			//assert
			_fileManagerMock.Verify(
				_ => _.Dispose(),
				Times.Once
			);
		}

		[Test]
		public void GetNativesForProduction_ShouldReturnResponsesFromInnerFileRepository()
		{
			//arrange
			FileResponse[] responses = { new FileResponse(), new FileResponse() };
			int productionID = _PRODUCTION_IDS.First();
			_fileRepositoryMock
				.Setup(_ => _.GetNativesForProduction(_WORKSPACE_ID, productionID, _DOCUMENT_IDS))
				.Returns(responses);

			//arrange
			FileResponse[] result = _sut.GetNativesForProduction(_WORKSPACE_ID, productionID, _DOCUMENT_IDS);

			//assert
			_fileRepositoryMock.Verify(
				_ => _.GetNativesForProduction(_WORKSPACE_ID, productionID, _DOCUMENT_IDS),
				Times.Once
			);

			result.Length.Should().Be(responses.Length);
			result.Should().Contain(responses);
		}

		[Test]
		public void GetNativesForProduction_ShouldNotThrowWhenDocumentIDsPassedAsNull()
		{
			//arrange
			Action action = () => _sut.GetNativesForProduction(
				_WORKSPACE_ID,
				_PRODUCTION_IDS.First(),
				documentIDs: null
			);

			//assert
			action.ShouldNotThrow();
		}

		[Test]
		public void GetNativesForProduction_ShouldCallDisposeOnFileManagerProxy()
		{
			//arrange
			FileResponse[] result = _sut.GetNativesForProduction(
				_WORKSPACE_ID,
				_PRODUCTION_IDS.First(),
				_DOCUMENT_IDS);

			//assert
			_fileManagerMock.Verify(
				_ => _.Dispose(),
				Times.Once
			);
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldReturnResponsesFromInnerFileRepository()
		{
			//arrange
			ProductionDocumentImageResponse[] responses =
			{
				new ProductionDocumentImageResponse(),
				new ProductionDocumentImageResponse()
			};
			int productionID = _PRODUCTION_IDS.First();
			_fileRepositoryMock
				.Setup(_ => _.GetImagesForProductionDocuments(_WORKSPACE_ID, productionID, _DOCUMENT_IDS))
				.Returns(responses);

			//arrange
			ProductionDocumentImageResponse[] result = _sut.GetImagesForProductionDocuments(_WORKSPACE_ID, productionID, _DOCUMENT_IDS);

			//assert
			_fileRepositoryMock.Verify(
				_ => _.GetImagesForProductionDocuments(_WORKSPACE_ID, productionID, _DOCUMENT_IDS),
				Times.Once
			);

			result.Length.Should().Be(responses.Length);
			result.Should().Contain(responses);
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldNotThrowWhenDocumentIDsPassedAsNull()
		{
			//arrange
			Action action = () => _sut.GetImagesForProductionDocuments(
				_WORKSPACE_ID,
				_PRODUCTION_IDS.First(),
				documentIDs: null
			);

			//assert
			action.ShouldNotThrow();
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldCallDisposeOnFileManagerProxy()
		{
			//arrange
			ProductionDocumentImageResponse[] result = _sut.GetImagesForProductionDocuments(
				_WORKSPACE_ID,
				_PRODUCTION_IDS.First(),
				_DOCUMENT_IDS);

			//assert
			_fileManagerMock.Verify(
				_ => _.Dispose(),
				Times.Once
			);
		}

		[Test]
		public void GetImagesForDocuments_ShouldReturnResponsesFromInnerFileRepository()
		{
			//arrange
			DocumentImageResponse[] responses =
			{
				new DocumentImageResponse(),
				new DocumentImageResponse()
			};
			_fileRepositoryMock
				.Setup(_ => _.GetImagesForDocuments(_WORKSPACE_ID, _DOCUMENT_IDS))
				.Returns(responses);

			//arrange
			DocumentImageResponse[] result = _sut.GetImagesForDocuments(_WORKSPACE_ID, _DOCUMENT_IDS);

			//assert
			_fileRepositoryMock.Verify(
				_ => _.GetImagesForDocuments(_WORKSPACE_ID, _DOCUMENT_IDS),
				Times.Once
			);

			result.Length.Should().Be(responses.Length);
			result.Should().Contain(responses);
		}

		[Test]
		public void GetImagesForDocuments_ShouldNotThrowWhenDocumentIDsPassedAsNull()
		{
			//arrange
			Action action = () => _sut.GetImagesForDocuments(
				_WORKSPACE_ID,
				documentIDs: null
			);

			//assert
			action.ShouldNotThrow();
		}

		[Test]
		public void GetImagesForDocuments_ShouldCallDisposeOnFileManagerProxy()
		{
			//arrange
			DocumentImageResponse[] result = _sut.GetImagesForDocuments(
				_WORKSPACE_ID,
				_DOCUMENT_IDS);

			//assert
			_fileManagerMock.Verify(
				_ => _.Dispose(),
				Times.Once
			);
		}

		[Test]
		public void GetProducedImagesForDocument_ShouldReturnResponsesFromInnerFileRepository()
		{
			//arrange
			FileResponse[] responses =
			{
				new FileResponse(),
				new FileResponse()
			};
			int documentID = _DOCUMENT_IDS.First();
			_fileRepositoryMock
				.Setup(_ => _.GetProducedImagesForDocument(_WORKSPACE_ID, documentID))
				.Returns(responses);

			//arrange
			FileResponse[] result = _sut.GetProducedImagesForDocument(_WORKSPACE_ID, documentID);

			//assert
			_fileRepositoryMock.Verify(
				_ => _.GetProducedImagesForDocument(_WORKSPACE_ID, documentID),
				Times.Once
			);

			result.Length.Should().Be(responses.Length);
			result.Should().Contain(responses);
		}

		[Test]
		public void GetProducedImagesForDocument_ShouldCallDisposeOnFileManagerProxy()
		{
			//arrange
			FileResponse[] result = _sut.GetProducedImagesForDocument(
				_WORKSPACE_ID,
				_DOCUMENT_IDS.First());

			//assert
			_fileManagerMock.Verify(
				_ => _.Dispose(),
				Times.Once
			);
		}

		[Test]
		public void GetImagesForExport_ShouldReturnResponsesFromInnerFileRepository()
		{
			//arrange
			ExportProductionDocumentImageResponse[] responses =
			{
				new ExportProductionDocumentImageResponse(),
				new ExportProductionDocumentImageResponse()
			};
			_fileRepositoryMock
				.Setup(_ => _.GetImagesForExport(_WORKSPACE_ID, _PRODUCTION_IDS, _DOCUMENT_IDS))
				.Returns(responses);

			//arrange
			ExportProductionDocumentImageResponse[] result = _sut.GetImagesForExport(
				_WORKSPACE_ID, 
				_PRODUCTION_IDS, 
				_DOCUMENT_IDS
			);

			//assert
			_fileRepositoryMock.Verify(
				_ => _.GetImagesForExport(_WORKSPACE_ID, _PRODUCTION_IDS, _DOCUMENT_IDS),
				Times.Once
			);

			result.Length.Should().Be(responses.Length);
			result.Should().Contain(responses);
		}

		[Test]
		public void GetImagesForExport_ShouldNotThrowWhenDocumentIDsPassedAsNull()
		{
			//arrange
			Action action = () => _sut.GetImagesForExport(
				_WORKSPACE_ID,
				_PRODUCTION_IDS,
				documentIDs: null
			);

			//assert
			action.ShouldNotThrow();
		}

		[Test]
		public void GetImagesForExport_ShouldNotThrowWhenProductionIDsPassedAsNull()
		{
			//arrange
			Action action = () => _sut.GetImagesForExport(
				_WORKSPACE_ID,
				productionIDs: null,
				documentIDs: _DOCUMENT_IDS
			);

			//assert
			action.ShouldNotThrow();
		}

		[Test]
		public void GetImagesForExport_ShouldCallDisposeOnFileManagerProxy()
		{
			//arrange
			ExportProductionDocumentImageResponse[] result = _sut.GetImagesForExport(
				_WORKSPACE_ID,
				_PRODUCTION_IDS,
				_DOCUMENT_IDS
			);

			//assert
			_fileManagerMock.Verify(
				_ => _.Dispose(),
				Times.Once
			);
		}
	}
}
