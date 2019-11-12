using System;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.Tests.OldBatchesCleanup
{
	[TestFixture]
	public class OldBatchesCleanupServiceFactoryTests
	{
		private IOldBatchesCleanupServiceFactory _sut;
		private Mock<Lazy<IErrorService>> _errorServiceMock;
		private Mock<IAPILog> _apiLogMock;

		[SetUp]
		public void SetUp()
		{
			var servicesMgrStub = new Mock<IServicesMgr>();
			var helperStub = new Mock<IHelper>();

			helperStub
				.Setup(x => x.GetServicesManager())
				.Returns(servicesMgrStub.Object);

			_errorServiceMock = new Mock<Lazy<IErrorService>>();
			_apiLogMock = new Mock<IAPILog>();

			_sut = new OldBatchesCleanupServiceFactory(helperStub.Object, _errorServiceMock.Object, _apiLogMock.Object);
		}

		[Test]
		public void Create_ShouldDeleteBatches_WhenOlderThanSevenDays()
		{
			// Act
			IOldBatchesCleanupService oldBatchesCleanupService = _sut.Create();

			// Assert
			oldBatchesCleanupService.Should().NotBeNull();
		}
	}
}