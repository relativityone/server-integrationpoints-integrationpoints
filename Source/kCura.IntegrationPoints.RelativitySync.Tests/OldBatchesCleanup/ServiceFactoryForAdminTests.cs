using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.Tests.OldBatchesCleanup
{
	[TestFixture, Category("Unit")]
	public class ServiceFactoryForAdminTests
	{
		private ServiceFactoryForAdmin _sut;
		private Mock<IServicesMgr> _servicesMgrFake;

		[SetUp]
		public void SetUp()
		{
			_servicesMgrFake = new Mock<IServicesMgr>();
			_sut = new ServiceFactoryForAdmin(_servicesMgrFake.Object);
		}

		[Test]
		public async Task CreateProxyAsync_ItShouldCreateProxy_WhenClassImplementsIDisposable()
		{
			Mock<IDisposable> disposableObject = new Mock<IDisposable>();
			_servicesMgrFake.Setup(x => x.CreateProxy<IDisposable>(ExecutionIdentity.System)).Returns(disposableObject.Object);

			var result = await _sut.CreateProxyAsync<IDisposable>().ConfigureAwait(false);

			result.Should().Be(disposableObject.Object);
		}
	}
}