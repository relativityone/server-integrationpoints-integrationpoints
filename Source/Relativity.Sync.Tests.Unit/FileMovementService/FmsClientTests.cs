using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using Polly;
using Relativity.Sync.HttpClient;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer.FileMovementService;
using Relativity.Sync.Transfer.FileMovementService.Models;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.FileMovementService
{
    [TestFixture]
    public class FmsClientTests
    {
        private const string KubernetesUrl = "http://k8s";
        private const string FmsUrl = "fms";

        private Mock<ISharedServiceHttpClientFactory> _httpClientFactory;
        private Mock<IHttpClientRetryPolicyProvider> _retryProvider;

        private Mock<HttpMessageHandler> _httpMessageHandler;

        private FmsClient _sut;

        [SetUp]
        public void SetUp()
        {
            _httpClientFactory = new Mock<ISharedServiceHttpClientFactory>();
            _retryProvider = new Mock<IHttpClientRetryPolicyProvider>();

            _httpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClientFactory
                .Setup(x => x.GetHttpClientAsync())
                .ReturnsAsync(new System.Net.Http.HttpClient(_httpMessageHandler.Object));

            _retryProvider.Setup(x => x.GetPolicy(It.IsAny<int>()))
                .Returns(Policy<HttpResponseMessage>.Handle<Exception>().WaitAndRetryAsync(0, attempt => TimeSpan.Zero));

            _sut = new FmsClient(_httpClientFactory.Object, _retryProvider.Object, new JSONSerializer(), new EmptyLogger());
        }

        [Test]
        public async Task GetRunStatusAsync_ShouldReturnResponse()
        {
            // Arrange
            string endpointUrl = $"{KubernetesUrl}/{FmsUrl}/api/v1/DataFactory/GetStatus";
            RunStatusRequest request = new RunStatusRequest()
            {
                RunId = "RunID",
                TraceId = Guid.NewGuid(),
                EndpointURL = endpointUrl
            };

            RunStatusResponse response = new RunStatusResponse()
            {
                TraceId = Guid.NewGuid(),
                Message = "Some message",
                Status = "Success"
            };
            string expectedUri = $"{KubernetesUrl}/{FmsUrl}/api/v1/DataFactory/GetStatus/RunID?traceId={request.TraceId}";

            _httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals(expectedUri)), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(response))
                });

            // Act
            RunStatusResponse actualResponse = await _sut.GetRunStatusAsync(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            actualResponse.Message.Should().Be(response.Message);
            actualResponse.Status.Should().Be(response.Status);
            actualResponse.TraceId.Should().Be(response.TraceId);
        }

        [Test]
        public void GetRunStatusAsync_ShouldThrow_WhenReponseNotSuccessfull()
        {
            // Arrange
            string endpointUrl = $"{KubernetesUrl}/{FmsUrl}/api/v1/DataFactory/GetStatus";
            RunStatusRequest request = new RunStatusRequest()
            {
                RunId = "RunID",
                TraceId = Guid.NewGuid(),
                EndpointURL = endpointUrl
            };
            string expectedUri = $"{KubernetesUrl}/{FmsUrl}/api/v1/DataFactory/GetStatus/RunID?traceId={request.TraceId}";

            _httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Equals(expectedUri)), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            Func<Task<RunStatusResponse>> action = async () => await _sut.GetRunStatusAsync(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            action.Should().Throw<HttpRequestException>();
        }

        [Test]
        public async Task CopyListOfFilesAsync_ShouldReturnResponse()
        {
            // Arrange
            string expectedUri = $"{KubernetesUrl}/{FmsUrl}/api/v1/DataFactory/CopyListOfFiles";
            CopyListOfFilesRequest request = new CopyListOfFilesRequest()
            {
                DestinationPath = "dest path",
                PathToListOfFiles = "files path",
                SourcePath = "source path",
                TraceId = Guid.NewGuid(),
                EndpointURL = expectedUri
            };

            CopyListOfFilesResponse response = new CopyListOfFilesResponse()
            {
                PipelineName = "pipeline",
                RelativityInstance = "rel instance",
                RunId = "run id",
                TraceId = request.TraceId
            };

            _httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Equals(expectedUri) && req.Content.ReadAsStringAsync().GetAwaiter().GetResult().Equals(JsonConvert.SerializeObject(request))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(response))
                });

            // Act
            CopyListOfFilesResponse actualResponse = await _sut.CopyListOfFilesAsync(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            actualResponse.TraceId.Should().Be(response.TraceId);
            actualResponse.PipelineName.Should().Be(response.PipelineName);
            actualResponse.RelativityInstance.Should().Be(response.RelativityInstance);
            actualResponse.RunId.Should().Be(response.RunId);
        }

        [Test]
        public void CopyListOfFilesAsync_ShouldThrow_WhenResponseNotSuccessfull()
        {
            // Arrange
            string expectedUri = $"{KubernetesUrl}/{FmsUrl}/api/v1/DataFactory/CopyListOfFiles";
            CopyListOfFilesRequest request = new CopyListOfFilesRequest()
            {
                DestinationPath = "dest path",
                PathToListOfFiles = "files path",
                SourcePath = "source path",
                TraceId = Guid.NewGuid(),
                EndpointURL = expectedUri
            };

            _httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Equals(expectedUri) && req.Content.ReadAsStringAsync().GetAwaiter().GetResult().Equals(JsonConvert.SerializeObject(request))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            Func<Task<CopyListOfFilesResponse>> action = async () => await _sut.CopyListOfFilesAsync(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            action.Should().Throw<HttpRequestException>();
        }

        [Test]
        public async Task CancelRunAsync_ShouldReturnResponse()
        {
            // Arrange
            string expectedUri = $"{KubernetesUrl}/{FmsUrl}/api/v1/DataFactory/Cancel";
            RunCancelRequest request = new RunCancelRequest()
            {
                TraceId = Guid.NewGuid(),
                RunId = "RunID",
                EndpointURL = expectedUri
            };

            string response = "response";

            _httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Equals(expectedUri) && req.Content.ReadAsStringAsync().GetAwaiter().GetResult().Equals(JsonConvert.SerializeObject(request))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(response)
                });

            // Act
            string actualResponse = await _sut.CancelRunAsync(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            actualResponse.Should().Be(response);
        }

        [Test]
        public void CancelRunAsync_ShouldThrow_WhenResponseNotSuccessfull()
        {
            // Arrange
            string expectedUri = $"{KubernetesUrl}/{FmsUrl}/api/v1/DataFactory/Cancel";
            RunCancelRequest request = new RunCancelRequest()
            {
                TraceId = Guid.NewGuid(),
                RunId = "RunID",
                EndpointURL = expectedUri
            };

            _httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Equals(expectedUri) && req.Content.ReadAsStringAsync().GetAwaiter().GetResult().Equals(JsonConvert.SerializeObject(request))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            Func<Task<string>> action = async () => await _sut.CancelRunAsync(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            action.Should().Throw<HttpRequestException>();
        }
    }
}
