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
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;
		private Mock<ISynchronizationConfiguration> _configuration;

		private RelativityExportBatcherFactory _instance;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_configuration = new Mock<ISynchronizationConfiguration>();

			_instance = new RelativityExportBatcherFactory(_serviceFactory.Object, _configuration.Object);
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