using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Polly.Retry;
using Relativity.Sync.HttpClient;
using Relativity.Sync.Logging;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.HttpClient
{
    [TestFixture]
    public class HttpClientRetryPolicyProviderTests
    {
        private Mock<ISerializer> _serializer;

        private HttpClientRetryPolicyProvider _sut;

        [SetUp]
        public void SetUp()
        {
            _serializer = new Mock<ISerializer>();
            _sut = new HttpClientRetryPolicyProvider(_serializer.Object, new EmptyLogger());
            _sut.Pow = 0; // Do not wait too long in unit tests
        }

        [Test]
        public async Task GetPolicy_ShouldRetryUntilSuccess_WhenHttpRequestException()
        {
            // Arrange
            int maxRetryCount = 3;
            Mock<ITestService> testService = new Mock<ITestService>();
            testService
                .SetupSequence(service => service.Invoke())
                .Throws<HttpRequestException>()
                .Throws<HttpRequestException>()
                .Throws<HttpRequestException>()
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                ;

            // Act
            RetryPolicy<HttpResponseMessage> retryPolicy = _sut.GetPolicy(maxRetryCount);
            await retryPolicy.ExecuteAsync(async () => await testService.Object.Invoke());

            // Assert
            testService.Verify(x => x.Invoke(), Times.Exactly(maxRetryCount + 1));
        }

        [Test]
        public void GetPolicy_ShouldRetryUntilFail_WhenHttpRequestException()
        {
            // Arrange
            int maxRetryCount = 3;
            Mock<ITestService> testService = new Mock<ITestService>();
            testService
                .SetupSequence(service => service.Invoke())
                .Throws<HttpRequestException>()
                .Throws<HttpRequestException>()
                .Throws<HttpRequestException>()
                .Throws<HttpRequestException>()
                ;

            // Act
            RetryPolicy<HttpResponseMessage> retryPolicy = _sut.GetPolicy(maxRetryCount);
            Func<Task<HttpResponseMessage>> action = async () => await retryPolicy.ExecuteAsync(async () => await testService.Object.Invoke());

            // Assert
            action.Should().Throw<HttpRequestException>();
            testService.Verify(x => x.Invoke(), Times.Exactly(maxRetryCount + 1));
        }

        [Test]
        public async Task GetPolicy_ShouldRetry_WhenResponseNotSuccessfull()
        {
            // Arrange
            int maxRetryCount = 1;
            Mock<ITestService> testService = new Mock<ITestService>();
            testService
                .SetupSequence(service => service.Invoke())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadGateway))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                ;

            // Act
            RetryPolicy<HttpResponseMessage> retryPolicy = _sut.GetPolicy(maxRetryCount);
            await retryPolicy.ExecuteAsync(async () => await testService.Object.Invoke());

            // Assert
            testService.Verify(x => x.Invoke(), Times.Exactly(maxRetryCount + 1));
        }

        public interface ITestService
        {
            Task<HttpResponseMessage> Invoke();
        }
    }
}
