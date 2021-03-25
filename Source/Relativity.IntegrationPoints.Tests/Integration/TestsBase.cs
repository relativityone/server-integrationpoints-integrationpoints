using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public abstract class TestsBase
	{
		public InMemoryDatabase Database { get; set; }

		public ProxyMock Proxy { get; set; }

		public TestHelper Helper { get; set; }

		public TestContext Context { get; set; }

		public IWindsorContainer Container { get; set; }

		public HelperManager HelperManager { get; set; }

		public WorkspaceTest SourceWorkspace { get; set; }
		
		[SetUp]
		public virtual void SetUp()
		{
			Proxy = new ProxyMock();

			Database = new InMemoryDatabase(Proxy);

			Helper = new TestHelper(Proxy);

			Context = new TestContext();

			HelperManager = new HelperManager(Database, Proxy, Context);

			SetupGlobalSettings();

			SourceWorkspace = HelperManager.WorkspaceHelper.CreateWorkspace();

			SetupContainer(SourceWorkspace);
		}
		
		private void SetupContainer(WorkspaceTest sourceWorkspace)
		{
			Container = new WindsorContainer();
			Container.Kernel.Resolver.AddSubResolver(new CollectionResolver(Container.Kernel));

			Container.Register(Component.For<TestContext>().Instance(Context).LifestyleSingleton());
			Container.Register(Component.For<InMemoryDatabase>().Instance(Database).LifestyleSingleton());

			RegisterRelativityApiServices();
			RegisterFakeRipServices();
			RegisterRipServices(sourceWorkspace);
			RegisterRipAgentTasks();
			RegisterValidators();
		}

		private void RegisterRelativityApiServices()
		{
			Container.Register(Component.For<IHelper, IAgentHelper>().Instance(Helper));
			Container.Register(Component.For<IAPILog>().Instance(new ConsoleLogger()).LifestyleSingleton());
			Container.Register(Component.For<IServicesMgr>().UsingFactoryMethod(c => c.Resolve<IHelper>().GetServicesManager()));
		}

		private void RegisterRipServices(WorkspaceTest sourceWorkspace)
		{
			Container.Register(Component.For<IWorkspaceDBContext>().IsDefault().IsFallback().OverWrite().UsingFactoryMethod(c =>
				new FakeWorkspaceDbContext(sourceWorkspace.ArtifactId))
			);

			Container.Register(Component.For<IServiceContextHelper>().IsDefault().IsFallback().OverWrite().UsingFactoryMethod(c =>
				new ServiceContextHelperForAgent(c.Resolve<IAgentHelper>(), sourceWorkspace.ArtifactId)));

			Container.Register(Component.For<IRelativityObjectManager>().IsDefault().IsFallback().OverWrite().UsingFactoryMethod(c =>
			{
				IRelativityObjectManagerFactory factory = c.Resolve<IRelativityObjectManagerFactory>();
				return factory.CreateRelativityObjectManager(sourceWorkspace.ArtifactId);
			}).LifestyleTransient());

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
			Container.Register(Component.For<IValidationExecutor>().ImplementedBy<ValidationExecutor>());
			Container.Register(Component.For<IIntegrationPointProviderValidator>().ImplementedBy<IntegrationPointProviderValidator>());
			Container.Register(Component.For<IIntegrationPointPermissionValidator>().ImplementedBy<IntegrationPointPermissionValidator>());

			Container.Register(Component.For<ISecretsRepository>().ImplementedBy<SecretsRepository>());
			Container.Register(Component.For<ISecretStoreFacade>().ImplementedBy<SecretStoreFacade>());
			Container.Register(Component.For<ISecretStore>().UsingFactoryMethod(c => c.Resolve<IHelper>().GetSecretStore()));
			Container.Register(Component.For<Lazy<ISecretStore>>().UsingFactoryMethod(c =>
				new Lazy<ISecretStore>(() => c.Resolve<IHelper>().GetSecretStore())));

			Container.Register(Component.For<IArtifactService>().ImplementedBy<ArtifactService>());
		}

		private void RegisterFakeRipServices()
		{
			Container.Register(Component.For<IDataSourceProvider>().ImplementedBy<FakeDataSourceProvider>());
			Container.Register(Component.For<IProviderFactory>().ImplementedBy<FakeProviderFactory>());
			Container.Register(Component.For<IProviderFactoryLifecycleStrategy>().ImplementedBy<FakeProviderFactoryLifecycleStrategy>());
			Container.Register(Component.For<IMessageService>().ImplementedBy<FakeMessageService>());
			Container.Register(Component.For<IQueryManager>().ImplementedBy<QueryManagerMock>());
			Container.Register(Component.For<IRepositoryFactory>().UsingFactoryMethod(kernel =>
				new FakeRepositoryFactory(kernel.Resolve<InMemoryDatabase>(), new RepositoryFactory(kernel.Resolve<IHelper>(), kernel.Resolve<IServicesMgr>()))));

			Container.Register(Component.For<IBatchStatus>().ImplementedBy<FakeBatchStatus>());
			Container.Register(Component.For<IValidator>().ImplementedBy<FakeValidator>());
			Container.Register(Component.For<IPermissionValidator>().ImplementedBy<FakePermissionValidator>());
		}

		private void RegisterRipAgentTasks()
		{
			Container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifestyleTransient());
		}

		private void RegisterValidators()
		{
		}

		private void SetupGlobalSettings()
		{
			Config.Instance.InstanceSettingsProvider = new FakeInstanceSettingsProvider();
		}
	}
}
