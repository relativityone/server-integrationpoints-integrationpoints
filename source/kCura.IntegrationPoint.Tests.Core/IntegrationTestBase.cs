using System;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.Apps.Common.Data;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class IntegrationTestBase
	{
		protected IWindsorContainer Container;
		protected IConfigurationStore ConfigurationStore;
		protected IntegrationTestBase()
		{
			Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			GerronHelper = new Helper();
			_help = new Lazy<ITestHelper>(() => new TestHelper(GerronHelper));
		}

		public ITestHelper Helper => _help.Value;
		private readonly Lazy<ITestHelper> _help;


		public Helper GerronHelper { get; }
	}
}