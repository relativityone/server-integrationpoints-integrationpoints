using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public abstract class TestsBase
	{
		public InMemoryDatabase Database { get; set; }

		public ProxyMock Proxy { get; set; }

		public TestHelper Helper { get; set; }

		public TestContext Context { get; set; }

		public IWindsorContainer Container { get; set; }

		public HelperManager HelperManager { get; set; }

		protected TestsBase()
		{
			Database = new InMemoryDatabase();

			Proxy = new ProxyMock(Database);

			Helper = new TestHelper(Proxy);

			Context = new TestContext();

			HelperManager = new HelperManager(Database, Proxy);

			SetupContainer();
		}

		public void SetupContainer()
		{
			Container = new WindsorContainer();

			Container.Register(Component.For<TestContext>().Instance(Context).LifestyleSingleton());
			Container.Register(Component.For<InMemoryDatabase>().Instance(Database).LifestyleSingleton());

			Container.Register(Component.For<IHelper, IAgentHelper>().Instance(Helper));

			Container.Register(Component.For<IQueryManager>().ImplementedBy<QueryManagerMock>());
		}

		[TearDown]
		public void TearDown()
		{
			Database.Clear();
			Proxy.Clear();

			Context.SetDateTime(null);
		}
	}
}
