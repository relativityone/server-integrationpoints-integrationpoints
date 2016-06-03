using System;
using System.Diagnostics;
using System.Security.Claims;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Contexts;

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

		protected void IISReset()
		{
			Process process = Process.Start(@"C:\Windows\System32\iisreset.exe");
			process?.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds);
		}
	}
}