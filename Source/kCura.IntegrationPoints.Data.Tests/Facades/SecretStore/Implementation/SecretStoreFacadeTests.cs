using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Facades.SecretStore.Implementation
{
    [TestFixture, Category("Unit")]
    public class SecretStoreFacadeTests
    {
        private Mock<ISecretStore> _secretStoreMock;
        private SecretStoreFacade _sut;
        private const string _TEST_SECRET_STORE_PATH = "testPath/101/202";

        [SetUp]
        public void SetUp()
        {
            _secretStoreMock = new Mock<ISecretStore>();
            var secretStoreLazy = new Lazy<ISecretStore>(
                () => _secretStoreMock.Object
            );
            _sut = new SecretStoreFacade(secretStoreLazy);
        }

        [Test]
        public async Task GetAsync_ShouldReturnSameResultAsSecretStore()
        {
            // arrange
            var expectedSecret = new Secret();

            _secretStoreMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedSecret);

            // act
            Secret actualSecret = await _sut
                .GetAsync(_TEST_SECRET_STORE_PATH)
                .ConfigureAwait(false);

            // assert
            actualSecret.Should().Be(expectedSecret);
            _secretStoreMock.Verify(x => x.GetAsync(_TEST_SECRET_STORE_PATH), Times.Once);
        }

        [Test]
        public async Task SetAsync_ShouldCallSecretStoreOnce()
        {
            // arrange
            var secret = new Secret();

            // act
            await _sut.SetAsync(_TEST_SECRET_STORE_PATH, secret).ConfigureAwait(false);

            // assert
            _secretStoreMock
                .Verify(x => x.SetAsync(_TEST_SECRET_STORE_PATH, secret), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_ShouldCallSecretStoreOnce()
        {
            // act
            await _sut.DeleteAsync(_TEST_SECRET_STORE_PATH).ConfigureAwait(false);

            // assert
            _secretStoreMock
                .Verify(x => x.DeleteAsync(_TEST_SECRET_STORE_PATH), Times.Once);
        }

        [Test]
        public void GetAsync_ShouldThrowWhenSecretStoreThrows()
        {
            // arrange
            var exception = new InvalidOperationException();
            _secretStoreMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(exception);

            // act
            Func<Task<Secret>> func = () => _sut.GetAsync(_TEST_SECRET_STORE_PATH);

            // assert
            func.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void SetAsync_ShouldThrowWhenSecretStoreThrows()
        {
            // arrange
            var exception = new InvalidOperationException();
            _secretStoreMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Secret>()))
                .Throws(exception);

            // act
            Func<Task> func = () => _sut.SetAsync(_TEST_SECRET_STORE_PATH, new Secret());

            // assert
            func.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void DeleteAsync_ShouldThrowWhenSecretStoreThrows()
        {
            // arrange
            var exception = new InvalidOperationException();
            _secretStoreMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            Func<Task> func = () => _sut.DeleteAsync(_TEST_SECRET_STORE_PATH);

            // assert
            func.ShouldThrow<InvalidOperationException>();
        }
    }
}
