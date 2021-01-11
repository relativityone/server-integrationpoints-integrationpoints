using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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

		public static TestWorkspace SourceWorkspace { get; private set; }
		public static TestWorkspace DestinationWorkspace { get; private set; }

		private static readonly IList<int> _managedWorkspacesIDs = new List<int>();

		[OneTimeSetUp]
		public static void InitializeFixture()
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
			string sourceWorkspaceName = $"Rip.SystemTests-{DateTime.Now.Ticks}";
			int sourceWorkspaceID = Workspace.CreateWorkspace(
				sourceWorkspaceName,
				WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME);

			SourceWorkspace = new TestWorkspace(sourceWorkspaceID, sourceWorkspaceName);

			string destinationWorkspaceName = $"Rip.SystemTests.Destination-{DateTime.Now.Ticks}";
			int destinationWorkspaceID = Workspace.CreateWorkspace(
				destinationWorkspaceName,
				WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME);

			DestinationWorkspace = new TestWorkspace(destinationWorkspaceID, destinationWorkspaceName);
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
					return new TestServiceContextHelper(helper, SourceWorkspace.ArtifactID);
				}));
			Container.Register(
				Component.For<IWorkspaceDBContext>()
					.ImplementedBy<WorkspaceDBContext>()
					.UsingFactoryMethod(k => new WorkspaceDBContext(k.Resolve<IHelper>().GetDBContext(SourceWorkspace.ArtifactID)))
					.LifeStyle.Transient);
			Container.Register(Component.For<IRelativityObjectManagerService>().Instance(new RelativityObjectManagerService(Container.Resolve<IHelper>(), SourceWorkspace.ArtifactID)).LifestyleTransient());
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

		private static void InitializeRelativityInstanceSettingsClient()
		{
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(TestHelper);
		}

		public static Task<TestWorkspace> CreateManagedWorkspaceWithDefaultName(string templateName = WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME) =>
			CreateManagedWorkspace($"Rip.SystemTests.Managed-{DateTime.Now.Ticks}", templateName);

		public static async Task<TestWorkspace> CreateManagedWorkspace(string workspaceName, string templateName = WorkspaceTemplateNames.FUNCTIONAL_TEMPLATE_NAME)
		{
			int workspaceID = await Workspace.CreateWorkspaceAsync(workspaceName, templateName).ConfigureAwait(false);
			_managedWorkspacesIDs.Add(workspaceID);
			TestWorkspace newWorkspace = new TestWorkspace(workspaceID, workspaceName);
			return newWorkspace;
		}

		[OneTimeTearDown]
		public static void TearDownFixture()
		{
			DeleteSourceAndDestinationWorkspaces();

			foreach (int workspaceId in _managedWorkspacesIDs)
			{
				Workspace.DeleteWorkspace(workspaceId);
			}
		}

		private static void DeleteSourceAndDestinationWorkspaces()
		{
			Workspace.DeleteWorkspace(SourceWorkspace.ArtifactID);
			Workspace.DeleteWorkspace(DestinationWorkspace.ArtifactID);
		}

		public static void Log(string message) =>
			Console.WriteLine($@"[{nameof(SystemTestsSetupFixture)}] {message}");

		public static void ResetFixture(Exception cause = null)
		{
			Log($"Resetting fixture... (Caused by {TestContext.CurrentContext.Test.FullName})");
			if (cause != null)
			{
				Log($"[CAUSE] {cause}");
			}

			var timer = Stopwatch.StartNew();

			DeleteSourceAndDestinationWorkspaces();
			InitializeFixture();

			timer.Stop();
			Log($"Resetting fixture done in {timer.Elapsed.TotalSeconds} seconds");
		}

		public static void InvokeActionsAndResetFixtureOnException(IEnumerable<Action> actions)
		{
			try
			{
				foreach (var action in actions)
				{
					action();
				}
			}
			catch (Exception e)
			{
				ResetFixture(e);
			}
		}
	}
}
