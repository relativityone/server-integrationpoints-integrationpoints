using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class NotificationExecutorTests
	{
		[SetUp]
		public void SetUp()
		{
			ContainerHelper.CreateContainer(cb =>
			{
				IntegrationTestsContainerBuilder.MockStepsExcept<INotificationConfiguration>(cb);
			});
		}
	}
}