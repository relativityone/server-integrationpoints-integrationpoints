using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Tests.Internals
{
	[TestFixture]
	public class KeplerSourceProviderInstallerTests
	{
		private Mock<IProviderManager> _providerManager;

		private KeplerSourceProviderInstaller _sut;

		private const int _WORKSPACE_ID = 313;
		private const string _SOURCE_PROVIDER_NAME = "TestSourceProvider";
		private const string _ERROR_MESSAGE = "Error occure in kepler";

		[SetUp]
		public void SetUp()
		{
			Mock<IAPILog> loggerMock = new Mock<IAPILog>()
			{
				DefaultValue = DefaultValue.Mock
			};

			_providerManager = new Mock<IProviderManager>();
			Mock<IServicesMgr>  servicesManagerMock = new Mock<IServicesMgr>();
			servicesManagerMock
				.Setup(x =>
					x.CreateProxy<IProviderManager>(It.IsAny<ExecutionIdentity>())
				).Returns(_providerManager.Object);

			const int numberOfRetriesForKeplerCalls = 3;
			var keplerRequestHelper = new KeplerRequestHelper(loggerMock.Object, servicesManagerMock.Object, numberOfRetriesForKeplerCalls, 0);

			_sut = new KeplerSourceProviderInstaller(keplerRequestHelper);
		}

		[Test]
		public async Task ShouldSendRequestToProviderManagerServiceForOneSourceProvider()
		{
			// arrange
			var sourceProvider = new SourceProvider
			{
				Name = _SOURCE_PROVIDER_NAME
			};
			SourceProvider[] sourceProvidersToInstall = { sourceProvider };

			SetInstallProviderResponse(isSuccess: true);

			// act
			await _sut.InstallSourceProvidersAsync(_WORKSPACE_ID, sourceProvidersToInstall).ConfigureAwait(false);

			// assert
			_providerManager.Verify(x =>
				x.InstallProviderAsync(It.Is<InstallProviderRequest>(request => AssertInstallProviderRequestIsValid(request, sourceProvidersToInstall)))
			);
		}

		[Test]
		public void ShouldThrowExceptionWhenInstallRequestReturnedError()
		{
			// arrange
			var sourceProvider = new SourceProvider
			{
				Name = _SOURCE_PROVIDER_NAME
			};
			SourceProvider[] sourceProvidersToInstall = { sourceProvider };

			SetInstallProviderResponse(isSuccess: false);

			// act
			Func<Task> installAction = () => _sut.InstallSourceProvidersAsync(_WORKSPACE_ID, sourceProvidersToInstall);

			// assert
			string expectedError = $"An error occured while installing source providers: {_ERROR_MESSAGE}";
			installAction.ShouldThrow<InvalidSourceProviderException>().WithMessage(expectedError);
		}

		[Test]
		public async Task ShouldRetryOnKeplerError()
		{
			// arrange
			var sourceProvider = new SourceProvider
			{
				Name = _SOURCE_PROVIDER_NAME
			};
			SourceProvider[] sourceProvidersToInstall = { sourceProvider };

			SetInstallProviderToThrowInFirstCallAndReturnSuccessInSecond();

			// act
			await _sut.InstallSourceProvidersAsync(_WORKSPACE_ID, sourceProvidersToInstall).ConfigureAwait(false);

			// assert
			const int callCount = 2;
			_providerManager.Verify(x =>
				x.InstallProviderAsync(It.Is<InstallProviderRequest>(request => AssertInstallProviderRequestIsValid(request, sourceProvidersToInstall))),
				Times.Exactly(callCount)
			);
		}

		[Test]
		public void ShouldThrowExceptionWhenKeplerConstantlyFails()
		{
			// arrange
			var sourceProvider = new SourceProvider
			{
				Name = _SOURCE_PROVIDER_NAME
			};
			SourceProvider[] sourceProvidersToInstall = { sourceProvider };

			SetInstallProviderToThrow();

			// act
			Func<Task> installAction = () => _sut.InstallSourceProvidersAsync(_WORKSPACE_ID, sourceProvidersToInstall);

			// assert
			string expectedError = $"Error occured while sending request to {nameof(IProviderManager)}";
			installAction.ShouldThrow<InvalidSourceProviderException>().WithMessage(expectedError);
		}

		[Test]
		public void ShouldThrowArgumentExceptionForNullSourceProviders()
		{
			// arrange
			SourceProvider[] sourceProvidersToInstall = null;

			// act
			Func<Task> installSourceProvidersAction = () => _sut.InstallSourceProvidersAsync(_WORKSPACE_ID, sourceProvidersToInstall);

			// arrange
			installSourceProvidersAction.ShouldThrow<ArgumentException>("because source providers list was null");
		}

		private bool AssertInstallProviderRequestIsValid(
			InstallProviderRequest request,
			IEnumerable<SourceProvider> expectedSourceProviders)
		{
			request.WorkspaceID.Should().Be(_WORKSPACE_ID);

			foreach (SourceProvider sourceProvider in expectedSourceProviders)
			{
				request.ProvidersToInstall.Should().Contain(x => x.Name == sourceProvider.Name);
			}

			return true;
		}

		private void SetInstallProviderResponse(bool isSuccess)
		{
			_providerManager
				.Setup(x => x.InstallProviderAsync(It.IsAny<InstallProviderRequest>()))
				.Returns(Task.FromResult(CreateInstallProviderResponse(isSuccess)));
		}

		private void SetInstallProviderToThrow()
		{
			_providerManager
				.Setup(x => x.InstallProviderAsync(It.IsAny<InstallProviderRequest>()))
				.Throws<InvalidOperationException>();
		}

		private void SetInstallProviderToThrowInFirstCallAndReturnSuccessInSecond()
		{
			_providerManager
				.SetupSequence(x => x.InstallProviderAsync(It.IsAny<InstallProviderRequest>()))
				.Throws<InvalidOperationException>()
				.Returns(Task.FromResult(CreateInstallProviderResponse(isSuccess: true)));
		}

		private InstallProviderResponse CreateInstallProviderResponse(bool isSuccess)
		{
			return isSuccess
				? new InstallProviderResponse()
				: new InstallProviderResponse(_ERROR_MESSAGE);
		}
	}
}
