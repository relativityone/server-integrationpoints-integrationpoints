using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Helpers;
using Relativity.Sync.HttpClient;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.HttpClient
{
    [TestFixture]
    public class SharedServiceHttpClientFactoryTests
    {
        private Mock<ISourceServiceFactoryForAdmin> _sourceServiceFactoryForAdmin;
        private Mock<IAuthTokenProvider> _authTokenProvider;

        private SharedServiceHttpClientFactory _sut;

        [SetUp]
        public void SetUp()
        {
            _authTokenProvider = new Mock<IAuthTokenProvider>();
            _sourceServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
            _sourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IAuthTokenProvider>())
                .ReturnsAsync(_authTokenProvider.Object);

            _sut = new SharedServiceHttpClientFactory(_sourceServiceFactoryForAdmin.Object, new EmptyLogger());
        }

        [Test]
        public async Task GetHttpClientAsync_ShouldCreateHttpClient()
        {
            // Arrange
            const string token = "MyToken";
            _authTokenProvider
                .Setup(x => x.GetAuthToken("SharedServices"))
                .ReturnsAsync(token);

            // Act
            System.Net.Http.HttpClient httpClient = await _sut.GetHttpClientAsync().ConfigureAwait(false);

            // Assert
            httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Bearer");
            httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(token);
            httpClient.Timeout.TotalMinutes.Should().Be(2);
        }

        [Test]
        public void GetHttpClientAsync_ShouldThrow_WhenGetAuthTokenFails()
        {
            // Arrange
            _authTokenProvider
                .Setup(x => x.GetAuthToken("SharedServices"))
                .Throws<ServiceException>();

            // Act
            Func<Task<System.Net.Http.HttpClient>> action = () => _sut.GetHttpClientAsync();

            // Assert
            action.Should().Throw<ServiceException>();
        }
    }
}