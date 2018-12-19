using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades;
using kCura.IntegrationPoints.Data.Facades.Implementations;
using kCura.IntegrationPoints.Data.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Tests.Facades.Implementations
{
	[TestFixture]
	public class ObjectManagerFacadeWithRetriesTests
	{
		private ObjectManagerFacadeWithRetries _sut;
		private Mock<IObjectManagerFacade> _objectManager;

		private const int _WORKSPACE_ID = 3232;

		[SetUp]
		public void SetUp()
		{
			var retryHandler = new RetryHandler(null, 1, 0);
			var retryHandlerFactory = new Mock<IRetryHandlerFactory>();
			retryHandlerFactory
				.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>()))
				.Returns(retryHandler);

			_objectManager = new Mock<IObjectManagerFacade>();

			_sut = new ObjectManagerFacadeWithRetries(_objectManager.Object, retryHandlerFactory.Object);
		}

		[Test]
		public async Task CreateShouldReturnResultAndRetryOnFailures()
		{
			var request = new CreateRequest();
			await ShouldReturnResultAndRetryOnFailure(x => x.CreateAsync(_WORKSPACE_ID, request));
		}

		[Test]
		public async Task ReadShouldReturnResultAndRetryOnFailures()
		{
			var request = new ReadRequest();
			await ShouldReturnResultAndRetryOnFailure(x => x.ReadAsync(_WORKSPACE_ID, request));
		}

		[Test]
		public async Task UpdateShouldReturnResultAndRetryOnFailures()
		{
			var request = new UpdateRequest();
			await ShouldReturnResultAndRetryOnFailure(x => x.UpdateAsync(_WORKSPACE_ID, request));
		}

		[Test]
		public async Task DeleteShouldReturnResultAndRetryOnFailures()
		{
			var request = new DeleteRequest();
			await ShouldReturnResultAndRetryOnFailure(x => x.DeleteAsync(_WORKSPACE_ID, request));
		}

		[Test]
		public async Task QueryShouldReturnResultAndRetryOnFailures()
		{
			var request = new QueryRequest();
			await ShouldReturnResultAndRetryOnFailure(x => x.QueryAsync(_WORKSPACE_ID, request, 0, 0));
		}

		[Test]
		public void ShouldDisposeObjectManagerFacade()
		{
			// act
			_sut.Dispose();

			// assert
			_objectManager.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void ShouldDisposedObjectMangerFacadeOnlyOnce()
		{
			// act
			_sut.Dispose();
			_sut.Dispose();

			// assert
			_objectManager.Verify(x => x.Dispose(), Times.Once);
		}

		private async Task ShouldReturnResultAndRetryOnFailure<TResult>(Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest)
			where TResult : new()
		{
			// arrange
			var expectedResult = new TResult();
			Task<TResult> expectedResultTask = Task.FromResult(expectedResult);

			_objectManager.SetupSequence(methodToTest)
				.Throws<InvalidOperationException>()
				.Returns(expectedResultTask);

			Func<IObjectManagerFacade, Task<TResult>> compiledMethodToTest = methodToTest.Compile();

			// act
			TResult result = await compiledMethodToTest(_sut);

			// assert
			result.Should().Be(expectedResult);
		}
	}
}
