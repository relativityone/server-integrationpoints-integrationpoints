using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class RelativityExportBatcherFactoryTests
	{
		private RelativityExportBatcherFactory _instance;

		[SetUp]
		public void SetUp()
		{
			Mock<ISourceServiceFactoryForUser> serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			Mock<ISynchronizationConfiguration> configuration = new Mock<ISynchronizationConfiguration>();

			_instance = new RelativityExportBatcherFactory(serviceFactoryForUser.Object, configuration.Object);
		}

		[Test]
		public void ItShouldCreateRelativityExportBatcher()
		{
			// act
			IRelativityExportBatcher batcher = _instance.CreateRelativityExportBatcher(Mock.Of<IBatch>());

			// assert
			batcher.Should().NotBeNull();
		}
	}
}