using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.LDAPProvider.Installers;
using kCura.IntegrationPoints.RelativitySync;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.Sync;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Telemetry.APM;
using Relativity.Testing.Identification;
using Relativity.Toggles;
using SystemInterface.IO;
using SystemWrapper.IO;
using ImportInstaller = kCura.IntegrationPoints.ImportProvider.Parser.Installers.ServicesInstaller;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	[TestExecutionCategory.CI, TestLevel.L1]
	public abstract class TestsBase
	{
		public RelativityInstanceTest FakeRelativityInstance { get; set; }

		public ProxyMock Proxy { get; set; }

		public TestHelper Helper { get; set; }

		public TestContext Context { get; set; }

		public IWindsorContainer Container { get; set; }

		public WorkspaceTest SourceWorkspace { get; set; }

		public FakeUser User { get; set; }

		public ISerializer Serializer { get; set; }
		
		[SetUp]
		public virtual void SetUp()
		{
			User = new FakeUser
			{
				IsAdmin = true
			};
			
			Context = new TestContext
			{
				User = User
			};
			
			Proxy = new ProxyMock(Context);
			Helper = new TestHelper(Proxy);

			int sourceWorkspaceArtifactId = ArtifactProvider.NextId();

			SetupContainer(sourceWorkspaceArtifactId);
			Serializer = Container.Resolve<ISerializer>();
			
			FakeRelativityInstance = new RelativityInstanceTest(Proxy, Context, Serializer);

			Proxy.Setup(FakeRelativityInstance);
			
			SourceWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspaceWithIntegrationPointsApp(sourceWorkspaceArtifactId);

			SetupGlobalSettings();
		}

		protected virtual WindsorContainer GetContainer()
		{
			return new WindsorContainer();
		}
		
		private void SetupContainer(int sourceWorkspace)
		{
			Container = GetContainer();
			Container.Kernel.Resolver.AddSubResolver(new CollectionResolver(Container.Kernel));

			Container.Register(Component.For<TestContext>().Instance(Context).LifestyleSingleton());
			Container.Register(Component.For<RelativityInstanceTest>().UsingFactoryMethod(() => FakeRelativityInstance).LifestyleSingleton());

			Container.Install(new ValidationInstaller());
			Container.Install(new LdapProviderInstaller());
			Container.Install(new RelativitySyncInstaller());
			Container.Install(new ImportInstaller());

			OverrideRelativitySyncInstaller();

			RegisterRelativityApiServices();
			RegisterFakeRipServices();
			RegisterRipServices(sourceWorkspace);
			RegisterRipAgentTasks();
			RegisterAuthentication();
		}

		private void OverrideRelativitySyncInstaller()
		{
			Container.Register(Component.For<ISyncOperationsWrapper, IExtendedFakeSyncOperations>()
				.ImplementedBy<FakeSyncOperationsWrapper>()
				.LifestyleSingleton().IsDefault());
		}

		private void RegisterRelativityApiServices()
		{
			Container.Register(Component.For<IHelper, IAgentHelper, ICPHelper>().Instance(Helper));
			Container.Register(Component.For<IAPILog>().Instance(new ConsoleLogger()).LifestyleSingleton());
			Container.Register(Component.For<IAPM>().UsingFactoryMethod(k => Client.APMClient, managedExternally: true).LifestyleTransient());
			Container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(c => c.Resolve<IHelper>().GetServicesManager()));
			Container.Register(Component.For<IToggleProvider>().ImplementedBy<FakeToggleProviderWithDefaultValue>());
		}

		private void RegisterRipServices(int sourceWorkspaceId)
		{
			Container.Register(Component.For<IWorkspaceDBContext>().IsDefault().IsFallback().OverWrite().UsingFactoryMethod(c =>
				new FakeWorkspaceDbContext(sourceWorkspaceId))
			);

			Container.Register(Component.For<IServiceContextHelper>().IsDefault().IsFallback().OverWrite().UsingFactoryMethod(c =>
				new ServiceContextHelperForAgent(c.Resolve<IAgentHelper>(), sourceWorkspaceId)));

			Container.Register(Component.For<IRelativityObjectManager>().IsDefault().IsFallback().OverWrite().UsingFactoryMethod(c =>
			{
				IRelativityObjectManagerFactory factory = c.Resolve<IRelativityObjectManagerFactory>();
				return factory.CreateRelativityObjectManager(sourceWorkspaceId);
			}).LifestyleTransient());

			Container.Register(Component.For<RelativitySyncConstrainsChecker>().ImplementedBy<RelativitySyncConstrainsChecker>());
			Container.Register(Component.For<IRemovableAgent>().ImplementedBy<FakeNonRemovableAgent>());
			Container.Register(Component.For<ICaseServiceContext>().ImplementedBy<CaseServiceContext>());
			Container.Register(Component.For<IDataProviderFactory>().ImplementedBy<DataProviderBuilder>());
			Container.Register(Component.For<IJobManager>().ImplementedBy<AgentJobManager>());
			Container.Register(Component.For<IJobService>().ImplementedBy<JobService>());
			
			Container.Register(Component.For<ISerializer>().ImplementedBy<JSONSerializer>().UsingFactoryMethod(c =>
			{
				var serializer = new JSONSerializer();
				IAPILog logger = c.Resolve<IHelper>().GetLoggerFactory().GetLogger();
				return new SerializerWithLogging(serializer, logger);
			}).LifestyleSingleton());
			
			Container.Register(Component.For<IGuidService>().ImplementedBy<DefaultGuidService>());
			Container.Register(Component.For<IJobHistoryErrorService>().ImplementedBy<JobHistoryErrorService>());
			Container.Register(Component.For<ITimeService>().UsingFactoryMethod(() => new FakeTimeService(Context)));
			Container.Register(Component.For<IScheduleRuleFactory>().ImplementedBy<DefaultScheduleRuleFactory>());
			Container.Register(Component.For<IManagerFactory>().ImplementedBy<ManagerFactory>());
			Container.Register(Component.For<IAgentValidator>().ImplementedBy<AgentValidator>());

			Container.Register(Component.For<ProviderFactoryVendor>().ImplementedBy<ProviderFactoryVendor>());
			Container.Register(Component.For<IEddsServiceContext>().ImplementedBy<EddsServiceContext>());
			Container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().UsingFactoryMethod(c =>
				new AgentService(c.Resolve<IHelper>(), c.Resolve<IQueryManager>(), Const.Agent.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)));
			Container.Register(Component.For<IJobServiceDataProvider>().ImplementedBy<JobServiceDataProvider>());
			Container.Register(Component.For<IIntegrationPointSerializer>().ImplementedBy<IntegrationPointSerializer>());

			Container.Register(Component.For<IJobTracker>().ImplementedBy<JobTracker>());
			Container.Register(Component.For<IJobResourceTracker>().ImplementedBy<JobResourceTracker>());
			Container.Register(Component.For<IChoiceQuery>().ImplementedBy<ChoiceQuery>());

			Container.Register(Component.For<IServiceFactory>().ImplementedBy<ServiceFactory>());
			Container.Register(Component.For<IProviderTypeService>().ImplementedBy<ProviderTypeService>());
			Container.Register(Component.For<IIntegrationPointService>().ImplementedBy<IntegrationPointService>()
				.UsingFactoryMethod(c => c.Resolve<IServiceFactory>().CreateIntegrationPointService(c.Resolve<IHelper>())));
			Container.Register(Component.For<IJobHistoryService>().ImplementedBy<JobHistoryService>()
				.UsingFactoryMethod(c => c.Resolve<IServiceFactory>().CreateJobHistoryService(c.Resolve<IAPILog>())));
			Container.Register(Component.For<IRelativityObjectManagerFactory>().ImplementedBy<RelativityObjectManagerFactory>().LifestyleTransient());

			Container.Register(Component.For<IIntegrationPointRepository>().ImplementedBy<IntegrationPointRepository>());

			Container.Register(Component.For<ISynchronizerFactory>().ImplementedBy<GeneralWithEntityRdoSynchronizerFactory>().DependsOn(new { container = Container }).LifestyleTransient());

			Container.Register(Component.For<ISecretsRepository>().ImplementedBy<SecretsRepository>());
			Container.Register(Component.For<ISecretStoreFacade>().ImplementedBy<SecretStoreFacade>());
			Container.Register(Component.For<ISecretStore>().UsingFactoryMethod(c => c.Resolve<IHelper>().GetSecretStore()));
			Container.Register(Component.For<Lazy<ISecretStore>>().UsingFactoryMethod(c =>
				new Lazy<ISecretStore>(() => c.Resolve<IHelper>().GetSecretStore())));

			Container.Register(Component.For<IArtifactService>().ImplementedBy<ArtifactService>());

			Container.Register(Component.For<ITaskParametersBuilder>().ImplementedBy<TaskParametersBuilder>().LifestyleTransient());
			Container.Register(Component.For<IDataTransferLocationService>().ImplementedBy<DataTransferLocationService>().LifestyleTransient());
			Container.Register(Component.For<IDirectory>().ImplementedBy<LongPathDirectory>().LifestyleTransient());
			Container.Register(Component.For<IIntegrationPointTypeService>().ImplementedBy<IntegrationPointTypeService>().LifestyleTransient());
			Container.Register(Component.For<ICryptographyHelper>().ImplementedBy<CryptographyHelper>().LifestyleTransient());
			Container.Register(Component.For<IFileInfoFactory>().ImplementedBy<FileInfoFactory>().LifestyleTransient());

			Container.Register(Component.For<IConfigFactory>().ImplementedBy<ConfigFactory>().LifestyleSingleton());
			Container.Register(Component.For<IServiceManagerProvider>().ImplementedBy<ServiceManagerProvider>().LifestyleTransient());
			Container.Register(Component.For<IProductionManager>().ImplementedBy<ProductionManager>());
			Container.Register(Component.For<IArtifactServiceFactory>().ImplementedBy<ArtifactServiceFactory>());
			Container.Register(Component.For<ISqlServiceFactory>().ImplementedBy<HelperConfigSqlServiceFactory>().LifestyleSingleton());
			Container.Register(Component.For<IRetryHandlerFactory>().ImplementedBy<RetryHandlerFactory>().LifestyleSingleton());
			Container.Register(Component.For<IExternalServiceInstrumentationProvider>().ImplementedBy<ExternalServiceInstrumentationProviderWithJobContext>().LifestyleSingleton());
			Container.Register(Component.For<IConfig>().Instance(Config.Instance).LifestyleSingleton());
			Container.Register(Component.For<IOAuth2ClientFactory>().ImplementedBy<OAuth2ClientFactory>().LifestyleTransient());
			Container.Register(Component.For<ITokenProviderFactoryFactory>().ImplementedBy<TokenProviderFactoryFactory>().LifestyleSingleton());

			Container.Register(Component.For<IJobContextProvider>().UsingFactoryMethod(k =>
			{
				JobTest job = new JobBuilder()
					.WithSubmittedBy(User.ArtifactId)
					.WithWorkspace(SourceWorkspace)
					.Build();

				IJobContextProvider jobContextProvider = new FakeJobContextProvider();
				jobContextProvider.StartJobContext(new Job(job.AsDataRow()));

				return jobContextProvider;
			}).LifestyleSingleton());

			Container.Register(Component.For<CurrentUser>().UsingFactoryMethod(k =>
			{
				IJobContextProvider jobContextProvider = k.Resolve<IJobContextProvider>();
				return new CurrentUser(userID: jobContextProvider.Job.SubmittedBy);
			}).LifestyleTransient());
		}

		private void RegisterFakeRipServices()
		{
			Container.Register(Component.For<IDataSourceProvider>().ImplementedBy<FakeDataSourceProvider>());
			Container.Register(Component.For<IProviderFactory>().ImplementedBy<FakeProviderFactory>());
			Container.Register(Component.For<IProviderFactoryLifecycleStrategy>().ImplementedBy<FakeProviderFactoryLifecycleStrategy>());
			Container.Register(Component.For<IMessageService>().ImplementedBy<FakeMessageService>());
			Container.Register(Component.For<IQueryManager>().ImplementedBy<QueryManagerMock>());
			Container.Register(Component.For<IRepositoryFactory>().UsingFactoryMethod(kernel =>
				new FakeRepositoryFactory(kernel.Resolve<RelativityInstanceTest>(), new RepositoryFactory(kernel.Resolve<IHelper>(), kernel.Resolve<IServicesMgr>()))));

			Container.Register(Component.For<IBatchStatus>().ImplementedBy<FakeBatchStatus>());
		}

		private void RegisterRipAgentTasks()
		{
			Container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifestyleTransient());
			Container.Register(Component.For<ImportServiceManager>().ImplementedBy<ImportServiceManager>().LifestyleTransient());
		}

		private void RegisterAuthentication()
		{
			Container.Register(Component.For<ILoginHelperFacade>().ImplementedBy<LoginHelperRetryDecorator>().LifestyleTransient());
			Container.Register(Component.For<ILoginHelperFacade>().ImplementedBy<LoginHelperInstrumentationDecorator>().LifestyleTransient());
			Container.Register(Component.For<ILoginHelperFacade>().ImplementedBy<LoginHelperFacade>().LifestyleSingleton());

			Container.Register(Component.For<IWebApiLoginService>().ImplementedBy<WebApiLoginService>().LifestyleTransient());

			Container.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<OAuth2TokenGenerator>().LifestyleTransient());
		}

		private void SetupGlobalSettings()
		{
			Config.Instance.InstanceSettingsProvider = new FakeInstanceSettingsProvider();
		}
	}
}
