using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Core.Installers.Registrations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using kCura.IntegrationPoints.EventHandlers.Commands.Metrics;
using kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity;
using kCura.IntegrationPoints.EventHandlers.Context;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;
using kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup;
using kCura.IntegrationPoints.RelativitySync.RdoCleanup;
using kCura.IntegrationPoints.Security;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
	internal class EventHandlerInstaller : IWindsorInstaller
	{
		private readonly IEHContext _context;

		public EventHandlerInstaller(IEHContext context)
		{
			_context = context;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Install(new InfrastructureInstaller());

			InstallDependencies(container);
		}

		private void InstallDependencies(IWindsorContainer container)
		{
			container.Register(Component.For<IEHContext>().Instance(_context).LifestyleSingleton());
			container.Register(Component.For<IHelper, IEHHelper>().Instance(_context.Helper).LifestyleSingleton());

			container.Register(Component.For<IServiceContextHelper>().UsingFactoryMethod(k =>
			{
				IEHContext context = k.Resolve<IEHContext>();
				return new ServiceContextHelperForEventHandlers(context.Helper, context.Helper.GetActiveCaseID());
			}).LifestyleSingleton());

			container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(k =>
			{
				IEHContext context = k.Resolve<IEHContext>();
				return new WorkspaceDBContext(context.Helper.GetDBContext(context.Helper.GetActiveCaseID()));
			}).LifestyleTransient());

			container.Register(Component.For<IWorkspaceContext>()
				.ImplementedBy<EventHandlerWorkspaceContextService>()
				.LifestyleTransient()
			);

			container.Register(Component.For<IIntegrationPointProviderValidator>().UsingFactoryMethod(k =>
					new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), k.Resolve<IIntegrationPointSerializer>(),  k.Resolve<IRelativityObjectManagerFactory>()))
				.LifestyleSingleton());

			container.Register(Component.For<IIntegrationPointPermissionValidator>().UsingFactoryMethod(k =>
				new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(),
					k.Resolve<IIntegrationPointSerializer>())).LifestyleSingleton());

			container.Register(Component.For<IValidationExecutor>().ImplementedBy<ValidationExecutor>().LifestyleSingleton());

			container.Register(Component.For<IAuthTokenGenerator>().UsingFactoryMethod(kernel =>
			{
				IEHContext context = kernel.Resolve<IEHContext>();
				IOAuth2ClientFactory oauth2ClientFactory = kernel.Resolve<IOAuth2ClientFactory>();
				ITokenProviderFactoryFactory tokenProviderFactory = kernel.Resolve<ITokenProviderFactoryFactory>();
				int userID = context.Helper.GetAuthenticationManager().UserInfo.ArtifactID;
				var contextUser = new CurrentUser(userID);

				return new OAuth2TokenGenerator(context.Helper, oauth2ClientFactory, tokenProviderFactory, contextUser);
			}).LifestyleTransient());

			container.Register(Component.For<IErrorService>().ImplementedBy<EhErrorService>().LifestyleTransient());

			container.Register(Component.For<IEncryptionManager>().ImplementedBy<DefaultEncryptionManager>().LifestyleSingleton());
			container.Register(Component.For<DeleteIntegrationPointCommand>().ImplementedBy<DeleteIntegrationPointCommand>().LifestyleTransient());
			container.Register(Component.For<PreCascadeDeleteIntegrationPointCommand>().ImplementedBy<PreCascadeDeleteIntegrationPointCommand>().LifestyleTransient());
			container.Register(Component.For<RemoveAgentJobLogTableCommand>().ImplementedBy<RemoveAgentJobLogTableCommand>().LifestyleTransient());
			container.Register(Component.For<ImportNativeFileCopyModeUpdater>().ImplementedBy<ImportNativeFileCopyModeUpdater>().LifestyleTransient());
			container.Register(Component.For<SetImportNativeFileCopyModeCommand>().ImplementedBy<SetImportNativeFileCopyModeCommand>().LifestyleTransient());
			container.Register(Component.For<UpdateLdapConfigurationCommand>().ImplementedBy<UpdateLdapConfigurationCommand>().LifestyleTransient());
			container.Register(Component.For<IRemoveSecuredConfigurationFromIntegrationPointService>().ImplementedBy<RemoveSecuredConfigurationFromIntegrationPointService>().LifestyleSingleton());
			container.Register(Component.For<ISplitJsonObjectService>().ImplementedBy<SplitJsonObjectService>().LifestyleSingleton());
			container.Register(Component.For<UpdateRelativityConfigurationCommand>().ImplementedBy<UpdateRelativityConfigurationCommand>().LifestyleTransient());
			container.Register(Component.For<UpdateFtpConfigurationCommand>().ImplementedBy<UpdateFtpConfigurationCommand>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointSecretDelete>().UsingFactoryMethod(k => IntegrationPointSecretDeleteFactory.Create(k.Resolve<IEHContext>().Helper))
				.LifestyleTransient());
			container.Register(Component.For<ICorrespondingJobDelete>().ImplementedBy<CorrespondingJobDelete>().LifestyleTransient());
			container.Register(Component.For<IPreCascadeDeleteEventHandlerValidator>().ImplementedBy<PreCascadeDeleteEventHandlerValidator>().LifestyleTransient());
			container.Register(Component.For<IArtifactsToDelete>().ImplementedBy<ArtifactsToDelete>().LifestyleTransient());
			container.Register(Component.For<RenameCustodianToEntityInIntegrationPointConfigurationCommand>().ImplementedBy<RenameCustodianToEntityInIntegrationPointConfigurationCommand>().LifestyleTransient());
			container.Register(Component.For<MigrateSecretCatalogPathToSecretStorePathCommand>().ImplementedBy<MigrateSecretCatalogPathToSecretStorePathCommand>().LifestyleTransient());

			container.Register(Component.For<IOldBatchesCleanupServiceFactory>().ImplementedBy<OldBatchesCleanupServiceFactory>().LifestyleTransient());
			container.Register(Component.For<IOldBatchesCleanupService>().UsingFactoryMethod(c => c.Resolve<IOldBatchesCleanupServiceFactory>().Create()).LifestyleTransient());
			container.Register(Component.For<RemoveBatchesFromOldJobsCommand>().ImplementedBy<RemoveBatchesFromOldJobsCommand>().LifestyleTransient());

			container.Register(Component.For<ISyncRdoCleanupService>().ImplementedBy<SyncRdoCleanupService>().LifestyleTransient());
			container.Register(Component.For<SyncRdoDeleteCommand>().ImplementedBy<SyncRdoDeleteCommand>().LifestyleTransient());

			container.Register(Component.For<IRemovableAgent>().ImplementedBy<FakeNonRemovableAgent>().LifestyleTransient());

			container.Register(Component.For<RegisterScheduleJobSumMetricsCommand>().ImplementedBy<RegisterScheduleJobSumMetricsCommand>().LifestyleTransient());
			
			container.AddSecretStoreMigrator();
		}
    }
}
