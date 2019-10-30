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

		[SetUp]
		public void SetUp()
		{
			var servicesMgrStub = new Mock<IServicesMgr>();
			var helperStub = new Mock<IHelper>();

			helperStub
				.Setup(x => x.GetServicesManager())
				.Returns(servicesMgrStub.Object);

			_sut = new OldBatchesCleanupServiceFactory(helperStub.Object);
		}

		[Test]
		public void ItShouldDeleteBatchesOlderThan7Days()
		{
			// Act
			IOldBatchesCleanupService oldBatchesCleanupService = _sut.Create();

			// Assert
			Assert.IsNotNull(oldBatchesCleanupService, "Factory returned null instance.");
		}
	}
}