using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SyncJobFactoryTests
	{
		private SyncJobFactory _instance;

		[SetUp]
		public void SetUp()
		{
			_instance = new SyncJobFactory();
		}

		[Test]
		public void ItShouldCreateSyncJob()
		{
			IContainer container = IntegrationTestsContainerBuilder.CreateContainer();
			SyncJobParameters syncJobParameters = new SyncJobParameters(1, 1);

			// ACT
			ISyncJob job = _instance.Create(container, syncJobParameters);

			// ASSERT
			job.Should().NotBeNull();
		}
	}
}