using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Facades.SecretStore.Implementation
{
    [TestFixture, Category("Unit")]
    public class SecretStoreFacadeInstrumentationDecoratorTests
    {
        private Mock<ISecretStoreFacade> _secretStoreFacadeMock;
        private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
        private Mock<IExternalServiceInstrumentation> _instrumentationMock;
        private Mock<IExternalServiceInstrumentationStarted> _startedInstrumentationMock;

        private SecretStoreFacadeInstrumentationDecorator _sut;

        private const string _TEST_SECRET_STORE_PATH = "testPath/101/202";

        [SetUp]
        public void SetUp()
        {
            _secretStoreFacadeMock = new Mock<ISecretStoreFacade>();
            _instrumentationMock = new Mock<IExternalServiceInstrumentation>();
            _startedInstrumentationMock = new Mock<IExternalServiceInstrumentationStarted>();
            _instrumentationMock
                .Setup(x => x.Started())
                .Returns(_startedInstrumentationMock.Object);
            _instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
            _instrumentationProviderMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_instrumentationMock.Object);
            _sut = new SecretStoreFacadeInstrumentationDecorator(
                _secretStoreFacadeMock.Object,
                _instrumentationProviderMock.Object);
        }

        [Test]
        public async Task GetAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            // arrange
            var result = new Secret();

            _secretStoreFacadeMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(result);

            // act
            Secret secret = await _sut
                .GetAsync(_TEST_SECRET_STORE_PATH)
                .ConfigureAwait(false);

            // assert
            _instrumentationMock.Verify(x => x.Started());
            _startedInstrumentationMock.Verify(x => x.Completed());
        }

        [Test]
        public async Task SetAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            // arrange
            var secret = new Secret();

            // act
            await _sut
                .SetAsync(_TEST_SECRET_STORE_PATH, secret)
                .ConfigureAwait(false);

            // assert
            _instrumentationMock.Verify(x => x.Started());
            _startedInstrumentationMock.Verify(x => x.Completed());
        }

        [Test]
        public async Task DeleteAsync_ShouldCallStartedAndCompletedForSuccessfulCall()
        {
            // act
            await _sut
                .DeleteAsync(_TEST_SECRET_STORE_PATH)
                .ConfigureAwait(false);

            // assert
            _instrumentationMock.Verify(x => x.Started());
            _startedInstrumentationMock.Verify(x => x.Completed());
        }

        [Test]
        public async Task GetAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            // arrange
            var exception = new Exception();
            _secretStoreFacadeMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            try
            {
                Secret secret = await _sut
                    .GetAsync(_TEST_SECRET_STORE_PATH)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                // ignore
            }

            // assert
            _startedInstrumentationMock.Verify(x => x.Failed(exception));
        }

        [Test]
        public async Task SetAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            // arrange
            var secret = new Secret();
            var exception = new Exception();
            _secretStoreFacadeMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Secret>()))
                .Throws(exception);

            // act
            try
            {
                await _sut.SetAsync(_TEST_SECRET_STORE_PATH, secret).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // ignore
            }

            // assert
            _startedInstrumentationMock.Verify(x => x.Failed(exception));
        }
        [Test]
        public async Task DeleteAsync_ShouldCallFailedWhenExceptionIsThrown()
        {
            // arrange
            var exception = new Exception();
            _secretStoreFacadeMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            try
            {
                await _sut.DeleteAsync(_TEST_SECRET_STORE_PATH).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // ignore
            }

            // assert
            _startedInstrumentationMock.Verify(x => x.Failed(exception));
        }

        [Test]
        public void GetAsync_ShouldRethrowExceptions()
        {
            // arrange
            var exception = new Exception();
            _secretStoreFacadeMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            Func<Task> getAction = () => _sut.GetAsync(_TEST_SECRET_STORE_PATH);

            // assert
            getAction.ShouldThrow<Exception>()
                .Which
                .Should().Be(exception);
        }

        [Test]
        public void SetAsync_ShouldRethrowExceptions()
        {
            // arrange
            var secret = new Secret();
            var exception = new Exception();
            _secretStoreFacadeMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Secret>()))
                .Throws(exception);

            // act
            Func<Task> setAction = () => _sut.SetAsync(_TEST_SECRET_STORE_PATH, secret);

            // assert
            setAction.ShouldThrow<Exception>()
                .Which
                .Should().Be(exception);
        }

        [Test]
        public void DeleteAsync_ShouldRethrowExceptions()
        {
            // arrange
            var exception = new Exception();
            _secretStoreFacadeMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Throws(exception);

            // act
            Func<Task> deleteAction = () => _sut.DeleteAsync(_TEST_SECRET_STORE_PATH);

            // assert
            deleteAction.ShouldThrow<Exception>()
                .Which
                .Should().Be(exception);
        }
    }
}
