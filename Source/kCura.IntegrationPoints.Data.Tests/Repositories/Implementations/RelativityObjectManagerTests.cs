using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.StreamWrappers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class RelativityObjectManagerTests
	{
		private Mock<IAPILog> _apiLogMock;
		private Mock<IObjectManagerFacadeFactory> _objectManagerFacadeFactoryMock;
		private Mock<IObjectManagerFacade> _objectManagerFacadeMock;
		private RelativityObjectManager _sut;

		private const int _WORKSPACE_ARTIFACT_ID = 12345;
		private const int _REL_OBJECT_ARTIFACT_ID = 10;
		private const int _FIELD_ARTIFACT_ID = 789;

		[SetUp]
		public void SetUp()
		{
			_apiLogMock = new Mock<IAPILog>();
			_apiLogMock.Setup(x => x.ForContext<RelativityObjectManager>()).Returns(_apiLogMock.Object);
			_apiLogMock.Setup(x => x.ForContext<SelfDisposingStream>()).Returns(_apiLogMock.Object);
			_apiLogMock.Setup(x => x.ForContext<SelfRecreatingStream>()).Returns(_apiLogMock.Object);
			_objectManagerFacadeMock = new Mock<IObjectManagerFacade>();
			_objectManagerFacadeFactoryMock = new Mock<IObjectManagerFacadeFactory>();
			_objectManagerFacadeFactoryMock
				.Setup(x => x.Create(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerFacadeMock.Object);
			_sut = new RelativityObjectManager(
				_WORKSPACE_ARTIFACT_ID,
				_apiLogMock.Object,
				_objectManagerFacadeFactoryMock.Object);
		}

		[Test]
		public void StreamUnicodeLongText_ItShouldRethrowIntegrationPointException()
		{
			// arrange
			_objectManagerFacadeMock
				.Setup(x => 
					x.StreamLongTextAsync(
						It.IsAny<int>(), 
						It.IsAny<RelativityObjectRef>(), 
						It.IsAny<FieldRef>()))
				.Throws<IntegrationPointsException>();

			//act
			Action action = () => 
				_sut.StreamUnicodeLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef {ArtifactID = _FIELD_ARTIFACT_ID},
					ExecutionIdentity.System);

			// assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void StreamNonUnicodeLongText_ItShouldRethrowIntegrationPointException()
		{
			// arrange
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.Throws<IntegrationPointsException>();

			// act
			Action action = () =>
				_sut.StreamNonUnicodeLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef { ArtifactID = _FIELD_ARTIFACT_ID },
					ExecutionIdentity.System);

			// assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void StreamUnicodeLongText_ItShouldThrowExceptionWrappedInIntegrationPointException()
		{
			// arrange
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.Throws<Exception>();

			// act
			Action action = () => 
				_sut.StreamUnicodeLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef() {ArtifactID = _FIELD_ARTIFACT_ID},
					ExecutionIdentity.System);

			// assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void StreamNonUnicodeLongText_ItShouldThrowExceptionWrappedInIntegrationPointException()
		{
			// arrange
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.Throws<Exception>();

			// act
			Action action = () =>
				_sut.StreamNonUnicodeLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
					ExecutionIdentity.System);

			// assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void StreamUnicodeLongText_ItShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade()
		{
			// arrange
			Stream expectedStream = new Mock<Stream>().Object;
			var keplerStreamMock = new Mock<IKeplerStream>();
			keplerStreamMock.Setup(x => x.GetStreamAsync()).ReturnsAsync(expectedStream);
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.ReturnsAsync(keplerStreamMock.Object);

			// act
			Stream result = _sut.StreamUnicodeLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef() {ArtifactID = _FIELD_ARTIFACT_ID},
					ExecutionIdentity.System);

			// assert
			result.Should().BeOfType<SelfDisposingStream>();
			var selfDisposingStream = (SelfDisposingStream) result;
			Stream innerStream = selfDisposingStream.InnerStream;
			innerStream.Should().BeOfType<SelfRecreatingStream>();
			var selfRecreatingStream = (SelfRecreatingStream) innerStream;
			selfRecreatingStream.InnerStream.Should().Be(expectedStream);
		}

		[Test]
		public void StreamNonUnicodeLongText_ItShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade()
		{
			// arrange
			Stream expectedStream = new Mock<Stream>().Object;
			var keplerStreamMock = new Mock<IKeplerStream>();
			keplerStreamMock.Setup(x => x.GetStreamAsync()).ReturnsAsync(expectedStream);
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.ReturnsAsync(keplerStreamMock.Object);

			// act
			Stream result = _sut.StreamNonUnicodeLongText(
				_REL_OBJECT_ARTIFACT_ID,
				new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
				ExecutionIdentity.System);

			// assert
			result.Should().BeOfType<SelfDisposingStream>();
			var selfDisposingStream = (SelfDisposingStream) result;
			Stream innerStream1 = selfDisposingStream.InnerStream;
			innerStream1.Should().BeOfType<AsciiToUnicodeStream>();
			var asciiToUnicodeStream = (AsciiToUnicodeStream) innerStream1;
			Stream innerStream2 = asciiToUnicodeStream.AsciiStream;
			innerStream2.Should().BeOfType<SelfRecreatingStream>();
			var selfRecreatingStream = (SelfRecreatingStream) innerStream2;
			selfRecreatingStream.InnerStream.Should().Be(expectedStream);
		}

		[Test]
		public void MassUpdateAsync_ShouldRethrowIntegrationPointException()
		{
			// arrange
			var expectedException = new IntegrationPointsException();
			_objectManagerFacadeMock
				.Setup(x =>
					x.UpdateAsync(
						It.IsAny<int>(),
						It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
						It.IsAny<MassUpdateOptions>()))
				.Throws(expectedException);

			// act
			Func<Task> massUpdateAction = () =>
				_sut.MassUpdateAsync(
					Enumerable.Empty<int>(),
					It.IsAny<IEnumerable<FieldRefValuePair>>(),
					It.IsAny<FieldUpdateBehavior>(),
					It.IsAny<ExecutionIdentity>());

			// assert
			massUpdateAction.ShouldThrow<IntegrationPointsException>()
				.Which.Should().Be(expectedException);
		}

		[Test]
		public void MassUpdateAsync_ShouldWrapExceptionInIntegrationPointException()
		{
			// arrange
			var expectedInnerException = new Exception();
			_objectManagerFacadeMock
				.Setup(x =>
					x.UpdateAsync(
						It.IsAny<int>(),
						It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
						It.IsAny<MassUpdateOptions>()))
				.Throws(expectedInnerException);

			// act
			Func<Task> massUpdateAction = () =>
				_sut.MassUpdateAsync(
					Enumerable.Empty<int>(),
					It.IsAny<IEnumerable<FieldRefValuePair>>(),
					It.IsAny<FieldUpdateBehavior>(),
					It.IsAny<ExecutionIdentity>());

			// assert
			massUpdateAction.ShouldThrow<IntegrationPointsException>()
				.Which.InnerException.Should().Be(expectedInnerException);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task MassUpdateAsync_ShouldReturnValueFromObjectManagerFacade(bool isSuccess)
		{
			// arrange
			var massUpdateResult = new MassUpdateResult
			{
				Success = isSuccess
			};
			_objectManagerFacadeMock
				.Setup(x =>
					x.UpdateAsync(
						It.IsAny<int>(),
						It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
						It.IsAny<MassUpdateOptions>()))
				.ReturnsAsync(massUpdateResult);

			// act
			bool actualResult = await _sut.MassUpdateAsync(
					Enumerable.Empty<int>(),
					It.IsAny<IEnumerable<FieldRefValuePair>>(),
					It.IsAny<FieldUpdateBehavior>(),
					It.IsAny<ExecutionIdentity>())
				.ConfigureAwait(false);

			// assert
			actualResult.Should().Be(isSuccess);
		}

		[Test]
		public async Task MassUpdateAsync_ShouldSendProperRequest()
		{
			// arrange
			var massUpdateResult = new MassUpdateResult
			{
				Success = true
			};

			IList<int> objectIDs = Enumerable.Range(0, 5).ToList();

			FieldRefValuePair[] fields =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						ArtifactID = 1
					},
					Value = "one"
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						ArtifactID = 2
					},
					Value = "two"
				}
			};

			FieldUpdateBehavior updateBehavior = FieldUpdateBehavior.Merge;

			_objectManagerFacadeMock
				.Setup(x =>
					x.UpdateAsync(
						It.IsAny<int>(),
						It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
						It.IsAny<MassUpdateOptions>()))
				.ReturnsAsync(massUpdateResult);

			// act
			await _sut.MassUpdateAsync(
					objectIDs,
					fields,
					updateBehavior,
					It.IsAny<ExecutionIdentity>())
				.ConfigureAwait(false);

			// assert
			Func<MassUpdateByObjectIdentifiersRequest, bool> requestVerifier = request =>
			{
				bool isValid = true;
				isValid &= request.Objects.Select(x => x.ArtifactID).SequenceEqual(objectIDs);
				isValid &= request.FieldValues.SequenceEqual(fields);
				return isValid;
			};

			Func<MassUpdateOptions, bool> updateOptionsVerifier = options =>
				options.UpdateBehavior == updateBehavior;

			_objectManagerFacadeMock.Verify(x => x.UpdateAsync(
				_WORKSPACE_ARTIFACT_ID,
				It.Is<MassUpdateByObjectIdentifiersRequest>(request => requestVerifier(request)),
				It.Is<MassUpdateOptions>(options => updateOptionsVerifier(options)))
			);
		}

		[Test]
		public async Task InitializeExportAsync_ItShouldReturnSameResultAsFacade()
		{
			// arrange
			var queryRequest = new QueryRequest();
			const int start = 6;
			var expectedResult = new ExportInitializationResults();
			_objectManagerFacadeMock
				.Setup(x =>
					x.InitializeExportAsync(
						_WORKSPACE_ARTIFACT_ID,
						queryRequest,
						start))
				.ReturnsAsync(expectedResult);

			// act
			ExportInitializationResults actualResult = await _sut.InitializeExportAsync(
					queryRequest,
					start,
					ExecutionIdentity.System)
				.ConfigureAwait(false);

			// assert
			_objectManagerFacadeMock.Verify(x => x.InitializeExportAsync(
				_WORKSPACE_ARTIFACT_ID,
				queryRequest,
				start));
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public void InitializeExportAsync_ItShouldRethrowIntegrationPointException()
		{
			// arrange
			var queryRequest = new QueryRequest();
			const int start = 6;
			_objectManagerFacadeMock
				.Setup(x =>
					x.InitializeExportAsync(
						_WORKSPACE_ARTIFACT_ID,
						queryRequest,
						start))
				.Throws<IntegrationPointsException>();

			// act
			Func<Task> action = () =>
				_sut.InitializeExportAsync(
					queryRequest,
					start,
					ExecutionIdentity.System);

			// assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void InitializeExportAsync_ItShouldThrowExceptionWrappedInIntegrationPointException()
		{
			// arrange
			var queryRequest = new QueryRequest();
			const int start = 6;
			_objectManagerFacadeMock
				.Setup(x =>
					x.InitializeExportAsync(
						_WORKSPACE_ARTIFACT_ID,
						queryRequest,
						start))
				.Throws<Exception>();

			// act
			Func<Task> action = () =>
				_sut.InitializeExportAsync(
					queryRequest,
					start,
					ExecutionIdentity.System);

			// assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public async Task RetrieveResultsBlockFromExport_ShouldCallFacadeOnce_WhenEntireBlockIsReturned()
		{
			// arrange
			Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
			const int resultsBlockSize = 10;
			const int exportIndexID = 0;
			RelativityObjectSlim[] expectedResult = CreateTestRelativityObjectsSlim(resultsBlockSize);
			_objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					resultsBlockSize,
					exportIndexID))
				.ReturnsAsync(expectedResult);

			// act
			RelativityObjectSlim[] actualResult = await _sut.RetrieveResultsBlockFromExportAsync(
					runID,
					resultsBlockSize,
					exportIndexID,
					ExecutionIdentity.System)
				.ConfigureAwait(false);

			// assert
			_objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					resultsBlockSize,
					exportIndexID),
				Times.Once);
			actualResult.ShouldBeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
		}

		[Test]
		public async Task RetrieveResultsBlockFromExport_ShouldCallFacadeTwoTimes_WhenBlockIsReturnedInHalves()
		{
			// arrange
			Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
			const int resultsBlockSize = 10;
			const int returnedSize = 5;
			const int exportIndexID = 0;
			const int expectedCallsCount = 2;
			RelativityObjectSlim[] expectedResult = CreateTestRelativityObjectsSlim(resultsBlockSize);
			_objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					resultsBlockSize,
					exportIndexID))
				.ReturnsAsync(GetRelativityObjectSlimArrayPart(expectedResult, 0, returnedSize));
			_objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					resultsBlockSize - returnedSize,
					exportIndexID + returnedSize))
				.ReturnsAsync(GetRelativityObjectSlimArrayPart(expectedResult, returnedSize, returnedSize));

			// act
			RelativityObjectSlim[] actualResult = await _sut.RetrieveResultsBlockFromExportAsync(
					runID,
					resultsBlockSize,
					exportIndexID,
					ExecutionIdentity.System)
				.ConfigureAwait(false);

			// assert
			_objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					It.IsAny<int>(),
					It.IsAny<int>()),
				Times.Exactly(expectedCallsCount));
			actualResult.ShouldBeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
		}

		[Test]
		public async Task RetrieveResultsBlockFromExport_ShouldCallFacadThreeTimes_WhenBlockIsReturnedInUnevenParts()
		{
			// arrange
			Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
			const int resultsBlockSize = 10;
			const int returnedSize = 4;
			const int exportIndexID = 0;
			const int expectedCallsCount = 3;
			RelativityObjectSlim[] expectedResult = CreateTestRelativityObjectsSlim(resultsBlockSize);
			_objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					resultsBlockSize,
					exportIndexID))
				.ReturnsAsync(GetRelativityObjectSlimArrayPart(expectedResult, 0, returnedSize));
			_objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					resultsBlockSize - returnedSize,
					exportIndexID + returnedSize))
				.ReturnsAsync(GetRelativityObjectSlimArrayPart(expectedResult, returnedSize, returnedSize));
			_objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					resultsBlockSize - 2 * returnedSize,
					exportIndexID + 2 * returnedSize))
				.ReturnsAsync(GetRelativityObjectSlimArrayPart(
					expectedResult, 
					2 * returnedSize,
					resultsBlockSize - 2 * returnedSize));

			// act
			RelativityObjectSlim[] actualResult = await _sut.RetrieveResultsBlockFromExportAsync(
					runID,
					resultsBlockSize,
					exportIndexID,
					ExecutionIdentity.System)
				.ConfigureAwait(false);

			// assert
			_objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
					_WORKSPACE_ARTIFACT_ID,
					runID,
					It.IsAny<int>(),
					It.IsAny<int>()),
				Times.Exactly(expectedCallsCount));
			actualResult.ShouldBeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
		}

		[Test]
		public void RetrieveResultsBlockFromExportAsync_ItShouldRethrowIntegrationPointException()
		{
			Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
			const int resultsBlockSize = 6;
			const int exportIndexID = 0;
			_objectManagerFacadeMock
				.Setup(x =>
					x.RetrieveResultsBlockFromExportAsync(
						_WORKSPACE_ARTIFACT_ID,
						runID,
						resultsBlockSize,
						exportIndexID))
				.Throws<IntegrationPointsException>();

			Func<Task> action = () =>
				_sut.RetrieveResultsBlockFromExportAsync(
						runID,
						resultsBlockSize,
						exportIndexID,
						ExecutionIdentity.System);

			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void RetrieveResultsBlockFromExportAsync_ItShouldThrowExceptionWrappedInIntegrationPointException()
		{
			Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
			const int resultsBlockSize = 6;
			const int exportIndexID = 0;
			_objectManagerFacadeMock
				.Setup(x =>
					x.RetrieveResultsBlockFromExportAsync(
						_WORKSPACE_ARTIFACT_ID,
						runID,
						resultsBlockSize,
						exportIndexID))
				.Throws<Exception>();

			Func<Task> action = () =>
				_sut.RetrieveResultsBlockFromExportAsync(
						runID,
						resultsBlockSize,
						exportIndexID,
						ExecutionIdentity.System);

			action.ShouldThrow<IntegrationPointsException>();
		}

		private static RelativityObjectSlim[] CreateTestRelativityObjectsSlim(int size)
		{
			var objects = new RelativityObjectSlim[size];
			int iterator = 1;
			for (int i = 0; i < size; ++i)
			{
				int artifactID = ++iterator;
				var values = new List<object> { ++iterator, ++iterator, ++iterator, ++iterator };
				var objectSlim = new RelativityObjectSlim
				{
					ArtifactID = artifactID,
					Values = values
				};
				objects[i] = objectSlim;
			}
			return objects;
		}

		private static RelativityObjectSlim[] GetRelativityObjectSlimArrayPart(RelativityObjectSlim[] originalArray, int start, int length)
		{
			var result = new RelativityObjectSlim[length];
			Array.Copy(originalArray, start, result, 0, length);
			return result;
		}
	}
}
