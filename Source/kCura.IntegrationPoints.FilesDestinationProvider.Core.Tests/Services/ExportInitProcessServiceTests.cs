
using System;
using System.ComponentModel;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Services
{
	class ExportInitProcessServiceTests
	{
		private ExportInitProcessService _subjectUnderTests;
		private IHelper _helperMock;
		private IRepositoryFactory _repositoryFactoryMock;
		private ExportUsingSavedSearchSettings _exportSettings;
		private IDocumentTotalsRepository _documentTotalsRepository;
		private IAPILog _loggerMock;

		private const int _WKSP_ID = 1;
		private const int _VIEW_ID = 2;
		private const int _FOLDER_ID = 3;
		private const int _SAVED_SEARCH_ID = 4;
		private const int _PROD_SET_ID = 5;

		private const int _EXPECTED_DOC_COUNT = 10;

		[SetUp]
		public void Init()
		{
			_exportSettings = new ExportUsingSavedSearchSettings
			{
				SourceWorkspaceArtifactId = _WKSP_ID,
				SavedSearchArtifactId = _SAVED_SEARCH_ID,
				FolderArtifactId = _FOLDER_ID,
				ProductionId = _PROD_SET_ID,
				ViewId = _VIEW_ID,
				StartExportAtRecord = 1
			};
			_helperMock = Substitute.For<IHelper>();
			_repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
			_documentTotalsRepository = Substitute.For<IDocumentTotalsRepository>();
			_loggerMock = Substitute.For<IAPILog>();

			ILogFactory logFactoryMock = Substitute.For<ILogFactory>();
			IAPILog apiLog = Substitute.For<IAPILog>();

			_helperMock.GetLoggerFactory().Returns(logFactoryMock);
			logFactoryMock.GetLogger().Returns(apiLog);

			apiLog.ForContext<ExportInitProcessService>().Returns(_loggerMock);

			_repositoryFactoryMock.GetDocumentTotalsRepository(_WKSP_ID).Returns(_documentTotalsRepository);

			_subjectUnderTests = new ExportInitProcessService(_helperMock, _repositoryFactoryMock);
		}

		[Test]
		public void ItShouldReturnCorrectSavedSearchDocCountNumber()
		{
			// Arrange
			_exportSettings.ExportType = ExportSettings.ExportType.SavedSearch.ToString();

			_documentTotalsRepository.GetSavedSearchTotalDocsCount(_SAVED_SEARCH_ID).Returns(_EXPECTED_DOC_COUNT);

			// Act
			int returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings);

			// Assert
			_documentTotalsRepository.Received().GetSavedSearchTotalDocsCount(_SAVED_SEARCH_ID);
			Assert.That(returnedValue, Is.EqualTo(_EXPECTED_DOC_COUNT));
		}

		[Test]
		public void ItShouldReturnCorrectProductionDocCountNumber()
		{
			// Arrange
			_exportSettings.ExportType = ExportSettings.ExportType.ProductionSet.ToString();

			_documentTotalsRepository.GetProductionDocsCount(_PROD_SET_ID).Returns(_EXPECTED_DOC_COUNT);

			// Act
			int returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings);

			// Assert
			_documentTotalsRepository.Received().GetProductionDocsCount(_PROD_SET_ID);
			Assert.That(returnedValue, Is.EqualTo(_EXPECTED_DOC_COUNT));
		}

		[Test]
		[TestCase(ExportSettings.ExportType.Folder, false)]
		[TestCase(ExportSettings.ExportType.FolderAndSubfolders, true)]
		public void ItShouldReturnCorrectSavedSearchDocCountNumber(ExportSettings.ExportType exportType, bool includeSubFolders)
		{
			// Arrange
			_exportSettings.ExportType = exportType.ToString();

			_documentTotalsRepository.GetFolderTotalDocsCount(_FOLDER_ID, _VIEW_ID, includeSubFolders).Returns(_EXPECTED_DOC_COUNT);

			// Act
			int returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings);

			// Assert
			_documentTotalsRepository.Received().GetFolderTotalDocsCount(_FOLDER_ID, _VIEW_ID, includeSubFolders);
			Assert.That(returnedValue, Is.EqualTo(_EXPECTED_DOC_COUNT));
		}

		[Test]
		[TestCase(11, 0)]
		[TestCase(1, _EXPECTED_DOC_COUNT)]
		[TestCase(10, 1)]
		[TestCase(4, 7)]
		public void ItShouldConsiderStartIndexAtParam(int startAtIndex, int expectedCount)
		{
			// Arrange
			_exportSettings.StartExportAtRecord = startAtIndex;
			_exportSettings.ExportType = ExportSettings.ExportType.SavedSearch.ToString();

			_documentTotalsRepository.GetSavedSearchTotalDocsCount(_SAVED_SEARCH_ID).Returns(_EXPECTED_DOC_COUNT);

			// Act
			int returnedValue = _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings);

			// Assert
			Assert.That(returnedValue, Is.EqualTo(expectedCount));
		}

		[Test]
		public void ItShouldThrowExeptiononUknownExportType()
		{
			// Arrange
			_exportSettings.ExportType = "SomeType";

			// Act & Assert
			InvalidEnumArgumentException thrownException = Assert.Throws<InvalidEnumArgumentException>(() => _subjectUnderTests.CalculateDocumentCountToTransfer(_exportSettings));
			_loggerMock.Received().LogError(thrownException, Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}
