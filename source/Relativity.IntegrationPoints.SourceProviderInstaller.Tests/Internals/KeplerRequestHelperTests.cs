using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Tests.Internals
{
	[TestFixture]
	public class KeplerRequestHelperTests
	{
		private Mock<IAPILog> _loggerMock;
		private Mock<ITestService> _serviceMock;

		private KeplerRequestHelper _sut;

		private const string _RESPONSE = "Service response";
		private const int _NUMBER_OF_RETRIES = 3;
		private const int _WAIT_TIME_BETWEEN_RETRIES = 0;

		[SetUp]
		public void SetUp()
		{
			_serviceMock = new Mock<ITestService>(MockBehavior.Loose);

			_loggerMock = new Mock<IAPILog>();
			Mock<IServicesMgr> servicesManagerMock = new Mock<IServicesMgr>();
			servicesManagerMock
				.Setup(x => x.CreateProxy<ITestService>(It.IsAny<ExecutionIdentity>()))
				.Returns(_serviceMock.Object);

			_sut = new KeplerRequestHelper(
				_loggerMock.Object,
				servicesManagerMock.Object,
				_NUMBER_OF_RETRIES,
				_WAIT_TIME_BETWEEN_RETRIES
			);
		}

		[Test]
		public async Task ShouldMakeOneCallForSuccessResponse()
		{
			// arrange
			int request = 0;
			_serviceMock
				.Setup(x => x.Method(It.IsAny<int>()))
				.Returns(Task.FromResult(_RESPONSE));

			// act
			string result = await _sut.ExecuteWithRetriesAsync<ITestService, int, string>((service, r) => service.Method(r), request).ConfigureAwait(false);

			// assert
			result.Should().Be(_RESPONSE);
			_serviceMock.Verify(x => x.Method(It.IsAny<int>()), Times.Once);
		}

		[Test]
		public async Task ShouldRetryOnErrors()
		{
			// arrange
			int request = 0;
			_serviceMock
				.SetupSequence(x => x.Method(It.IsAny<int>()))
				.Throws<InvalidOperationException>()
				.Throws<ArgumentException>()
				.Returns(Task.FromResult(_RESPONSE));

			// act
			string result = await _sut.ExecuteWithRetriesAsync<ITestService, int, string>((service, r) => service.Method(r), request).ConfigureAwait(false);

			// assert
			result.Should().Be(_RESPONSE);
		}

		[Test]
		public async Task ShouldLogWarningWhenRetrying()
		{
			// arrange
			int request = 0;
			_serviceMock
				.SetupSequence(x => x.Method(It.IsAny<int>()))
				.Throws<InvalidOperationException>()
				.Throws<ArgumentException>()
				.Returns(Task.FromResult(_RESPONSE));

			// act
			await _sut.ExecuteWithRetriesAsync<ITestService, int, string>((service, r) => service.Method(r), request).ConfigureAwait(false);

			// assert
			VerifyWarningWasLogged<ArgumentException>();
			VerifyWarningWasLogged<InvalidOperationException>();
		}

		[Test]
		public void ShouldRethrowLastExceptionWhenExceededNumberOfRetries()
		{
			int request = 0;
			_serviceMock
				.SetupSequence(x => x.Method(It.IsAny<int>()))
				.Throws<InvalidOperationException>()
				.Throws<InvalidOperationException>()
				.Throws<InvalidOperationException>()
				.Throws<ArgumentException>()
				.Returns(Task.FromResult(_RESPONSE));

			// act
			Func<Task> executeMethodAction = () => _sut.ExecuteWithRetriesAsync<ITestService, int, string>((service, r) => service.Method(r), request);

			// assert
			executeMethodAction.ShouldThrow<InvalidSourceProviderException>().WithInnerException<ArgumentException>();
		}

		private void VerifyWarningWasLogged<TException>()
			where TException : Exception
		{
			_loggerMock.Verify(x =>
				x.LogWarning(
					It.IsAny<TException>(),
					It.IsAny<string>(),
					It.IsAny<object[]>())
				);
		}

		public interface ITestService : IDisposable
		{
			Task<string> Method(int request);
		}
	}
}
