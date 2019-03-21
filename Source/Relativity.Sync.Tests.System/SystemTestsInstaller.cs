using Autofac;
using Relativity.API;
using Relativity.Sync.Tests.System.Stubs;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	/// <summary>
	///     Installer for mocks/stubs around external dependencies that we can't/won't
	///     reference during integration tests.
	/// </summary>
	internal sealed class SystemTestsInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			// Relativity.API
			builder.RegisterType<ServicesManagerStub>().As<IServicesMgr>();
			builder.RegisterType<ProvideServiceUrisStub>().As<IProvideServiceUris>();
		}
	}
}