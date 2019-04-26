using FluentAssertions;
using kCura.Relativity.DataReaderClient;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class BatchProgressHandlerFactoryTests
	{
		[Test]
		public void ItShouldCreateBatchProgressHandler()
		{
			BatchProgressHandlerFactory factory = new BatchProgressHandlerFactory(new EmptyLogger());

			// act
			IBatchProgressHandler batchProgressHandler = factory.CreateBatchProgressHandler(Mock.Of<IBatch>(),
				Mock.Of<IImportNotifier>());

			// assert
			batchProgressHandler.Should().NotBeNull();
		}
	}
}