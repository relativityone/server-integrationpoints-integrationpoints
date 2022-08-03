using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Facades.SecretStore.Implementation
{
    [TestFixture, Category("Unit")]
    public class SecretStoreFacadeRetryDecoratorTests
    {
        private SecretStoreFacadeRetryDecorator _sut;
        private Mock<ISecretStoreFacade> _secretStoreFacadeMock;

        private const string _TEST_SECRET_STORE_PATH = "testPath/101/202";

        [SetUp]
        public void SetUp()
        {
            var retryHandler = new RetryHandler(null, 1, 0);
            var retryHandlerFactory = new Mock<IRetryHandlerFactory>();
            retryHandlerFactory
                .Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>()))
                .Returns(retryHandler);

            _secretStoreFacadeMock = new Mock<ISecretStoreFacade>();

            _sut = new SecretStoreFacadeRetryDecorator(
                _secretStoreFacadeMock.Object, 
                retryHandlerFactory.Object
            );
        }

        [Test]
        public async Task GetAsync_ShouldReturnResultAndRetryOnFailures()
        {
            // arrange
            var expectedResult = new Secret();

            _secretStoreFacadeMock.SetupSequence(x => x.GetAsync(It.IsAny<string>()))
                .Throws<InvalidOperationException>()
                .ReturnsAsync(expectedResult);

            // act
            Secret result = await _sut
                .GetAsync(_TEST_SECRET_STORE_PATH)
                .ConfigureAwait(false);

            // assert
            result.Should().Be(expectedResult);
        }

        [Test]
        public void SetAsync_ShouldReturnResultAndRetryOnFailures()
        {
            // arrange
            var secret = new Secret();

            _secretStoreFacadeMock
                .SetupSequence(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Secret>()))
                .Throws<InvalidOperationException>();

            // act
            Func<Task> setAction = () => _sut.SetAsync(_TEST_SECRET_STORE_PATH, secret);

            // assert
            setAction.ShouldNotThrow();
        }

        [Test]
        public void DeleteAsync_ShouldReturnResultAndRetryOnFailures()
        {
            // arrange
            _secretStoreFacadeMock
                .SetupSequence(x => x.DeleteAsync(It.IsAny<string>()))
                .Throws<InvalidOperationException>();

            // act
            Func<Task> setAction = () => _sut.DeleteAsync(_TEST_SECRET_STORE_PATH);

            // assert
            setAction.ShouldNotThrow();
        }
    }
}
