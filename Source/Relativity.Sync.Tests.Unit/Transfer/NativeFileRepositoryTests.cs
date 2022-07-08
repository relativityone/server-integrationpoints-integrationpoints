using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Objects;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal sealed class NativeFileRepositoryTests
	{
		private Mock<ISearchService> _searchService;
		private Mock<IObjectManager> _objectManager;

		private NativeFileRepository _instance;
		private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUser;

		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _FILENAME_COLUMN_NAME = "Filename";
		private const string _SIZE_COLUMN_NAME = "Size";

		[SetUp]
		public void SetUp()
		{
			_searchService = new Mock<ISearchService>();
			_serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			_objectManager = new Mock<IObjectManager>();

			_serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_serviceFactoryForUser.Setup(x => x.CreateProxyAsync<ISearchService>()).ReturnsAsync(_searchService.Object);

			SyncJobParameters parameters = new SyncJobParameters(0, 0, 0, Guid.NewGuid());

			_instance = new NativeFileRepository(_serviceFactoryForUser.Object, new EmptyLogger(), parameters);
		}

		[Test]
		public void ItShouldThrowProperExceptionWhenGetNativesFails()
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = new[] { 0, 1 };

			_searchService.Setup(x => x.RetrieveNativesForSearchAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
				.Throws(new InvalidOperationException());

			// Act
			Action action = () => _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldCallGetNativesWithCorrectArguments()
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = new[] { 0, 1 };

			_searchService.Setup(x => x.RetrieveNativesForSearchAsync(workspaceArtifactId, It.Is<string>(ids => ids == string.Join(",", documentIds)), It.IsAny<string>()))
				.ReturnsAsync(CreateSampleDataSet)
				.Verifiable();

			// Act
			await _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false);

			// Assert
			_searchService.Verify();
		}

		[Test]
		public async Task ItShouldShortCircuitOnEmptyDocumentIds()
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = Array.Empty<int>();

			// Act
			IEnumerable<INativeFile> results = await _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false);

			// Assert
			results.Should().BeEmpty();
			_searchService.Verify(x => x.RetrieveNativesForSearchAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public async Task ItShouldShortCircuitOnNullDocumentIds()
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = null;

			// Act
			IEnumerable<INativeFile> results = await _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false);

			// Assert
			results.Should().BeEmpty();
			_searchService.Verify(x => x.RetrieveNativesForSearchAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

#pragma warning restore RG2009 // Hardcoded Numeric Value


		private static IEnumerable<TestCaseData> TransformResponsesCorrectlyCases()
		{
			yield return new TestCaseData(new DataSetWrapper(new DataSet()), Enumerable.Empty<INativeFile>())
			{
				TestName = "Empty file response"
			};

			yield return new TestCaseData(null, Enumerable.Empty<INativeFile>())
			{
				TestName = "Null file response"
			};

			DataSetWrapper response = CreateSampleDataSet();

			NativeFile[] expectedNativeFiles =
			{
				new NativeFile(123, @"\\test1\test2", "test3.txt", 11),
				new NativeFile(456, @"\\test2\test3", "test5.txt", 12),
				new NativeFile(789, @"\\test3\test4", "test6.html", 13)
			};

			yield return new TestCaseData(response, expectedNativeFiles)
			{
				TestName = "Proper file response transformation"
			};
		}

		private static DataSetWrapper CreateSampleDataSet() => CreateSampleDataSet(null);

		private static DataSetWrapper CreateSampleDataSet(IEnumerable<INativeFile> natives)
		{
			natives = natives ?? new List<INativeFile>
			{
				new NativeFile(123, @"\\test1\test2",  "test3.txt", 11),
				new NativeFile(456, @"\\test2\test3",  "test5.txt", 12),
				new NativeFile(789, @"\\test3\test4",  "test6.html", 13)
			};

			DataTable dataTable = new DataTable("Table");
			dataTable.Columns.AddRange(new[]
			{
				new DataColumn(_DOCUMENT_ARTIFACT_ID_COLUMN_NAME, typeof(int)),
				new DataColumn(_LOCATION_COLUMN_NAME, typeof(string)),
				new DataColumn(_FILENAME_COLUMN_NAME, typeof(string)),
				new DataColumn(_SIZE_COLUMN_NAME, typeof(long))
			});

			foreach (var nativeFile in natives)
			{
				dataTable.Rows.Add(CreateRow(dataTable, nativeFile.DocumentArtifactId, nativeFile.Location, nativeFile.Filename, nativeFile.Size));
			}

			DataSet dataSet = new DataSet();
			dataSet.Tables.Add(dataTable);
			return new DataSetWrapper(dataSet);
		}

		private static DataRow CreateRow(DataTable table, int id, string location, string fileName, long size)
		{
			DataRow row = table.NewRow();
			row[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME] = id;
			row[_LOCATION_COLUMN_NAME] = location;
			row[_FILENAME_COLUMN_NAME] = fileName;
			row[_SIZE_COLUMN_NAME] = size;
			return row;
		}

#pragma warning restore RG2009 // With the exception of zero and one, never hard-code a numeric value; always declare a constant instead

		[TestCaseSource(nameof(TransformResponsesCorrectlyCases))]
		public async Task ItShouldTransformResponsesCorrectly(DataSetWrapper response, IEnumerable<INativeFile> expected)
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = new[] { 0, 1 };

			_searchService.Setup(x => x.RetrieveNativesForSearchAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(response);

			// Act
			IEnumerable<INativeFile> actual = await _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false);

			// Assert
			List<INativeFile> expectedList = expected.ToList();
			List<INativeFile> actualList = actual.ToList();

			expectedList.Count.Should().Be(actualList.Count);
			expectedList.Zip(actualList, AreEqual).All(x => x).Should().Be(true);
		}

		[Test]
		public async Task ItShouldIgnoreInputLengthWhenReturningResults()
		{
			// Arrange
			const int workspaceArtifactId = 123;
			const int numDocumentIds = 10;
			ICollection<int> documentIds = Enumerable.Range(0, numDocumentIds).ToList();

			const int numFileResponses = 3;
			_searchService.Setup(x => x.RetrieveNativesForSearchAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(CreateSampleDataSet);

			// Act
			IEnumerable<INativeFile> actual = await _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false);

			// Assert
			actual.Count().Should().Be(numFileResponses);
		}

		private static bool AreEqual(INativeFile me, INativeFile you)
		{
			return
				me != null && you != null &&
				me.DocumentArtifactId == you.DocumentArtifactId &&
				me.Location == you.Location &&
				me.Filename == you.Filename &&
				me.Size == you.Size;
		}
	}
}
