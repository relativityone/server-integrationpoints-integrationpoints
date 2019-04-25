using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.Relativity.Client;
using Relativity.API;

namespace Rip.SystemTests
{
	[SetUpFixture]
	public class SystemTestsFixture
	{
		private const string _RELATIVITY_STARTER_TEMPLATE_NAME = "Relativity Starter Template";

		public static IWindsorContainer Container { get; private set; }
		public static IConfigurationStore ConfigurationStore { get; private set; }
		public static ITestHelper TestHelper { get; private set; }

		public static int WorkspaceID { get; private set; }
		public static int DestinationWorkspaceID { get; private set; }

		[OneTimeSetUp]
		public async Task InitializeFixtureAsync()
		{
			Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			TestHelper = new TestHelper();

			await CreateAndConfigureWorkspaces();
			InitializeContainer();
		}

		private static async Task CreateAndConfigureWorkspaces()
		{
			WorkspaceID = Workspace.CreateWorkspace(
				workspaceName: $"Rip.SystemTests-{DateTime.Now.Ticks}",
				templateName: _RELATIVITY_STARTER_TEMPLATE_NAME
			);

			var applicationManager = new RelativityApplicationManager(TestHelper);
			if (SharedVariables.UseIpRapFile())
			{
				await applicationManager.ImportRipToLibraryAsync();
			}

			applicationManager.InstallApplicationFromLibrary(WorkspaceID);

			DestinationWorkspaceID = Workspace.CreateWorkspace(
				workspaceName: $"Rip.SystemTests.Destination-{DateTime.Now.Ticks}",
				templateName: _RELATIVITY_STARTER_TEMPLATE_NAME
			);
		}

		private static void InitializeContainer()
		{
			Container.Register(Component.For<IHelper>().UsingFactoryMethod(k => TestHelper, managedExternally: true));
			Container.Register(Component.For<IAPILog>().UsingFactoryMethod(k => TestHelper.GetLoggerFactory().GetLogger()));
			Container.Register(Component.For<IRsapiClientWithWorkspaceFactory>().ImplementedBy<RsapiClientWithWorkspaceFactory>().LifestyleTransient());
			Container.Register(Component.For<IServiceContextHelper>()
				.UsingFactoryMethod(k =>
				{
					IHelper helper = k.Resolve<IHelper>();
					return new TestServiceContextHelper(helper, WorkspaceID);
				}));
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceContext>()
					.UsingFactoryMethod(k => new WorkspaceContext(k.Resolve<IHelper>().GetDBContext(WorkspaceID)))
					.LifeStyle.Transient);
			Container.Register(
				Component.For<IRSAPIClient>()
					.UsingFactoryMethod(k =>
					{
						IRSAPIClient client = Rsapi.CreateRsapiClient();
						client.APIOptions.WorkspaceID = WorkspaceID;
						return client;
					})
					.LifeStyle.Transient);
			Container.Register(Component.For<IRSAPIService>().Instance(new RSAPIService(Container.Resolve<IHelper>(), WorkspaceID)).LifestyleTransient());
			Container.Register(Component.For<IExporterFactory>().ImplementedBy<ExporterFactory>());
			Container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());
			var dependencies = new IWindsorInstaller[]
			{
				new QueryInstallers(),
				new KeywordInstaller(),
				new SharedAgentInstaller(),
				new ServicesInstaller(),
				new ValidationInstaller()
			};

			foreach (IWindsorInstaller dependency in dependencies)
			{
				dependency.Install(Container, ConfigurationStore);
			}
		}

		[OneTimeTearDown]
		public void TearDownFixture()
		{
			Workspace.DeleteWorkspace(WorkspaceID);
			Workspace.DeleteWorkspace(DestinationWorkspaceID);
		}

	}
}
