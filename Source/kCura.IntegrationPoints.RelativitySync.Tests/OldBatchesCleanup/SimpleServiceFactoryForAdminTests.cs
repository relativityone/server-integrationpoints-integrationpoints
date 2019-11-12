using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.Tests.OldBatchesCleanup
{
	[TestFixture]
	public class SimpleServiceFactoryForAdminTests
	{
		private SimpleServiceFactoryForAdmin _sut;
		private Mock<IServicesMgr> _servicesMgrMock;

		[SetUp]
		public void SetUp()
		{
			_servicesMgrMock = new Mock<IServicesMgr>();
			_sut = new SimpleServiceFactoryForAdmin(_servicesMgrMock.Object);
		}

		[Test]
		public async Task CreateProxyAsync_ItShouldCreateProxy_WhenClassImplementsIDisposable()
		{
			ClassImplementsIDisposable classImplementsIDisposable = new ClassImplementsIDisposable();
			_servicesMgrMock.Setup(x => x.CreateProxy<IDisposable>(ExecutionIdentity.System)).Returns(classImplementsIDisposable);

			var result = await _sut.CreateProxyAsync<IDisposable>().ConfigureAwait(false);

			result.Should().Be(classImplementsIDisposable);
		}
	}

	class ClassImplementsIDisposable : IDisposable
	{
		public void Dispose()
		{ 
		}
	}
}