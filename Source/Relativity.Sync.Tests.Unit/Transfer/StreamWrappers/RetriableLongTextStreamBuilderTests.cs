using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Polly;
using Relativity.Kepler.Transport;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
    public sealed class RetriableLongTextStreamBuilderTests : IDisposable
    {
        private Mock<IObjectManager> _objectManager;
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUser;
        private Mock<IStreamRetryPolicyFactory> _policyFactory;
        private Mock<ISyncMetrics> _syncMetrics;

        private RetriableLongTextStreamBuilder _instance;
        private Mock<Stream> _readableStream;
        private Mock<IKeplerStream> _keplerStream;

        private const int _RETRY_COUNT = 3;
        private const int _RELATIVITY_OBJECT_ARTIFACT_ID = 1012323;
        private const int _WORKSPACE_ARTIFACT_ID = 1014023;
        private const string _LONG_TEXT_FIELD_NAME = "bar";

        [SetUp]
        public void SetUp()
        {
            _readableStream = new Mock<Stream>();
            _readableStream.SetupGet(s => s.CanRead).Returns(true);
            _keplerStream = new Mock<IKeplerStream>();

            _objectManager = new Mock<IObjectManager>();
            SetupStreamLongText(objectRef => objectRef.ArtifactID == _RELATIVITY_OBJECT_ARTIFACT_ID, fieldRef => fieldRef.Name == _LONG_TEXT_FIELD_NAME)
                .ReturnsAsync(_keplerStream.Object);
            _serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

            _policyFactory = new Mock<IStreamRetryPolicyFactory>();
            _policyFactory.Setup(f => f.Create(It.IsAny<Func<Stream, bool>>(), It.IsAny<Action<Stream, Exception, int>>(), _RETRY_COUNT, It.IsAny<TimeSpan>()))
                .Returns((Func<Stream, bool> shouldRetry, Action<Stream, Exception, int> onRetry, int retryCount, TimeSpan waitInterval) => GetRetryPolicy(shouldRetry, onRetry, retryCount));
            _syncMetrics = new Mock<ISyncMetrics>();

            _instance = new RetriableLongTextStreamBuilder(_WORKSPACE_ARTIFACT_ID, _RELATIVITY_OBJECT_ARTIFACT_ID, _LONG_TEXT_FIELD_NAME, _serviceFactoryForUser.Object, _policyFactory.Object,
                _syncMetrics.Object, new EmptyLogger());
        }

        [Test]
        public async Task ItShouldReturnStream()
        {
            // Arrange
            _keplerStream.Setup(x => x.GetStreamAsync()).ReturnsAsync(_readableStream.Object);

            // Act
            Stream result = await _instance.GetStreamAsync().ConfigureAwait(false);

            // Assert
            result.Should().Be(_readableStream.Object);
            _readableStream.VerifyGet(s => s.CanRead, Times.Once);
            _objectManager.Verify(om => om.StreamLongTextAsync(_WORKSPACE_ARTIFACT_ID, It.IsAny<RelativityObjectRef>(), It.IsAny<FieldRef>()), Times.Once);
        }

        [Test]
        public async Task ItShouldReturnReadableStreamWhenFirstNonReadable()
        {
            // Arrange
            const int expectedCallCount = 2;
            Mock<Stream> unreadableStream = new Mock<Stream>();
            unreadableStream.SetupGet(s => s.CanRead).Returns(false);

            _keplerStream.SetupSequence(x => x.GetStreamAsync()).ReturnsAsync(unreadableStream.Object).ReturnsAsync(_readableStream.Object);

            // Act
            Stream result = await _instance.GetStreamAsync().ConfigureAwait(false);

            // Assert
            result.Should().Be(_readableStream.Object);
            _readableStream.VerifyGet(s => s.CanRead, Times.Once);
            _objectManager.Verify(om => om.StreamLongTextAsync(_WORKSPACE_ARTIFACT_ID, It.IsAny<RelativityObjectRef>(), It.IsAny<FieldRef>()), Times.Exactly(expectedCallCount));

            _syncMetrics.Verify(x => x.Send(It.Is<StreamRetryMetric>(m => m.RetryCounter != null)), Times.Once);
        }

        [Test]
        public async Task ItShouldReturnReadableStreamWhenFirstThrows()
        {
            // Arrange
            const int expectedCallCount = 2;

            _keplerStream.SetupSequence(x => x.GetStreamAsync()).Throws<ServiceException>().ReturnsAsync(_readableStream.Object);

            // Act
            Stream result = await _instance.GetStreamAsync().ConfigureAwait(false);

            // Assert
            result.Should().Be(_readableStream.Object);
            _readableStream.VerifyGet(s => s.CanRead, Times.Once);
            _objectManager.Verify(om => om.StreamLongTextAsync(_WORKSPACE_ARTIFACT_ID, It.IsAny<RelativityObjectRef>(), It.IsAny<FieldRef>()), Times.Exactly(expectedCallCount));
            _syncMetrics.Verify(x => x.Send(It.Is<StreamRetryMetric>(m => m.RetryCounter != null)), Times.Once);
        }

        [Test]
        public async Task ItShouldReturnUnreadableStreamWhenAllAttemptsUnreadable()
        {
            // Arrange
            const int expectedCallCount = _RETRY_COUNT + 1;
            Mock<Stream> unreadableStream = new Mock<Stream>();
            unreadableStream.SetupGet(s => s.CanRead).Returns(false);

            _keplerStream.Setup(x => x.GetStreamAsync()).ReturnsAsync(unreadableStream.Object);

            // Act
            Stream result = await _instance.GetStreamAsync().ConfigureAwait(false);

            // Assert
            result.Should().Be(unreadableStream.Object);
            _objectManager.Verify(om => om.StreamLongTextAsync(_WORKSPACE_ARTIFACT_ID, It.IsAny<RelativityObjectRef>(), It.IsAny<FieldRef>()), Times.Exactly(expectedCallCount));

            _syncMetrics.Verify(x => x.Send(It.Is<StreamRetryMetric>(m => m.RetryCounter != null)), Times.Exactly(_RETRY_COUNT));
        }

        [Test]
        public async Task ItShouldThrowWhenAllAttemptsThrow()
        {
            // Arrange
            const int expectedCallCount = _RETRY_COUNT + 1;

            _keplerStream.Setup(x => x.GetStreamAsync()).Throws<ServiceException>();

            // Act
            Func<Task<Stream>> action = () => _instance.GetStreamAsync();

            // Assert
            await action.Should().ThrowAsync<ServiceException>().ConfigureAwait(false);

            _objectManager.Verify(om => om.StreamLongTextAsync(_WORKSPACE_ARTIFACT_ID, It.IsAny<RelativityObjectRef>(), It.IsAny<FieldRef>()), Times.Exactly(expectedCallCount));
            _syncMetrics.Verify(x => x.Send(It.Is<StreamRetryMetric>(m => m.RetryCounter != null)), Times.Exactly(_RETRY_COUNT));
        }

        private IAsyncPolicy<Stream> GetRetryPolicy(Func<Stream, bool> shouldRetry, Action<Stream, Exception, int> onRetry, int retryCount)
        {
            return Policy
                .HandleResult(shouldRetry)
                .Or<Exception>()
                .RetryAsync(retryCount, (result, i) => onRetry(result.Result, result.Exception, i));
        }

        private ISetup<IObjectManager, Task<IKeplerStream>> SetupStreamLongText(
            Func<RelativityObjectRef, bool> objectRefMatcher,
            Func<FieldRef, bool> fieldRefMatcher)
        {
            return _objectManager.Setup(x => x.StreamLongTextAsync(
                It.IsAny<int>(),
                It.Is<RelativityObjectRef>(r => objectRefMatcher(r)),
                It.Is<FieldRef>(r => fieldRefMatcher(r))));
        }

        public void Dispose()
        {
            _instance?.Dispose();
        }
    }
}
