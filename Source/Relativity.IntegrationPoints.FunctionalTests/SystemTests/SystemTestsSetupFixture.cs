using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Constants;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;

namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests
{
	[SetUpFixture]
	public class SystemTestsSetupFixture
	{
		public static IWindsorContainer Container { get; private set; }
		public static IConfigurationStore ConfigurationStore { get; private set; }
		public static ITestHelper TestHelper { get; private set; }

		public static int WorkspaceID { get; private set; }
		public static string WorkspaceName { get; private set; }
		public static int DestinationWorkspaceID { get; private set; }
		public static string DestinationWorkspaceName { get; private set; }

		[OneTimeSetUp]
		public void InitializeFixtureAsync()
		{
			Container = new WindsorContainer();
			ConfigurationStore = new DefaultConfigurationStore();
			TestHelper = new TestHelper();

			CreateAndConfigureWorkspaces();
			InitializeContainer();

			InitializeRelativityInstanceSettingsClient();
		}
		
		private static void CreateAndConfigureWorkspaces()
		{
			WorkspaceName= $"Rip.SystemTests-{DateTime.Now.Ticks}";
			WorkspaceID = Workspace.CreateWorkspace(
				workspaceName: WorkspaceName,
				templateName: WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME
			);

			DestinationWorkspaceName = $"Rip.SystemTests.Destination-{DateTime.Now.Ticks}";
			DestinationWorkspaceID = Workspace.CreateWorkspace(
				workspaceName: DestinationWorkspaceName,
				templateName: WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME
			);
		}

		private static void InitializeContainer()
		{
			Container.Register(Component
				.For<ILazyComponentLoader>()
				.ImplementedBy<LazyOfTComponentLoader>()
			);
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
					.ImplementedBy<WorkspaceDBContext>()
					.UsingFactoryMethod(k => new WorkspaceDBContext(k.Resolve<IHelper>().GetDBContext(WorkspaceID)))
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
			Container.Register(Component.For<IExportServiceObserversFactory>().ImplementedBy<IExportServiceObserversFactory>());
			Container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());
			Container.Register(Component.For<IExternalServiceInstrumentationProvider>()
				.ImplementedBy<ExternalServiceInstrumentationProviderWithoutJobContext>()
				.LifestyleSingleton());
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

		private void InitializeRelativityInstanceSettingsClient()
		{
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(TestHelper);
		}

		[OneTimeTearDown]
		public void TearDownFixture()
		{
			Workspace.DeleteWorkspace(WorkspaceID);
			Workspace.DeleteWorkspace(DestinationWorkspaceID);
		}

	}
}
