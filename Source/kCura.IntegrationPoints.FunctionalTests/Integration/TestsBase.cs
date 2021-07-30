﻿using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.LDAPProvider.Installers;
using kCura.IntegrationPoints.RelativitySync;
using kCura.IntegrationPoints.Synchronizers.RDO.Entity;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.LoadFile;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.WebApi;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.Sync;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using Relativity.Toggles;
using ImportInstaller = kCura.IntegrationPoints.ImportProvider.Parser.Installers.ServicesInstaller;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	[TestExecutionCategory.CI, TestLevel.L1]
	[Feature.DataTransfer.IntegrationPoints]
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

		public void RegisterJobContext(JobTest jobTest)
		{
			Job job = jobTest.AsJob();
			Container.Register(Component.For<Job>().UsingFactoryMethod(k => job).Named(Guid.NewGuid().ToString()).IsDefault());

			Container.Register(Component.For<IJobContextProvider>().UsingFactoryMethod(k =>
			{
				IJobContextProvider jobContextProvider = new FakeJobContextProvider();
				jobContextProvider.StartJobContext(job);

				return jobContextProvider;
			}).Named("TestJobContext").IsDefault());
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

			RegisterRelativityApiServices();

			RegisterScheduleAgentBase();

			Container.Install(new AgentInstaller(Helper, new DefaultScheduleRuleFactory(Container.Resolve<ITimeService>())));
			Container.Install(new QueryInstallers());
			Container.Install(new KeywordInstaller());
			Container.Install(new ServicesInstaller());
			Container.Install(new ValidationInstaller());
			Container.Install(new LdapProviderInstaller());
			Container.Install(new RelativitySyncInstaller());
			Container.Install(new ImportInstaller());

			OverrideRipServicesInstaller();
			OverrideRelativitySyncInstaller();

			RegisterFakeRipServices();
			RegisterRipServices(sourceWorkspace);
		}

		private void OverrideRelativitySyncInstaller()
		{
			Container.Register(Component.For<ISyncOperationsWrapper, IExtendedFakeSyncOperations>()
				.ImplementedBy<FakeSyncOperationsWrapper>()
				.LifestyleSingleton().IsDefault());
		}

		private void OverrideRipServicesInstaller()
		{
			Container.Register(Component.For<IToggleProvider>().ImplementedBy<FakeToggleProviderWithDefaultValue>().IsDefault());
		}

		private void RegisterRelativityApiServices()
		{
			Container.Register(Component.For<IHelper, IAgentHelper, ICPHelper>().Instance(Helper));
			Container.Register(Component.For<IAPILog>().Instance(new ConsoleLogger()).LifestyleSingleton());
		}

		private void RegisterScheduleAgentBase()
		{
			Container.Register(Component.For<ITimeService>().UsingFactoryMethod(() => new FakeTimeService(Context)));
		}
		private void RegisterRipServices(int sourceWorkspaceId)
		{
			Container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(c => new FakeWorkspaceDbContext(SourceWorkspace.ArtifactId, FakeRelativityInstance ))
				.Named(nameof(FakeWorkspaceDbContext)).IsDefault());


			Container.Register(Component.For<IServiceContextHelper>().IsDefault().IsFallback().OverWrite().UsingFactoryMethod(c =>
				new ServiceContextHelperForAgent(c.Resolve<IAgentHelper>(), sourceWorkspaceId)));

			Container.Register(Component.For<IRemovableAgent>().ImplementedBy<FakeNonRemovableAgent>().IsDefault());
			Container.Register(Component.For<IJobService>().ImplementedBy<JobService>());

			Container.Register(Component.For<IJobTrackerQueryManager>().ImplementedBy<FakeJobTrackerQueryManager>()
				.Named(nameof(FakeJobTrackerQueryManager)).IsDefault());
			Container.Register(Component.For<IEntityManagerQueryManager>().ImplementedBy<FakeEntityManagerQueryManager>()
				.Named(nameof(FakeEntityManagerQueryManager)).IsDefault());

			Container.Register(Component.For<IAgentService>().ImplementedBy<AgentService>().UsingFactoryMethod(c =>
				new AgentService(c.Resolve<IHelper>(), c.Resolve<IQueueQueryManager>(), Const.Agent.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)));
			
			Container.Register(Component.For<IJobServiceDataProvider>().ImplementedBy<JobServiceDataProvider>());
			Container.Register(Component.For<IIntegrationPointSerializer>().ImplementedBy<IntegrationPointSerializer>());

			Container.Register(Component.For<ISecretStore>().UsingFactoryMethod(c => c.Resolve<IHelper>().GetSecretStore()).Named(Guid.NewGuid().ToString()).IsDefault());
			Container.Register(Component.For<Lazy<ISecretStore>>().UsingFactoryMethod(c =>
				new Lazy<ISecretStore>(() => c.Resolve<IHelper>().GetSecretStore())).Named(Guid.NewGuid().ToString()).IsDefault());

			Container.Register(Component.For<Job>().UsingFactoryMethod(k =>
			{
				JobTest job = new JobBuilder()
					.WithSubmittedBy(User.ArtifactId)
					.WithWorkspace(SourceWorkspace)
					.Build();
			
				return job.AsJob();
			}).Named(Guid.NewGuid().ToString()).IsDefault());
			
			Container.Register(Component.For<IJobContextProvider>().UsingFactoryMethod(k =>
			{
				Job job = k.Resolve<Job>();
			
				IJobContextProvider jobContextProvider = new FakeJobContextProvider();
				jobContextProvider.StartJobContext(job);
			
				return jobContextProvider;
			}).LifestyleSingleton().Named(nameof(FakeJobContextProvider)).IsDefault());
		}

		private void RegisterFakeRipServices()
		{
			Container.Register(Component.For<IDataProviderFactory>().ImplementedBy<FakeDataProviderFactory>().DependsOn(new { container = Container })
				.Named(nameof(FakeDataProviderFactory)).IsDefault());
			Container.Register(Component.For<IDataSourceProvider>().ImplementedBy<FakeDataSourceProvider>().IsDefault());
			Container.Register(Component.For<IMessageService>().ImplementedBy<FakeMessageService>().IsDefault());
			Container.Register(Component.For<IQueueQueryManager>().ImplementedBy<QueueQueryManagerMock>().IsDefault());
			Container.Register(Component.For<IRepositoryFactory>().UsingFactoryMethod(kernel =>
				new FakeRepositoryFactory(kernel.Resolve<RelativityInstanceTest>(), new RepositoryFactory(kernel.Resolve<IHelper>(), kernel.Resolve<IServicesMgr>()))).IsDefault());
			Container.Register(Component.For<IJobStatisticsQuery>().ImplementedBy<FakeJobStatisticsQuery>().IsDefault());

			// LDAP Entity
			Container.Register(Component.For<IEntityManagerLinksSanitizer>().ImplementedBy<OpenLDAPEntityManagerLinksSanitizer>().IsDefault());

			// IAPI
			Container.Register(Component.For<IImportJobFactory>().ImplementedBy<FakeImportApiJobFactory>().LifestyleTransient().IsDefault());
			Container.Register(Component.For<kCura.IntegrationPoints.Synchronizers.RDO.IImportApiFactory>().ImplementedBy<FakeImportApiFactory>().IsDefault());
			Container.Register(Component.For<kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI.IImportApiFacade>().ImplementedBy<FakeImportApiFacade>().IsDefault());
			Container.Register(Component.For<IWebApiConfig>().UsingFactoryMethod(c => new FakeWebApiConfig()).LifestyleTransient().IsDefault());
			Container.Register(Component.For<IWinEddsBasicLoadFileFactory>().UsingFactoryMethod(c => new FakeWinEddsBasicLoadFileFactory()).LifestyleTransient().IsDefault());
		}
		
		private void SetupGlobalSettings()
		{
			Config.Instance.InstanceSettingsProvider = new FakeInstanceSettingsProvider();
		}
	}
}