using System;
using System.Diagnostics;
using System.Security.Claims;
using System.ServiceProcess;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using Relativity.API;

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