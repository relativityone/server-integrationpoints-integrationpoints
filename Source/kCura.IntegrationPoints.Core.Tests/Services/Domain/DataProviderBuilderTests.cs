using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Services.Domain;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Domain
{
	[TestFixture]
	public class DataProviderBuilderTests : TestBase
	{
		private IProviderFactory _providerFactory;
		private ProviderFactoryVendor _providerFactoryVendorMock;
		private DataProviderBuilder _dataProviderBuilder;

		public override void SetUp()
		{
			_providerFactory = Substitute.For<IProviderFactory>();
			_providerFactoryVendorMock = Substitute.For<ProviderFactoryVendor>();
			_providerFactoryVendorMock.GetProviderFactory(Arg.Any<Guid>()).Returns(_providerFactory);
			_dataProviderBuilder = new DataProviderBuilder(_providerFactoryVendorMock);
		}

		[Test]
		public void GetDataProvider_CallsAppropriateMethods()
		{
			Guid applicationGuid = Guid.NewGuid();
			Guid providerGuid = Guid.NewGuid();

			_dataProviderBuilder.GetDataProvider(applicationGuid, providerGuid);

			_providerFactoryVendorMock.Received().GetProviderFactory(applicationGuid);
			_providerFactory.Received().CreateProvider(providerGuid);
		}
	}
}
