using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.Domain;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Domain
{
    [TestFixture, Category("Unit")]
    public class ProviderFactoryVendorTests : TestBase
    {
        private IProviderFactory _providerFactoryMock;
        private IProviderFactoryLifecycleStrategy _providerFactoryStrategyMock;
        private ProviderFactoryVendor _providerFactoryVendor;

        public override void SetUp()
        {
            _providerFactoryMock = Substitute.For<IProviderFactory>();
            _providerFactoryStrategyMock = Substitute.For<IProviderFactoryLifecycleStrategy>();
            _providerFactoryStrategyMock.CreateProviderFactory(Arg.Any<Guid>()).Returns(_providerFactoryMock);
            _providerFactoryVendor = new ProviderFactoryVendor(_providerFactoryStrategyMock);
        }

        [Test]
        public void GetProviderFactory_CallsCreateProviderFactoryForSameGuid_UsesProviderFactoryStrategyOnce()
        {
            Guid guid = Guid.NewGuid();

            _providerFactoryVendor.GetProviderFactory(guid);
            _providerFactoryVendor.GetProviderFactory(guid);
            _providerFactoryVendor.GetProviderFactory(guid);

            _providerFactoryStrategyMock.Received(1).CreateProviderFactory(guid);
        }

        [Test]
        public void Dispose_CallsOnReleaseProviderFactoryForAll()
        {
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();

            _providerFactoryVendor.GetProviderFactory(guid1);
            _providerFactoryVendor.GetProviderFactory(guid2);
            _providerFactoryVendor.GetProviderFactory(guid3);

            _providerFactoryVendor.Dispose();

            _providerFactoryStrategyMock.Received().OnReleaseProviderFactory(guid1);
            _providerFactoryStrategyMock.Received().OnReleaseProviderFactory(guid2);
            _providerFactoryStrategyMock.Received().OnReleaseProviderFactory(guid3);
        }
    }
}
