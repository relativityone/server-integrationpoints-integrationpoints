using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation;
using kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation.TestsHelpers;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation
{
    [TestFixture, Category("Unit")]
    public class ObjectManagerFacadeRetryDecoratorTests
    {
        private ObjectManagerFacadeRetryDecorator _sut;
        private Mock<IObjectManagerFacade> _objectManager;

        [SetUp]
        public void SetUp()
        {
            var retryHandler = new RetryHandler(null, 1, 0);
            var retryHandlerFactory = new Mock<IRetryHandlerFactory>();
            retryHandlerFactory
                .Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>()))
                .Returns(retryHandler);

            _objectManager = new Mock<IObjectManagerFacade>();

            _sut = new ObjectManagerFacadeRetryDecorator(_objectManager.Object, retryHandlerFactory.Object);
        }

        [Test]
        public Task CreateAsync_ShouldReturnResultAndRetryOnFailures()
        {
            return ShouldReturnResultAndRetryOnFailureAsync(ObjectManagerFacadeTestsHelpers.CreateCallWithAnyArgs);
        }

        [Test]
        public Task ReadAsync_ShouldReturnResultAndRetryOnFailures()
        {
            return ShouldReturnResultAndRetryOnFailureAsync(ObjectManagerFacadeTestsHelpers.ReadCallWithAnyArgs);
        }

        [Test]
        public Task UpdateAsync_ShouldReturnResultAndRetryOnFailures()
        {
            return ShouldReturnResultAndRetryOnFailureAsync(ObjectManagerFacadeTestsHelpers.UpdateCallWithAnyArgs);
        }

        [Test]
        public Task UpdateAsync_MassUpdate_ShouldReturnResultAndRetryOnFailures()
        {
            return ShouldReturnResultAndRetryOnFailureAsync(ObjectManagerFacadeTestsHelpers.MassUpdateCallWithAnyArgs);
        }

        [Test]
        public Task DeleteAsync_ShouldReturnResultAndRetryOnFailures()
        {
            return ShouldReturnResultAndRetryOnFailureAsync(ObjectManagerFacadeTestsHelpers.DeleteCallWithAnyArgs);
        }

        [Test]
        public Task QueryAsync_ShouldReturnResultAndRetryOnFailures()
        {
            return ShouldReturnResultAndRetryOnFailureAsync(ObjectManagerFacadeTestsHelpers.QueryCallWithAnyArgs);
        }

        [Test]
        public void This_ShouldDisposeObjectManagerFacade()
        {
            // act
            _sut.Dispose();

            // assert
            _objectManager.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void This_ShouldDisposeObjectMangerFacadeOnlyOnce()
        {
            // act
            _sut.Dispose();
            _sut.Dispose();

            // assert
            _objectManager.Verify(x => x.Dispose(), Times.Once);
        }

        private async Task ShouldReturnResultAndRetryOnFailureAsync<TResult>(Expression<Func<IObjectManagerFacade, Task<TResult>>> methodToTest)
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
            TResult result = await compiledMethodToTest(_sut).ConfigureAwait(false);

            // assert
            result.Should().Be(expectedResult);
        }
    }
}
