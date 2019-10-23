using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.EventHandler;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Tests.Internals
{
	[TestFixture]
	public class IntegrationPointSourceProviderUninstallerInternalTests
	{
		private Mock<Action> _preExecuteActionMock;
		private Mock<Action<bool, Exception>> _postExecuteActionMock;
		private Mock<IProviderManager> _providerManagerMock;

		private IntegrationPointSourceProviderUninstallerInternal _sut;

		private const int _APPLICATION_ID = 3232;
		private const int _WORKSPACE_ID = 84221;
		private const int _NUMBER_OF_RETRIES = 3;
		private const int _WAIT_TIME_BETWEEN_RETRIES = 0;

		[SetUp]
		public void SetUp()
		{
			_preExecuteActionMock = new Mock<Action>();
			_postExecuteActionMock = new Mock<Action<bool, Exception>>();

			var loggerMock = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};

			_providerManagerMock = new Mock<IProviderManager>();

			var servicesManagerMock = new Mock<IServicesMgr>();
			servicesManagerMock
				.Setup(x => x.CreateProxy<IProviderManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_providerManagerMock.Object);

			var keplerRequestHelper = new KeplerRequestHelper(
				loggerMock.Object,
				servicesManagerMock.Object,
				_NUMBER_OF_RETRIES,
				_WAIT_TIME_BETWEEN_RETRIES
			);

			_sut = new IntegrationPointSourceProviderUninstallerInternal(
				keplerRequestHelper,
				_preExecuteActionMock.Object,
				_postExecuteActionMock.Object
			);
		}

		[Test]
		public void ShouldSendUninstallRequest()
		{
			// arrange
			var response = new UninstallProviderResponse();
			_providerManagerMock
				.Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
				.Returns(Task.FromResult(response));

			// act
			_sut.Execute(_WORKSPACE_ID, _APPLICATION_ID);

			// assert
			_providerManagerMock.Verify(x =>
				x.UninstallProviderAsync(
				It.Is<UninstallProviderRequest>(request => ValidateUninstallRequestIsValid(request))
			)
			);
		}

		[Test]
		public void ShouldReturnSuccessWhenKeplerReturnedSuccess()
		{
			//arrange
			var response = new UninstallProviderResponse();
			_providerManagerMock
				.Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
				.Returns(Task.FromResult(response));

			// act
			Response result = _sut.Execute(_WORKSPACE_ID, _APPLICATION_ID);

			// assert
			result.Success.Should().BeTrue("because kepler returned success response");
		}

		[Test]
		public void ShouldReturnErrorWhenKeplerReturnedError()
		{
			// arrange
			string errorMessage = "error in kepler";
			var response = new UninstallProviderResponse(errorMessage);
			_providerManagerMock
				.Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
				.Returns(Task.FromResult(response));

			// act
			Response result = _sut.Execute(_WORKSPACE_ID, _APPLICATION_ID);

			// assert
			result.Success.Should().BeFalse("because kepler returned error response");
			result.Message.Should().Be(errorMessage);
		}

		[Test]
		public void ShouldReturnErrorWhenKeplerThrownExceptions()
		{
			// arrange
			var exceptionToThrow = new InvalidOperationException();
			_providerManagerMock
				.Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
				.Throws(exceptionToThrow);

			// act
			Response result = _sut.Execute(_WORKSPACE_ID, _APPLICATION_ID);

			// assert
			string expectedErrorMessage = "Uninstalling source provider failed.";
			result.Success.Should().BeFalse("because kepler returned error response");
			result.Message.Should().Be(expectedErrorMessage);
		}

		[Test]
		public void ShouldExecutePreExecuteAction()
		{
			// act
			_sut.Execute(_WORKSPACE_ID, _APPLICATION_ID);

			// assert
			_preExecuteActionMock.Verify(x => x());
		}

		[Test]
		public void ShouldExecutePostExecuteActionWhenSuccess()
		{
			// arrange
			var response = new UninstallProviderResponse();
			_providerManagerMock
				.Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
				.Returns(Task.FromResult(response));

			// act
			_sut.Execute(_WORKSPACE_ID, _APPLICATION_ID);

			// assert
			bool expectedIsSuccess = true;
			Exception expectedException = null;
			_postExecuteActionMock.Verify(x => x(expectedIsSuccess, expectedException));
		}

		[Test]
		public void ShouldExecutePostExecuteActionWhenError()
		{
			// arrange
			string errorMessage = "error in kepler";
			var response = new UninstallProviderResponse(errorMessage);
			_providerManagerMock
				.Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
				.Returns(Task.FromResult(response));

			// act
			_sut.Execute(_WORKSPACE_ID, _APPLICATION_ID);

			// assert
			bool expectedIsSuccess = false;
			Exception expectedException = null;
			_postExecuteActionMock.Verify(x => x(expectedIsSuccess, expectedException));
		}

		[Test]
		public void ShouldExecutePostExecuteActionWhenException()
		{
			// arrange
			var exceptionToThrow = new InvalidOperationException();
			_providerManagerMock
				.Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
				.Throws(exceptionToThrow);

			// act
			_sut.Execute(_WORKSPACE_ID, _APPLICATION_ID);

			// assert
			bool expectedIsSuccess = false;
			_postExecuteActionMock.Verify(x =>
				x(
				expectedIsSuccess,
				It.Is<Exception>(ex => ValidatePostExecuteException(ex, exceptionToThrow))
			)
			);
		}

		private bool ValidatePostExecuteException(Exception exception, Exception expectedInnerException)
		{
			exception.InnerException.Should().Be(expectedInnerException);
			exception.Should().BeOfType<InvalidSourceProviderException>();
			return true;
		}

		private bool ValidateUninstallRequestIsValid(UninstallProviderRequest request)
		{
			request.ApplicationID.Should().Be(_APPLICATION_ID);
			request.WorkspaceID.Should().Be(_WORKSPACE_ID);

			return true;
		}
	}
}
