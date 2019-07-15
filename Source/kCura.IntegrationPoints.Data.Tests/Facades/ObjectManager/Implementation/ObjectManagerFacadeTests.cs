using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation;
using Moq;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation
{
	[TestFixture]
	public class ObjectManagerFacadeTests
	{
		private Mock<IObjectManager> _objectManagerMock;
		private ObjectManagerFacade _sut;

		[SetUp]
		public void SetUp()
		{
			_objectManagerMock = new Mock<IObjectManager>();
			_sut = new ObjectManagerFacade(() => _objectManagerMock.Object);
		}

		[Test]
		public async Task CreateAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var request = new CreateRequest();
			var expectedResult = new CreateResult();

			_objectManagerMock.Setup(x => x.CreateAsync(
					It.IsAny<int>(),
					It.IsAny<CreateRequest>()))
				.ReturnsAsync(expectedResult);

			//act
			CreateResult actualResult = await _sut
				.CreateAsync(workspaceId, request)
				.ConfigureAwait(false);

			//assert
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public async Task ReadAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var request = new ReadRequest();
			var expectedResult = new ReadResult();

			_objectManagerMock.Setup(x => x.ReadAsync(
					It.IsAny<int>(),
					It.IsAny<ReadRequest>()))
				.ReturnsAsync(expectedResult);

			//act
			ReadResult actualResult = await _sut
				.ReadAsync(workspaceId, request)
				.ConfigureAwait(false);

			//assert
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public async Task UpdateAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var request = new UpdateRequest();
			var expectedResult = new UpdateResult();

			_objectManagerMock.Setup(x => x.UpdateAsync(
					It.IsAny<int>(),
					It.IsAny<UpdateRequest>()))
				.ReturnsAsync(expectedResult);

			//act
			UpdateResult actualResult = await _sut
				.UpdateAsync(workspaceId, request)
				.ConfigureAwait(false);

			//assert
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public async Task QueryAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			const int start = 0;
			const int length = 1;
			var request = new QueryRequest();
			var expectedResult = new QueryResult();

			_objectManagerMock.Setup(x => x.QueryAsync(
					It.IsAny<int>(),
					It.IsAny<QueryRequest>(),
					It.IsAny<int>(),
					It.IsAny<int>()))
				.ReturnsAsync(expectedResult);

			//act
			QueryResult actualResult = await _sut
				.QueryAsync(workspaceId, request, start, length)
				.ConfigureAwait(false);

			//assert
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public async Task DeleteAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var request = new DeleteRequest();
			var expectedResult = new DeleteResult();

			_objectManagerMock.Setup(x => x.DeleteAsync(
					It.IsAny<int>(),
					It.IsAny<DeleteRequest>()))
				.ReturnsAsync(expectedResult);

			//act
			DeleteResult actualResult = await _sut
				.DeleteAsync(workspaceId, request)
				.ConfigureAwait(false);

			//assert
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public async Task StreamLongTextAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var relativityObjectRef = new RelativityObjectRef();
			var fieldRef = new FieldRef();
			IKeplerStream expectedResult = new Mock<IKeplerStream>().Object;

			_objectManagerMock.Setup(x => x.StreamLongTextAsync(
					It.IsAny<int>(),
					It.IsAny<RelativityObjectRef>(),
					It.IsAny<FieldRef>()))
				.ReturnsAsync(expectedResult);

			//act
			IKeplerStream actualResult = await _sut
				.StreamLongTextAsync(workspaceId, relativityObjectRef, fieldRef)
				.ConfigureAwait(false);

			//assert
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public async Task InitializeExportAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceID = 101;
			var queryRequest = new QueryRequest();
			const int start = 5;
			ExportInitializationResults expectedResult = new ExportInitializationResults();

			_objectManagerMock.Setup(x => x.InitializeExportAsync(
					workspaceID,
					queryRequest,
					start))
				.ReturnsAsync(expectedResult);

			//act
			ExportInitializationResults actualResult = await _sut
				.InitializeExportAsync(workspaceID, queryRequest, start)
				.ConfigureAwait(false);

			//assert
			_objectManagerMock.Verify(x => x.InitializeExportAsync(workspaceID, queryRequest, start), Times.Once);
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public async Task RetrieveResultsBlockFromExportAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceID = 101;
			Guid runID = Guid.Parse("EA150180-3A58-4DFF-AA6C-6385075FCFD3");
			const int resultsBlockSize = 5;
			const int exportIndexID = 0;
			RelativityObjectSlim relativityObjectSlim = new RelativityObjectSlim();
			RelativityObjectSlim[] expectedResult = {relativityObjectSlim};

			_objectManagerMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
					workspaceID,
					runID,
					resultsBlockSize,
					exportIndexID))
				.ReturnsAsync(expectedResult);

			//act
			RelativityObjectSlim[] actualResult = await _sut
				.RetrieveResultsBlockFromExportAsync(workspaceID, runID, resultsBlockSize, exportIndexID)
				.ConfigureAwait(false);

			//assert
			_objectManagerMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(workspaceID, runID, resultsBlockSize, exportIndexID), Times.Once);
			actualResult.Should().BeSameAs(expectedResult);
		}

		[Test]
		public void CreateAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceId = 101;
			var request = new CreateRequest();

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = async () => await _sut
				.CreateAsync(workspaceId, request)
				.ConfigureAwait(false);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public void ReadAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceId = 101;
			var request = new ReadRequest();

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = async () => await _sut
				.ReadAsync(workspaceId, request)
				.ConfigureAwait(false);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public void UpdateAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceId = 101;
			var request = new UpdateRequest();

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = async () => await _sut
				.UpdateAsync(workspaceId, request)
				.ConfigureAwait(false);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public void QueryAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceId = 101;
			const int start = 0;
			const int length = 1;
			var request = new QueryRequest();

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = async () => await _sut
				.QueryAsync(workspaceId, request, start, length)
				.ConfigureAwait(false);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public void DeleteAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceId = 101;
			var request = new DeleteRequest();

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = async () => await _sut
				.DeleteAsync(workspaceId, request)
				.ConfigureAwait(false);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public void StreamLongTextAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceArtifactID = 101;
			var relativityObjectRef = new RelativityObjectRef();
			var fieldRef = new FieldRef();

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = async () => await _sut
				.StreamLongTextAsync(workspaceArtifactID, relativityObjectRef, fieldRef)
				.ConfigureAwait(false);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public async Task UpdateAsync_MassUpdate_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var request = new MassUpdateByObjectIdentifiersRequest();
			var options = new MassUpdateOptions();
			var expectedResult = new MassUpdateResult();

			_objectManagerMock
				.Setup(x => x.UpdateAsync(workspaceId, request, options))
				.ReturnsAsync(expectedResult);

			//act
			MassUpdateResult actualResult = await _sut
				.UpdateAsync(workspaceId, request, options)
				.ConfigureAwait(false);

			//assert
			actualResult.Should().Be(expectedResult);
		}

		[Test]
		public void InitializeExportAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceID = 101;
			var queryRequest = new QueryRequest();
			const int start = 5;

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = () => _sut
				.InitializeExportAsync(workspaceID, queryRequest, start);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public void UpdateAsync_MassUpdate_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceId = 101;
			var request = new MassUpdateByObjectIdentifiersRequest();
			var options = new MassUpdateOptions();
			var exceptionToThrow = new Exception();

			_objectManagerMock
				.Setup(x => x.UpdateAsync(workspaceId, request, options))
				.Throws(exceptionToThrow);

			//act
			Func<Task> massUpdateAction = () => _sut.UpdateAsync(workspaceId, request, options);

			//assert
			massUpdateAction.ShouldThrow<Exception>()
				.Which.Should().Be(exceptionToThrow);
		}
		
		[Test]
		public void RetrieveResultsBlockFromExportAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceID = 101;
			Guid runID = Guid.Parse("EA150180-3A58-4DFF-AA6C-6385075FCFD3");
			const int resultsBlockSize = 5;
			const int exportIndexID = 0;

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = () => _sut
				.RetrieveResultsBlockFromExportAsync(workspaceID, runID, resultsBlockSize, exportIndexID);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public async Task This_ShouldDisposeObjectManagerWhenItsAlreadyCreated()
		{
			//arrange
			const int workspaceId = 101;
			ReadRequest request = new ReadRequest();
			await _sut
				.ReadAsync(workspaceId, request)
				.ConfigureAwait(false);

			//act
			_sut.Dispose();

			//assert
			_objectManagerMock.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void This_ShouldNotDisposeObjectManagerWhenItsNotCreated()
		{
			//act
			_sut.Dispose();

			//assert
			_objectManagerMock.Verify(x => x.Dispose(), Times.Never);
		}

		[Test]
		public async Task This_ShouldDisposeObjectManagerOnlyOnceWhenItsAlreadyCreated()
		{
			//arrange
			const int workspaceId = 101;
			ReadRequest request = new ReadRequest();
			await _sut
				.ReadAsync(workspaceId, request)
				.ConfigureAwait(false);

			//act
			_sut.Dispose();
			_sut.Dispose();

			//assert
			_objectManagerMock.Verify(x => x.Dispose(), Times.Once);
		}
	}
}
