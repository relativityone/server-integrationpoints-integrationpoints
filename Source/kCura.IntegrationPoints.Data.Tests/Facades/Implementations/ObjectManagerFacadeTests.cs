using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Facades.Implementations
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
			var result = new CreateResult();

			_objectManagerMock.Setup(x => x.CreateAsync(
					It.IsAny<int>(),
					It.IsAny<CreateRequest>()))
				.ReturnsAsync(result);

			//act
			CreateResult actualResult = await _sut.CreateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
		}

		[Test]
		public async Task ReadAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var request = new ReadRequest();
			var result = new ReadResult();

			_objectManagerMock.Setup(x => x.ReadAsync(
					It.IsAny<int>(),
					It.IsAny<ReadRequest>()))
				.ReturnsAsync(result);

			//act
			ReadResult actualResult = await _sut.ReadAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
		}

		[Test]
		public async Task UpdateAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var request = new UpdateRequest();
			var result = new UpdateResult();

			_objectManagerMock.Setup(x => x.UpdateAsync(
					It.IsAny<int>(),
					It.IsAny<UpdateRequest>()))
				.ReturnsAsync(result);

			//act
			UpdateResult actualResult = await _sut.UpdateAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
		}

		[Test]
		public async Task QueryAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			const int start = 0;
			const int length = 1;
			var request = new QueryRequest();
			var result = new QueryResult();

			_objectManagerMock.Setup(x => x.QueryAsync(
					It.IsAny<int>(),
					It.IsAny<QueryRequest>(),
					It.IsAny<int>(),
					It.IsAny<int>()))
				.ReturnsAsync(result);

			//act
			QueryResult actualResult = await _sut.QueryAsync(workspaceId, request, start, length);

			//assert
			result.Should().Be(actualResult);
		}

		[Test]
		public async Task DeleteAsync_ShouldReturnSameResultAsObjectManager()
		{
			//arrange
			const int workspaceId = 101;
			var request = new DeleteRequest();
			var result = new DeleteResult();

			_objectManagerMock.Setup(x => x.DeleteAsync(
					It.IsAny<int>(),
					It.IsAny<DeleteRequest>()))
				.ReturnsAsync(result);

			//act
			DeleteResult actualResult = await _sut.DeleteAsync(workspaceId, request);

			//assert
			result.Should().Be(actualResult);
		}

		[Test]
		public void CreateAsync_ShouldThrowWhenObjectManagerNotInitialized()
		{
			//arrange
			const int workspaceId = 101;
			var request = new CreateRequest();

			_sut = new ObjectManagerFacade(() => null);

			//act
			Func<Task> action = async () => await _sut.CreateAsync(workspaceId, request);

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
			Func<Task> action = async () => await _sut.ReadAsync(workspaceId, request);

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
			Func<Task> action = async () => await _sut.UpdateAsync(workspaceId, request);

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
			Func<Task> action = async () => await _sut.QueryAsync(workspaceId, request, start, length);

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
			Func<Task> action = async () => await _sut.DeleteAsync(workspaceId, request);

			//assert
			action.ShouldThrow<NullReferenceException>();
		}

		[Test]
		public async Task This_ShouldDisposeObjectManagerWhenItsAlreadyCreated()
		{
			//arrange
			const int workspaceId = 101;
			ReadRequest request = new ReadRequest();
			await _sut.ReadAsync(workspaceId, request);

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
			await _sut.ReadAsync(workspaceId, request);

			//act
			_sut.Dispose();
			_sut.Dispose();

			//assert
			_objectManagerMock.Verify(x => x.Dispose(), Times.Once);
		}
	}
}
