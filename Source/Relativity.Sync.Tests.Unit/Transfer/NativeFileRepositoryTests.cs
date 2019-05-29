using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal sealed class NativeFileRepositoryTests
	{
		private Mock<IFileManager> _fileManager;
		private Mock<ISourceServiceFactoryForUser> _sourceServiceFactory;
		private NativeFileRepository _instance;

		[SetUp]
		public void SetUp()
		{
			_fileManager = new Mock<IFileManager>();
			_sourceServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			_sourceServiceFactory.Setup(x => x.CreateProxyAsync<IFileManager>())
				.ReturnsAsync(_fileManager.Object);
			_instance = new NativeFileRepository(_sourceServiceFactory.Object);
		}

		[Test]
		public void ItShouldThrowProperExceptionWhenCreateProxyFails()
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = new[] { 0, 1 };

			_sourceServiceFactory.Setup(x => x.CreateProxyAsync<IFileManager>())
				.Throws(new ServiceException());

			// Act
			Action action = () => _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			action.Should().Throw<ServiceException>();
		}

		[Test]
		public void ItShouldThrowProperExceptionWhenGetNativesFails()
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = new[] { 0, 1 };

			_fileManager.Setup(x => x.GetNativesForSearchAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.Throws(new ServiceException());

			// Act
			Action action = () => _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false).GetAwaiter().GetResult();

			// Assert
			action.Should().Throw<ServiceException>();
		}

		[Test]
		public async Task ItShouldCallGetNativesWithCorrectArguments()
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = new[] { 0, 1 };

			_fileManager.Setup(x => x.GetNativesForSearchAsync(workspaceArtifactId, It.Is<int[]>(ids => documentIds.SequenceEqual(ids))))
				.ReturnsAsync(Array.Empty<FileResponse>())
				.Verifiable();

			// Act
			await _instance.QueryAsync(workspaceArtifactId, documentIds).ConfigureAwait(false);

			// Assert
			_fileManager.Verify();
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
			_fileManager.Verify(x => x.GetNativesForSearchAsync(It.IsAny<int>(), It.IsAny<int[]>()), Times.Never);
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
			_fileManager.Verify(x => x.GetNativesForSearchAsync(It.IsAny<int>(), It.IsAny<int[]>()), Times.Never);
		}

#pragma warning disable RG2009 // With the exception of zero and one, never hard-code a numeric value; always declare a constant instead

		private static IEnumerable<TestCaseData> TransformResponsesCorrectlyCases()
		{
			yield return new TestCaseData(Array.Empty<FileResponse>(), Enumerable.Empty<INativeFile>())
			{
				TestName = "Empty file responses"
			};

			yield return new TestCaseData(null, Enumerable.Empty<INativeFile>())
			{
				TestName = "Null file responses"
			};

			FileResponse[] fileResponses =
			{
				new FileResponse { DocumentArtifactID = 123, Location = @"\\test1\test2", Filename = "test3.txt", Size = 101L },
				new FileResponse { DocumentArtifactID = 456, Location = @"\\test2\test3", Filename = "test5.txt", Size = 1010L },
				new FileResponse { DocumentArtifactID = 789, Location = @"\\test3\test4", Filename = "test6.html", Size = 231123L }
			};
			NativeFile[] expectedNativeFiles =
			{
				new NativeFile(123, @"\\test1\test2", "test3.txt", 101L),
				new NativeFile(456, @"\\test2\test3", "test5.txt", 1010L),
				new NativeFile(789, @"\\test3\test4", "test6.html", 231123L)
			};
			yield return new TestCaseData(fileResponses, expectedNativeFiles)
			{
				TestName = "Proper file response transformation"
			};
		}

#pragma warning restore RG2009 // With the exception of zero and one, never hard-code a numeric value; always declare a constant instead

		[TestCaseSource(nameof(TransformResponsesCorrectlyCases))]
		public async Task ItShouldTransformResponsesCorrectly(FileResponse[] responses, IEnumerable<INativeFile> expected)
		{
			// Arrange
			const int workspaceArtifactId = 123;
			ICollection<int> documentIds = new[] { 0, 1 };

			_fileManager.Setup(x => x.GetNativesForSearchAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync(responses);

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

			const int numFileResponses = 5;
			FileResponse[] responses = Enumerable.Range(0, numFileResponses)
				.Select(i => new FileResponse { DocumentArtifactID = i })
				.ToArray();
			_fileManager.Setup(x => x.GetNativesForSearchAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync(responses);

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
