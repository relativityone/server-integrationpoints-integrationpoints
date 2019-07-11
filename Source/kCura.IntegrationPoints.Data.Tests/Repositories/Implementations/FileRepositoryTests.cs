﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Handlers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.WinEDDS.Service.Export;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class FileRepositoryTests
	{
		private Mock<ISearchManager> _searchManagerMock;
		private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
		private Mock<IExternalServiceSimpleInstrumentation> _instrumentationSimpleProviderMock;
		private Mock<IRetryHandler> _retryHandlerMock;

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
			_retryHandlerMock = new Mock<IRetryHandler>();
			_instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
			_instrumentationSimpleProviderMock = new Mock<IExternalServiceSimpleInstrumentation>();
			_instrumentationProviderMock
				.Setup(x => x.CreateSimple(
					_KEPLER_SERVICE_TYPE,
					_KEPLER_SERVICE_NAME,
					It.IsAny<string>()))
				.Returns(_instrumentationSimpleProviderMock.Object);

			_sut = new FileRepository(() => _searchManagerMock.Object, _instrumentationProviderMock.Object, _retryHandlerMock.Object );
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
			List<string> result = _sut.GetImagesLocationForProductionDocuments(
				_WORKSPACE_ID,
				productionID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DataSet>(
				operationName: nameof(ISearchManager.RetrieveImagesForProductionDocuments)
			);
			AssertIfListsAreSameAsExpected(
				_testProductionDocumentImageResponses.Select(x => x.Location).ToList(),
				result
			);
		}

		[Test]
		public void GetImagesForProductionDocuments_ShouldThrowWhenNullPassedAsDocumentIDs()
		{
			//arrange
			const int productionID = 1111;

			//act
			Action action = () => _sut.GetImagesLocationForProductionDocuments(
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
			List<string> result = _sut.GetImagesLocationForProductionDocuments(
				_WORKSPACE_ID,
				productionID,
				documentIDs: new int[] { }
			);

			//assert
			result.Should().BeEmpty();
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
			Action action = () => _sut.GetImagesLocationForProductionDocuments(
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
			List<string> result = _sut.GetImagesLocationForDocuments(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DataSet>(
				operationName: nameof(ISearchManager.RetrieveImagesForDocuments)
			);
			AssertIfListsAreSameAsExpected(
				_testDocumentImageResponses.Select(x => x.Location).ToList(),
				result
			);
		}

		[Test]
		public void GetImagesForDocuments_ShouldThrowWhenNullPassedAsDocumentIDs()
		{
			//act
			Action action = () => _sut.GetImagesLocationForDocuments(
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
			List<string> result = _sut.GetImagesLocationForDocuments(
				_WORKSPACE_ID,
				documentIDs: new int[] { }
			);

			//assert
			result.Should().BeEmpty();
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
			Action action = () => _sut.GetImagesLocationForDocuments(
				_WORKSPACE_ID,
				documentIDs
			);

			//assert
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
	}
}
