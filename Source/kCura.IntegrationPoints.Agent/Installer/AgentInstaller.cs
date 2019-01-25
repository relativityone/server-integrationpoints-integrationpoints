using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.kCura.IntegrationPoints.Agent;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Agent.Validation;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Email;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.Toggles;
using System;
using ITaskFactory = kCura.IntegrationPoints.Agent.TaskFactory.ITaskFactory;

namespace kCura.IntegrationPoints.Agent.Installer
{
	internal class AgentInstaller : IWindsorInstaller
	{
		private readonly IAgentHelper _agentHelper;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;

		public AgentInstaller(IAgentHelper agentHelper, IScheduleRuleFactory scheduleRuleFactory)
		{
			_agentHelper = agentHelper;
			_scheduleRuleFactory = scheduleRuleFactory;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			try
			{
				InstallContainer(container);
			}
			catch (Exception e)
			{
				IAPILog logger = _agentHelper.GetLoggerFactory().GetLogger().ForContext<AgentInstaller>();
				logger.LogError(e, "Unable to install container using AgentInstaller");
				throw;
			}
		}

		private void InstallContainer(IWindsorContainer container)
		{
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
			container.Kernel.AddFacility<TypedFactoryFacility>();

			container.Register(Component.For<JobContextProvider>().Instance(new JobContextProvider()).LifestyleSingleton());

			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod(k =>
			{
				JobContextProvider jobContextProvider = k.Resolve<JobContextProvider>();
				return k.Resolve<IRsapiClientWithWorkspaceFactory>().CreateAdminClient(jobContextProvider.Job.WorkspaceID);

			}).LifestyleTransient());

			//container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>().DependsOn(Dependency.OnValue<int>(_job.WorkspaceID)).LifestyleTransient());
			container.Register(Component.For<IServiceContextHelper>().ImplementedBy<ServiceContextHelperForAgent>().DynamicParameters((k, d) =>
				{
					JobContextProvider jobContextProvider = k.Resolve<JobContextProvider>();
					d.InsertTyped(jobContextProvider.Job.WorkspaceID);
				}).LifestyleTransient());

			container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(k =>
			{
				JobContextProvider jobContextProvider = k.Resolve<JobContextProvider>();
				return new WorkspaceContext(_agentHelper.GetDBContext(jobContextProvider.Job.WorkspaceID));
			}).LifestyleTransient());

			container.Register(Component.For<Job>().UsingFactoryMethod(k =>
			{
				JobContextProvider jobContextProvider = k.Resolve<JobContextProvider>();
				return jobContextProvider.Job;
			}).LifestyleTransient());

			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k =>
			{
				JobContextProvider jobContextProvider = k.Resolve<JobContextProvider>();
				return new RSAPIService(container.Resolve<IHelper>(), jobContextProvider.Job.WorkspaceID);
			}).LifestyleTransient());

			container.Register(Component.For<IDBContext>().UsingFactoryMethod(k =>
			{
				JobContextProvider jobContextProvider = k.Resolve<JobContextProvider>();
				return k.Resolve<IHelper>().GetDBContext(jobContextProvider.Job.WorkspaceID);
			}).LifestyleTransient());

			container.Register(Component.For<CurrentUser>().UsingFactoryMethod(k =>
			{
				JobContextProvider jobContextProvider = k.Resolve<JobContextProvider>();
				return new CurrentUser { ID = jobContextProvider.Job.SubmittedBy };
			}).LifestyleTransient());

			container.Register(Component.For<IScheduleRuleFactory>().UsingFactoryMethod(k => _scheduleRuleFactory, true).LifestyleTransient());
			container.Register(Component.For<IHelper>().UsingFactoryMethod(k => _agentHelper, true).LifestyleTransient());
			container.Register(Component.For<IAgentHelper>().UsingFactoryMethod(k => _agentHelper, true).LifestyleTransient());
			container.Register(Component.For<IRelativityConfigurationFactory>().ImplementedBy<RelativityConfigurationFactory>().LifestyleSingleton());
			container.Register(Component.For<ISendable>().ImplementedBy<SMTP>().DependsOn(Dependency.OnValue<EmailConfiguration>(container.Resolve<IRelativityConfigurationFactory>().GetConfiguration())));
			container.Register(Component.For<ISMTPClientFactory>().ImplementedBy<SMTPClientFactory>().LifestyleTransient());
			container.Register(Component.For<SyncWorker>().ImplementedBy<SyncWorker>().LifestyleTransient());
			container.Register(Component.For<SyncManager>().ImplementedBy<SyncManager>().LifestyleTransient());
			container.Register(Component.For<ExportServiceManager>().ImplementedBy<ExportServiceManager>().LifestyleTransient());
			container.Register(Component.For<ImportServiceManager>().ImplementedBy<ImportServiceManager>().LifestyleTransient());
			container.Register(Component.For<SyncEntityManagerWorker>().ImplementedBy<SyncEntityManagerWorker>().LifestyleTransient());
			container.Register(Component.For<ITaskExceptionService>().ImplementedBy<TaskExceptionService>().LifestyleTransient());
			container.Register(Component.For<ITaskExceptionMediator>().ImplementedBy<TaskExceptionMediator>().LifestyleTransient());
			container.Register(Component.For<SendEmailManager>().ImplementedBy<SendEmailManager>().LifestyleTransient());
			container.Register(Component.For<SendEmailWorker>().ImplementedBy<SendEmailWorker>().LifestyleTransient());
			container.Register(Component.For<ExportManager>().ImplementedBy<ExportManager>().LifestyleTransient());
			container.Register(Component.For<ExportWorker>().ImplementedBy<ExportWorker>().LifestyleTransient());
			container.Register(Component.For<JobHistoryErrorServiceProvider>().ImplementedBy<JobHistoryErrorServiceProvider>().LifeStyle.BoundTo<ExportWorker>());
			container.Register(Component.For<IServiceManagerFactory<ISearchManager>>().ImplementedBy<SearchManagerFactory>().LifestyleSingleton());
			container.Register(Component.For<IAgentValidator>().ImplementedBy<AgentValidator>().LifestyleTransient());

			container.Register(Component.For<IJobSynchronizationChecker>().ImplementedBy<JobSynchronizationChecker>().LifestyleTransient());
			container.Register(Component.For<ITaskFactoryJobHistoryServiceFactory>().ImplementedBy<TaskFactoryJobHistoryServiceFactory>().LifestyleTransient());
			container.Register(Component.For<ITaskFactory>().ImplementedBy<TaskFactory.TaskFactory>().DependsOn(new { container }).LifestyleTransient());
			container.Register(Component.For<IConfigurationDeserializer>().ImplementedBy<ConfigurationDeserializer>().LifestyleSingleton());
			container.Register(Component.For<RelativitySyncConstrainsChecker>().ImplementedBy<RelativitySyncConstrainsChecker>().LifestyleSingleton());

			container.Register(Component.For<IAuthTokenGenerator>().UsingFactoryMethod(kernel =>
			{
				var helper = kernel.Resolve<IHelper>();
				var oauth2ClientFactory = kernel.Resolve<IOAuth2ClientFactory>();
				var tokenProviderFactory = kernel.Resolve<ITokenProviderFactoryFactory>();
				var contextUser = kernel.Resolve<CurrentUser>();

				return new OAuth2TokenGenerator(helper, oauth2ClientFactory, tokenProviderFactory, contextUser);
			}).LifestyleTransient());

			// TODO: yea, we need a better way of getting the target IRepositoryFactory to the IExporterFactory -- biedrzycki: Sept 1, 2016
			container.Register(Component.For<global::kCura.IntegrationPoints.Core.Factories.IExporterFactory>().UsingFactoryMethod(
				k =>
				{
					IOnBehalfOfUserClaimsPrincipalFactory claimsPrincipalFactory =
						k.Resolve<IOnBehalfOfUserClaimsPrincipalFactory>();
					IRepositoryFactory sourceRepositoryFactory = k.Resolve<IRepositoryFactory>();
					JobContextProvider jobContextProvider = k.Resolve<JobContextProvider>();
					int integrationPointId = jobContextProvider.Job.RelatedObjectArtifactID;
					ICaseServiceContext caseServiceContext = k.Resolve<ICaseServiceContext>();
					IntegrationPoint integrationPoint = caseServiceContext.RsapiService.RelativityObjectManager.Read<IntegrationPoint>(integrationPointId);
					if (integrationPoint == null)
					{
						throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
					}

					ISerializer serializer = k.Resolve<ISerializer>();
					ImportSettings importSettings = serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

					IRepositoryFactory targetRepositoryFactory = null;
					IHelper sourceHelper = k.Resolve<IHelper>();
					if (importSettings.IsFederatedInstance())
					{
						IHelperFactory helperFactory = k.Resolve<IHelperFactory>();
						IHelper targetHelper = helperFactory.CreateTargetHelper(sourceHelper, importSettings.FederatedInstanceArtifactId, integrationPoint.SecuredConfiguration);
						targetRepositoryFactory = new RepositoryFactory(sourceHelper, targetHelper.GetServicesManager());
					}
					else
					{
						targetRepositoryFactory = sourceRepositoryFactory;
					}
					IFederatedInstanceManager federatedInstanceManager = k.Resolve<IFederatedInstanceManager>();
					IFolderPathReaderFactory folderPathReaderFactory = k.Resolve<IFolderPathReaderFactory>();
					IToggleProvider toggleProvider = k.Resolve<IToggleProvider>();
					return new global::kCura.IntegrationPoints.Core.Factories.Implementations.ExporterFactory(claimsPrincipalFactory,
						sourceRepositoryFactory, targetRepositoryFactory, sourceHelper, federatedInstanceManager, folderPathReaderFactory,
						toggleProvider);
				}).LifestyleTransient());

			container.Register(Component.For<IRsapiClientWithWorkspaceFactory>().ImplementedBy<ExtendedRsapiClientWithWorkspaceFactory>().LifestyleTransient());
			container.Register(Component.For<IExternalServiceInstrumentationProvider>()
				.ImplementedBy<ExternalServiceInstrumentationProviderWithJobContext>()
				.LifestyleSingleton());
		}
	}
}