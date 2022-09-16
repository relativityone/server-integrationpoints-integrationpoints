using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using LanguageExt;
using Moq;
using NUnit.Framework;
using Relativity.API;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.SourceProviderInstaller;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers.Implementations
{
    [TestFixture, Category("Unit")]
    public class InProcessSourceProviderInstallerTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<IRipProviderInstaller> _ripProviderInstallerMock;
        private Mock<IEHHelper> _helperMock;
        private Mock<IKubernetesMode> _kubernetesModeFake;

        private SourceProvider _sourceProvider;
        private SourceProvider[] _sourceProviders;

        private InProcessSourceProviderInstaller _sut;

        private const int _WORKSPACE_ID = 84221;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>
            {
                DefaultValue = DefaultValue.Mock
            };

            _sourceProvider = new SourceProvider
            {
                GUID = Guid.NewGuid(),
                ApplicationGUID = Guid.NewGuid(),
                Name = "RipInternalProvider",
            };

            _ripProviderInstallerMock = new Mock<IRipProviderInstaller>();
            _kubernetesModeFake = new Mock<IKubernetesMode>();

            _helperMock = new Mock<IEHHelper>
            {
                DefaultValue = DefaultValue.Mock
            };
            _helperMock
                .Setup(x => x.GetActiveCaseID())
                .Returns(_WORKSPACE_ID);

            _sourceProviders = new[] { _sourceProvider };

            _sut = new InProcessSourceProviderInstaller(
                _loggerMock.Object,
                _helperMock.Object,
                _kubernetesModeFake.Object,
                null,
                _ripProviderInstallerMock.Object
            );
        }

        [Test]
        public async Task ShouldDirectlyCallRipProviderInstaller()
        {
            // act
            await _sut.InstallSourceProvidersAsync(_WORKSPACE_ID, _sourceProviders).ConfigureAwait(false);

            // assert
            _ripProviderInstallerMock.Verify(x =>
                x.InstallProvidersAsync(
                    It.Is<IEnumerable<SourceProvider>>(z => ValidateSourceProvider(z))
                )
            );
        }

        [Test]
        public async Task ShouldCompleteWithoutErrorWhenRipProviderInstallerReturnedSuccess()
        {
            // arrange
            _ripProviderInstallerMock.Setup(x => x.InstallProvidersAsync(It.IsAny<IEnumerable<SourceProvider>>()))
                .Returns(Task.FromResult(Right<string, Unit>(Unit.Default)));

            // act
            await _sut.InstallSourceProvidersAsync(_WORKSPACE_ID, _sourceProviders).ConfigureAwait(false);
        }

        [Test]
        public void ShouldThrowExceptionWhenRipProviderInstallerReturnedError()
        {
            // arrange
            string expectedError = "installation error";
            _ripProviderInstallerMock.Setup(x => x.InstallProvidersAsync(It.IsAny<IEnumerable<SourceProvider>>()))
                .Returns(Task.FromResult(Left<string, Unit>(expectedError)));

            // act
            Func<Task> installSourceProviderAction = () => _sut.InstallSourceProvidersAsync(_WORKSPACE_ID, _sourceProviders);

            // assert
            installSourceProviderAction.ShouldThrow<InvalidSourceProviderException>().WithMessage(expectedError);
        }

        [Test]
        public void ShouldThrowExceptionWhenCannotCreateRipProviderInstallerInstace()
        {
            // arrange
            _helperMock
                .Setup(x => x.GetDBContext(It.IsAny<int>()))
                .Throws<InvalidOperationException>();

            var sut = new InProcessSourceProviderInstaller(
                _loggerMock.Object,
                _helperMock.Object,
                _kubernetesModeFake.Object,
                null,
                ripProviderInstaller: null
                

            );

            // act
            Func<Task> installSourceProviderAction = () => sut.InstallSourceProvidersAsync(_WORKSPACE_ID, _sourceProviders);

            // assert
            string expectedMessage = $"Error occured while creating instance of {nameof(IRipProviderInstaller)}.";
            installSourceProviderAction.ShouldThrow<InvalidSourceProviderException>()
                .Which.Message.Should().Contain(expectedMessage);
        }

        private bool ValidateSourceProvider(IEnumerable<SourceProvider> sourceProviders)
        {
            return sourceProviders.Any(x => x.GUID == _sourceProvider.GUID);
        }
    }
}
