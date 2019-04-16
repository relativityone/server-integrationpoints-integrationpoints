using System;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.Services.FileField;
using Relativity.Services.FileField.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Repositories
{
	[TestFixture]
	public class FileFieldRepositoryTests
	{
		private Mock<IFileFieldManager> _fileFieldManagerMock;
		private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
		private Mock<IExternalServiceSimpleInstrumentation> _instrumentationSimpleProviderMock;

		private FileFieldRepository _sut;

		private const int _WORKSPACE_ID = 1001000;
		private const string _KEPLER_SERVICE_TYPE = "Kepler";
		private const string _KEPLER_SERVICE_NAME = nameof(IFileFieldManager);

		private readonly DynamicFileResponse[] _testDynamicFileResponses =
		{
			new DynamicFileResponse
			{
				FileID = 123,
				ObjectArtifactID = 12,
				FileName = "TestFileName1",
				Location = "Location11",
				Size = 23455,
			},
			new DynamicFileResponse
			{
				FileID = 321,
				ObjectArtifactID = 121,
				FileName = "TestFileName12",
				Location = "Location112",
				Size = 234551,
			}
		};

		[SetUp]
		public void SetUp()
		{
			_fileFieldManagerMock = new Mock<IFileFieldManager>();
			_instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
			_instrumentationSimpleProviderMock = new Mock<IExternalServiceSimpleInstrumentation>();
			_instrumentationProviderMock
				.Setup(x => x.CreateSimple(
					_KEPLER_SERVICE_TYPE,
					_KEPLER_SERVICE_NAME,
					It.IsAny<string>()))
				.Returns(_instrumentationSimpleProviderMock.Object);

			_sut = new FileFieldRepository(_fileFieldManagerMock.Object, _instrumentationProviderMock.Object);
		}

		[Test]
		public void GetFilesForDynamicObjects_ShouldReturnResponsesWhenCorrectObjectIDsPassed()
		{
			//arrange
			const int fileFieldID = 1002;
			int[] objectIDs = _testDynamicFileResponses.Select(x => x.ObjectArtifactID).ToArray();
			_instrumentationSimpleProviderMock
				.Setup(x => x.Execute(It.IsAny<Func<DynamicFileResponse[]>>()))
				.Returns(_testDynamicFileResponses);

			//act
			DynamicFileResponse[] result = _sut.GetFilesForDynamicObjects(
				_WORKSPACE_ID,
				fileFieldID,
				objectIDs
			);

			//assert
			VerifyIfInstrumentationHasBeenCalled<DynamicFileResponse[]>(
				operationName: nameof(IFileFieldManager.GetFilesForDynamicObjectsAsync)
			);
			AssertIfResponsesAreSameAsExpected(_testDynamicFileResponses, result);
		}

		[Test]
		public void GetFilesForDynamicObjects_ShouldThrowWhenNullPassedAsObjectIDs()
		{
			//arrange
			const int fileFieldID = 1002;

			//act
			Action action = () => _sut.GetFilesForDynamicObjects(
				_WORKSPACE_ID,
				fileFieldID,
				objectIDs: null
			);

			//assert
			action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: objectIDs");
			VerifyIfInstrumentationHasNeverBeenCalled<DynamicFileResponse[]>(
				operationName: nameof(IFileFieldManager.GetFilesForDynamicObjectsAsync)
			);
		}

		[Test]
		public void GetFilesForDynamicObjects_ShouldReturnEmptyArrayWhenEmptyArrayPassedAsObjectIDs()
		{
			//arrange
			const int fileFieldID = 1002;

			//act
			DynamicFileResponse[] result = _sut.GetFilesForDynamicObjects(
				_WORKSPACE_ID,
				fileFieldID,
				objectIDs: new int[] { }
			);

			//assert
			result.Should().BeEmpty();
			VerifyIfInstrumentationHasNeverBeenCalled<DynamicFileResponse[]>(
				operationName: nameof(IFileFieldManager.GetFilesForDynamicObjectsAsync)
			);
		}


		private void AssertIfResponsesAreSameAsExpected(
			DynamicFileResponse[] expectedResponses, 
			DynamicFileResponse[] currentResponses)
		{
			expectedResponses.Length.Should().Be(currentResponses.Length);

			var asserts = expectedResponses.Zip(currentResponses, (e, a) => new
			{
				Expected = e,
				Actual = a
			});

			foreach (var assert in asserts)
			{
				DynamicFileResponse actual = assert.Actual;
				DynamicFileResponse expected = assert.Expected;

				actual.FileID.Should().Be(expected.FileID);
				actual.ObjectArtifactID.Should().Be(expected.ObjectArtifactID);
				actual.FileName.Should().Be(expected.FileName);
				actual.Location.Should().Be(expected.Location);
				actual.Size.Should().Be(expected.Size);
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
				x => x.Execute(It.IsAny<Func<T>>()),
				Times.Never
			);
		}
	}
}
