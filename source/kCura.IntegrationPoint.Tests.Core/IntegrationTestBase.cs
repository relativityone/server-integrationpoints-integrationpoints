using System;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
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
			_help = new Lazy<IHelper>(() => new TestHelper(GerronHelper));
		}

		public IHelper Helper => _help.Value;
		private readonly Lazy<IHelper> _help;


		public Helper GerronHelper { get; }
	}
}