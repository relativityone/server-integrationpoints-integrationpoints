using FluentAssertions;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using LanguageExt;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Contracts;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints
{
    [TestFixture, Category("Unit")]
    public class InternalSourceProviderInstallerTests
    {
        private Mock<IRipProviderInstaller> _ripProviderInstallerMock;
        private Mock<IEHHelper> _helperMock;

        private SourceProvider _sourceProvider;

        private SubjectUnderTest _sut;

        private const int _APPLICATION_ID = 3232;
        private const int _WORKSPACE_ID = 84221;

        [SetUp]
        public void SetUp()
        {
            _sourceProvider = new SourceProvider
            {
                GUID = Guid.NewGuid(),
                ApplicationGUID = Guid.NewGuid(),
                Name = "RipInternalProvider",
            };

            _ripProviderInstallerMock = new Mock<IRipProviderInstaller>();

            _helperMock = new Mock<IEHHelper>
            {
                DefaultValue = DefaultValue.Mock
            };
            _helperMock
                .Setup(x => x.GetActiveCaseID())
                .Returns(_WORKSPACE_ID);

            SourceProvider[] sourceProviders = { _sourceProvider };

            _sut = new SubjectUnderTest(
                _ripProviderInstallerMock.Object,
                _helperMock.Object,
                sourceProviders
            );
        }

        [Test]
        public void ShouldDirectlyCallRipProviderInstaller()
        {
            // act
            _sut.Execute();

            // assert
            _ripProviderInstallerMock.Verify(x =>
                x.InstallProvidersAsync(
                    It.Is<IEnumerable<SourceProvider>>(z => ValidateSourceProvider(z))
                )
            );
        }

        [Test]
        public void ShouldReturnSuccessWhenRipProviderInstallerReturnedSuccess()
        {
            // arrange
            _ripProviderInstallerMock.Setup(x => x.InstallProvidersAsync(It.IsAny<IEnumerable<SourceProvider>>()))
                .Returns(Task.FromResult(Right<string, Unit>(Unit.Default)));

            // act
            Response result = _sut.Execute();

            // assert
            result.Success.Should().BeTrue("because provider installer returned success");
        }

        [Test]
        public void ShouldReturnErrorWhenRipProviderInstallerReturnedError()
        {
            // arrange
            _ripProviderInstallerMock.Setup(x => x.InstallProvidersAsync(It.IsAny<IEnumerable<SourceProvider>>()))
                .Returns(Task.FromResult(Left<string, Unit>("installation error")));

            // act
            Response result = _sut.Execute();

            // assert
            string expectedError = $"Failed to install [Provider: {_sourceProvider.Name}]";
            result.Success.Should().BeFalse("because provider installer returned failure");
            result.Message.Should().Be(expectedError);
        }

        private bool ValidateSourceProvider(IEnumerable<SourceProvider> sourceProviders)
        {
            return sourceProviders.Any(x => x.GUID == _sourceProvider.GUID);
        }

        private class SubjectUnderTest : InternalSourceProviderInstaller
        {
            private readonly IDictionary<Guid, SourceProvider> _sourceProviders;

            public SubjectUnderTest(
                IRipProviderInstaller ripProviderInstaller,
                IEHHelper helper,
                IEnumerable<SourceProvider> sourceProviders)
            : base(ripProviderInstaller)
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
