using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.Domain;
using Moq;
using NUnit.Framework;
using System;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers;

namespace kCura.IntegrationPoints.Core.Tests.Services.Domain
{
    [TestFixture, Category("Unit")]
    public class DataProviderBuilderTests : TestBase
    {
        private Mock<IProviderFactory> _providerFactory;
        private Mock<ProviderFactoryVendor> _providerFactoryVendorMock;
        private DataProviderBuilder _sut;
        private readonly Guid _applicationGuid = Guid.NewGuid();
        private readonly Guid _providerGuid = Guid.NewGuid();

        public override void SetUp()
        {
            _providerFactory = new Mock<IProviderFactory>();
            _providerFactoryVendorMock = new Mock<ProviderFactoryVendor>();
            _providerFactoryVendorMock
                .Setup(x => x.GetProviderFactory(It.IsAny<Guid>()))
                .Returns(_providerFactory.Object);

            _sut = new DataProviderBuilder(_providerFactoryVendorMock.Object);
        }

        [Test]
        public void GetDataProvider_CallsAppropriateMethods()
        {
            // act
            _sut.GetDataProvider(_applicationGuid, _providerGuid);

            // assert
            _providerFactoryVendorMock.Verify(x => x.GetProviderFactory(_applicationGuid));
            _providerFactory.Verify(x => x.CreateProvider(_providerGuid));
        }

        [Test]
        public void ShouldWrapProviderInSafeDisposeWrapperWhenProperInterfaceIsImplemented()
        {
            // arrange
            var providerMock = new Mock<IProviderAggregatedInterfaces>();
            SetupProviderFactoryMockToReturnProvider(providerMock.Object);

            // act
            IDataSourceProvider createdProvider = _sut.GetDataProvider(_applicationGuid, _providerGuid);

            // assert
            createdProvider.Should()
                .BeOfType<SafeDisposingProviderWrapper>(
                    "because providers implementing {0} should be wrapped in a {1} type",
                    nameof(IProviderAggregatedInterfaces),
                    nameof(SafeDisposingProviderWrapper)
                );
        }

        [Test]
        public void ShouldNotWrapProviderInSafeDisposeWrapperWhenProperInterfaceIsNotImplemented()
        {
            // arrange
            var providerMock = new Mock<IDataSourceProvider>();
            SetupProviderFactoryMockToReturnProvider(providerMock.Object);

            // act
            IDataSourceProvider createdProvider = _sut.GetDataProvider(_applicationGuid, _providerGuid);

            // assert
            createdProvider.Should()
                .NotBeOfType<SafeDisposingProviderWrapper>(
                    "because providers do not implementing {0} should not be wrapped in a {1} type",
                    nameof(IProviderAggregatedInterfaces),
                    nameof(SafeDisposingProviderWrapper)
                );
        }

        private void SetupProviderFactoryMockToReturnProvider(IDataSourceProvider provider)
        {
            _providerFactory
                .Setup(x => x.CreateProvider(It.IsAny<Guid>()))
                .Returns(provider);
        }
    }
}
