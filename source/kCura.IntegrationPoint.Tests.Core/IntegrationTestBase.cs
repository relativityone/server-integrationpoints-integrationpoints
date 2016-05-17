using System;
using System.Collections.Generic;
using System.Security.Claims;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Contexts;
using NSubstitute;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class IntegrationTestBase
	{
		protected IWindsorContainer Container;
		protected IConfigurationStore ConfigurationStore;
		protected IntegrationTestBase()
		{
			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				OnBehalfOfUserClaimsPrincipalFactory factory = new OnBehalfOfUserClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal(9);
			};

			Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			_help = new Lazy<ITestHelper>(() => new TestHelper());
		}

		public ITestHelper Helper => _help.Value;
		private readonly Lazy<ITestHelper> _help;
	}
}