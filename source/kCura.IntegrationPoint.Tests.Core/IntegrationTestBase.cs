using System;
using System.Security.Claims;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class IntegrationTestBase
	{
		protected IWindsorContainer Container;
		protected IConfigurationStore ConfigurationStore;
		public ITestHelper Helper => _help.Value;
		private readonly Lazy<ITestHelper> _help;
		private int _ADMIN_USER_ID = 9;

		protected IntegrationTestBase()
		{
			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				ClaimsPrincipalFactory factory = new ClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal2(_ADMIN_USER_ID);
			};

			Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			_help = new Lazy<ITestHelper>(() => new TestHelper());
		}
	}
}