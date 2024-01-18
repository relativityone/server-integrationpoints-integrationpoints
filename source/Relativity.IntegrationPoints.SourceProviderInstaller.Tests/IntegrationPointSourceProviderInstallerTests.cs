using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.EventHandler;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Services;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Tests
{
	[TestFixture]
	public class IntegrationPointSourceProviderInstallerTests
	{
		private Mock<IProviderManager> _providerManagerMock;
		private Mock<IEHHelper> _helperMock;

		private SourceProvider _firstSourceProvider;
		private SourceProvider _secondSourceProvider;

		private const int _APPLICATION_ID = 3232;
		private const int _WORKSPACE_ID = 84221;

		[SetUp]
		public void SetUp()
		{
			_firstSourceProvider = new SourceProvider
			{
				GUID = Guid.NewGuid(),
				ApplicationGUID = Guid.NewGuid(),
				Name = "RipCustomProvider",
			};
			_secondSourceProvider = new SourceProvider
			{
				GUID = Guid.NewGuid(),
				ApplicationGUID = Guid.NewGuid(),
				Name = "RipCustomProvider",
			};

			_providerManagerMock = new Mock<IProviderManager>();

			var servicesManagerMock = new Mock<IServicesMgr>();
			servicesManagerMock
				.Setup(x => x.CreateProxy<IProviderManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_providerManagerMock.Object);

			_helperMock = new Mock<IEHHelper>
			{
				DefaultValue = DefaultValue.Mock
			};
			_helperMock
				.Setup(x => x.GetServicesManager())
				.Returns(servicesManagerMock.Object);
			_helperMock
				.Setup(x => x.GetActiveCaseID())
				.Returns(_WORKSPACE_ID);
		}

		[Test]
		public void ShouldSendInstallRequest()
		{
			// arrange
			SourceProvider[] sourceProviders = { _firstSourceProvider };
			SubjectUnderTest sut = new SubjectUnderTest(_helperMock.Object, sourceProviders);

			// act
			sut.Execute();

			// assert
			VerifyInstallProviderCallWasMade(_firstSourceProvider);
		}

		[Test]
		public void ShouldSendTwoInstallRequest()
		{
			// arrange
			SourceProvider[] sourceProviders = { _firstSourceProvider, _secondSourceProvider };
			SubjectUnderTest sut = new SubjectUnderTest(_helperMock.Object, sourceProviders);

			// act
			sut.Execute();

			// assert
			VerifyInstallProviderCallWasMade(_firstSourceProvider);
			VerifyInstallProviderCallWasMade(_secondSourceProvider);
		}

		[Test]
		public void ShouldReturnErrorWhenSourceProviderListIsEmpty()
		{
			// arrange
			SourceProvider[] sourceProviders = { };
			SubjectUnderTest sut = new SubjectUnderTest(_helperMock.Object, sourceProviders);

			// act
			Response result = sut.Execute();

			// assert
			string expectedErrorMessage = "Provider does not implement the contract (Empty source provider list returned by GetSourceProviders)";
			result.Success.Should().BeFalse("because source providers list was empty");
			result.Message.Should().Be(expectedErrorMessage);
		}

		[Test]
		public void ShouldReturnSuccessWhenKeplerReturnedSuccess()
		{
			// arrange
			var response = new InstallProviderResponse();
			_providerManagerMock
				.Setup(x => x.InstallProviderAsync(It.IsAny<InstallProviderRequest>()))
				.Returns(Task.FromResult(response));

			SourceProvider[] sourceProviders = { _firstSourceProvider };
			SubjectUnderTest sut = new SubjectUnderTest(_helperMock.Object, sourceProviders);

			// act
			Response result = sut.Execute();

			// assert
			result.Success.Should().BeTrue("because kepler returned success response");
			result.Message.Should().Be("Source Providers created or updated successfully.");
		}

		[Test]
		public void ShouldReturnErrorWhenKeplerReturnedError()
		{
			// arrange
			string errorMessage = "install provider error";
			var response = new InstallProviderResponse(errorMessage);
			_providerManagerMock
				.Setup(x => x.InstallProviderAsync(It.IsAny<InstallProviderRequest>()))
				.Returns(Task.FromResult(response));

			SourceProvider[] sourceProviders = { _firstSourceProvider };
			SubjectUnderTest sut = new SubjectUnderTest(_helperMock.Object, sourceProviders);

			// act
			Response result = sut.Execute();

			// assert
			string expectedErrorMessage = $"Failed to install [Provider: {_firstSourceProvider.Name}]";
			result.Success.Should().BeFalse("because kepler returned error response");
			result.Message.Should().Be(expectedErrorMessage);
		}

		private bool ValidateInstallRequestIsValid(
			InstallProviderRequest request,
			SourceProvider expectedSourceProvider)
		{
			request.WorkspaceID.Should().Be(_WORKSPACE_ID);

			request.ProvidersToInstall.Should().Contain(x => x.GUID == expectedSourceProvider.GUID);

			return true;
		}

		private void VerifyInstallProviderCallWasMade(SourceProvider sourceProvider)
		{
			_providerManagerMock.Verify(x =>
				x.InstallProviderAsync(
					It.Is<InstallProviderRequest>(request => ValidateInstallRequestIsValid(request, sourceProvider))
				)
			);
		}

		private class SubjectUnderTest : IntegrationPointSourceProviderInstaller
		{
			private readonly IDictionary<Guid, SourceProvider> _sourceProviders;

			public SubjectUnderTest(IEHHelper helper, IEnumerable<SourceProvider> sourceProviders)
			{
				Helper = helper;
				ApplicationArtifactId = _APPLICATION_ID;
				_sourceProviders = sourceProviders.ToDictionary(x => x.GUID, x => x);
			}

			public override IDictionary<Guid, SourceProvider> GetSourceProviders()
			{
				return _sourceProviders;
			}
		}
	}
}
